﻿using CoreMVVM.Extentions;
using CoreMVVM.IOC.Builder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CoreMVVM.IOC.Core
{
    internal class LifetimeScope : ILifetimeScope
    {
        private readonly IReadOnlyDictionary<Type, IRegistration> _registeredTypes;
        private readonly ICollection<IDisposable> _disposables = new List<IDisposable>();
        private readonly LifetimeScope _parent;

        private readonly object _disposeLock = new object();

        public LifetimeScope(IReadOnlyDictionary<Type, IRegistration> registeredTypes)
        {
            _registeredTypes = registeredTypes;
        }

        public LifetimeScope(IReadOnlyDictionary<Type, IRegistration> registeredTypes, LifetimeScope parent)
        {
            _registeredTypes = registeredTypes;
            _parent = parent;
        }

        #region Properties

        public bool IsDisposed { get; private set; }

        /// <summary>
        /// Gets a collection of resolved instances with limited scoping.
        /// </summary>
        public Dictionary<IRegistration, object> ResolvedInstances { get; } = new Dictionary<IRegistration, object>();

        #endregion Properties

        #region Methods

        /// <summary>
        /// Returns an instance from the given type.
        /// </summary>
        /// <typeparam name="T">The type to get an instance for.</typeparam>
        /// <exception cref="ResolveUnregisteredInterfaceException">T is an unregistered or resolves to an interface.</exception>
        /// <exception cref="ResolveConstructionException">Fails to construct type or one of its arguments.</exception>
        public T Resolve<T>() => (T)Resolve(typeof(T), registerDisposable: true);

        /// <summary>
        /// Returns an instance from the given type.
        /// </summary>
        /// <param name="type">The type to get an instance for.</param>
        /// <exception cref="ResolveUnregisteredInterfaceException">type is an unregistered or resolves to an interface.</exception>
        /// <exception cref="ResolveConstructionException">Fails to construct type or one of its arguments.</exception>
        public object Resolve(Type type) => Resolve(type, registerDisposable: true);

        /// <summary>
        /// Creates a new lifetime scope.
        /// </summary>
        /// <returns>A new lifetime scope.</returns>
        public ILifetimeScope BeginLifetimeScope()
        {
            if (IsDisposed)
                throw new ObjectDisposedException(nameof(ILifetimeScope));

            ILifetimeScope childScope = new LifetimeScope(_registeredTypes, this);
            _disposables.Add(childScope);

            return childScope;
        }

        public void Dispose()
        {
            if (IsDisposed)
                return;

            lock (_disposeLock)
            {
                if (IsDisposed)
                    return;

                IsDisposed = true;
                foreach (IDisposable disposable in _disposables)
                    disposable.Dispose();

                _disposables.Clear();
            }
        }

        #endregion Methods

        #region Private resolve

        private object Resolve(Type type, bool registerDisposable)
        {
            if (IsDisposed)
                throw new ObjectDisposedException(nameof(ILifetimeScope));

            bool isRegistered = _registeredTypes.TryGetValue(type, out IRegistration registration);
            if (isRegistered)
            {
                if (registration.Scope == InstanceScope.None)
                    return ConstructFromRegistration(registration, registerDisposable);

                // Singletons should only be resolved by root.
                if (registration.Scope == InstanceScope.Singleton && _parent != null)
                    return _parent.Resolve(type, registerDisposable);

                // Result from scoped components are saved for future resolves.
                lock (registration)
                {
                    if (!ResolvedInstances.ContainsKey(registration))
                        ResolvedInstances[registration] = ConstructFromRegistration(registration, registerDisposable);

                    return ResolvedInstances[registration];
                }
            }

            return ConstructType(type, registerDisposable);
        }

        #endregion Private resolve

        #region Construct methods

        private object ConstructFromRegistration(IRegistration registration, bool registerDisposable)
        {
            if (registration.Factory != null)
            {
                object instance = registration.Factory(this);

                if (registerDisposable)
                    RegisterDisposable(instance);

                return instance;
            }
            else
                return ConstructType(registration.Type, registerDisposable);
        }

        /// <summary>
        /// Constructs an instance of the given type, using the constructor with the most parameters.
        /// </summary>
        /// <exception cref="ResolveUnregisteredInterfaceException">type is an interface.</exception>
        /// <exception cref="ResolveConstructionException">Fails to construct type.</exception>
        private object ConstructType(Type type, bool registerDisposable)
        {
            // Switch out any IOwned<> (or implementation) with Owned<>
            bool isOwned = typeof(IOwned<>).IsAssignableFromGeneric(type);
            if (isOwned)
                type = typeof(Owned<>).MakeGenericType(type.GenericTypeArguments);

            if (type.IsInterface)
                throw new ResolveUnregisteredInterfaceException($"Expected class or struct, recieved interface '{type}'.");

            try
            {
                ConstructorInfo[] constructors = type.GetConstructors();
                if (constructors.Length == 0)
                    return type.GetDefault();

                ConstructorInfo constructor = constructors
                    .OrderByDescending(c => c.GetParameters().Length)
                    .First();

                object[] args = constructor.GetParameters()
                                           .Select(param => Resolve(param.ParameterType, isOwned))
                                           .ToArray();

                object instance = constructor.Invoke(args);

                if (registerDisposable)
                    RegisterDisposable(instance);

                return instance;
            }
            catch (Exception e)
            {
                string message = $"Failed to construct instance of type '{type}'.";

                Resolve<ILogger>().Exception(message, e);
                throw new ResolveConstructionException(message, e);
            }
        }

        private void RegisterDisposable(object instance)
        {
            if (instance is IDisposable disposable)
                _disposables.Add(disposable);
        }

        #endregion Construct methods
    }
}