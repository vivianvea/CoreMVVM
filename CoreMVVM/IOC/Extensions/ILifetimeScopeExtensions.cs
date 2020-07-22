﻿using System;

namespace CoreMVVM.IOC
{
    /// <summary>
    /// Extension methods for getting services from an System.IServiceProvider.
    /// </summary>
    public static class ILifetimeScopeExtensions
    {
        /// <summary>
        /// Get service of type serviceType from the <see cref="ILifetimeScope"/>.
        /// </summary>
        /// <exception cref="ArgumentNullException">provider or serviceType is null.</exception>
        /// <exception cref="ResolveUnregisteredServiceException">no service of type serviceType exist.</exception>
        public static object ResolveRequiredService(this IServiceProvider provider, Type serviceType)
        {
            var service = ResolveService(provider, serviceType);
            if (service is null)
                throw new ResolveUnregisteredServiceException($"No service for type '{serviceType}' has been registered.");

            return service;
        }

        /// <summary>
        /// Get service of type T from the <see cref="ILifetimeScope"/>.
        /// </summary>
        /// <exception cref="ArgumentNullException">provider is null.</exception>
        /// <exception cref="ResolveUnregisteredServiceException">no service of type serviceType exist.</exception>
        public static T ResolveRequiredService<T>(this IServiceProvider provider)
            where T : class
        {
            return (T)ResolveRequiredService(provider, typeof(T));
        }

        /// <summary>
        /// Get service of type serviceType from the <see cref="ILifetimeScope"/>.
        /// </summary>
        /// <exception cref="ArgumentNullException">provider or serviceType is null.</exception>
        /// <returns>The resolved service, or null if none exist.</returns>
        public static object ResolveService(this IServiceProvider provider, Type serviceType)
        {
            if (provider is null) throw new ArgumentNullException(nameof(provider));
            if (serviceType is null) throw new ArgumentNullException(nameof(serviceType));

            return provider.GetService(serviceType);
        }

        /// <summary>
        /// Get service of type T from the <see cref="ILifetimeScope"/>.
        /// </summary>
        /// <returns>The resolved service, or null if none exist.</returns>
        /// <exception cref="ArgumentNullException">provider is null.</exception>
        public static T ResolveService<T>(this IServiceProvider provider)
            where T : class
        {
            return (T)ResolveService(provider, typeof(T));
        }
    }
}