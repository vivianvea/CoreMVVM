﻿using CoreMVVM.IOC.Core;
using System;

namespace CoreMVVM.IOC.Builder
{
    /// <summary>
    /// For building a container.
    /// </summary>
    public class ContainerBuilder
    {
        private readonly RegistrationCollection _registrations = new RegistrationCollection();

        /// <summary>
        /// Creates a new container builder with default registrations.
        /// </summary>
        public ContainerBuilder() : this(registerDefaults: true)
        {
        }

        /// <summary>
        /// Creates a new container builder.
        /// </summary>
        /// <param name="registerDefaults">Indicated if default registrations should be performed. See remarks.</param>
        /// <remarks>
        /// Defauls registrations include:
        /// - <see cref="ILogger"/> as <see cref="ConsoleLogger"/>. A logger is required to use the resulting container.
        /// </remarks>
        public ContainerBuilder(bool registerDefaults)
        {
            if (registerDefaults)
            {
                RegisterSingleton<ConsoleLogger>().As<ILogger>();
            }
        }

        /// <summary>
        /// Registers a component.
        /// </summary>
        /// <typeparam name="T">The type of the component to register.</typeparam>
        /// <remarks>No registration occurs by calling this method, the component must be registered using the returned builder.</remarks>
        public RegistrationBuilder Register<T>()
        {
            return Register(typeof(T));
        }

        /// <summary>
        /// Registers a component.
        /// </summary>
        /// <param name="type">The type of the component to register.</param>
        /// <remarks>No registration occurs by calling this method, the component must be registered using the returned builder.</remarks>
        public RegistrationBuilder Register(Type type)
        {
            return new RegistrationBuilder(_registrations, type);
        }

        /// <summary>
        /// Registers a component as a singleton.
        /// </summary>
        /// <typeparam name="T">The type of the component to register.</typeparam>
        /// <remarks>No registration occurs by calling this method, the component must be registered using the returned builder.</remarks>
        public RegistrationBuilder RegisterSingleton<T>()
        {
            return RegisterSingleton(typeof(T));
        }

        /// <summary>
        /// Registers a component as a singleton.
        /// </summary>
        /// <param name="type">The type of the component to register.</param>
        /// <remarks>No registration occurs by calling this method, the component must be registered using the returned builder.</remarks>
        public RegistrationBuilder RegisterSingleton(Type type)
        {
            return new RegistrationBuilder(_registrations, type, isSingleton: true);
        }

        /// <summary>
        /// Constructs a container with all the registered components and services.
        /// </summary>
        public IContainer Build()
        {
            // Registers the container as a singleton, so it always resolves to this instance.
            RegisterSingleton<Container>().As<IContainer>().AsSelf();

            IContainer container = new Container(_registrations);

            // And set this instance as the last created one, so this is the one that's returned upon a call to IContainer.Resolve().
            _registrations[typeof(IContainer)].LastCreatedInstance = container;

            return container;
        }
    }
}