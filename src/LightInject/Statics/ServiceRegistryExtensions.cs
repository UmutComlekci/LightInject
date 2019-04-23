namespace LightInject
{
    using System;

   /// <summary>
    /// Extends the <see cref="IServiceRegistry"/> interface with a
    /// set of convenience methods for registering services.
    /// </summary>
    public static class ServiceRegistryExtensions
    {
        /// <summary>
        /// Registers the <paramref name="serviceType"/> using the non-generic <paramref name="factory"/> to resolve the instance.
        /// </summary>
        /// <param name="serviceRegistry">The target <see cref="IServiceRegistry"/>.</param>
        /// <param name="serviceType">The type of service to register.</param>
        /// <param name="factory">The factory used to resolve the instance.</param>
        /// <returns>The <see cref="IServiceRegistry"/>, for chaining calls.</returns>
        public static IServiceRegistry Register(this IServiceRegistry serviceRegistry, Type serviceType, Func<IServiceFactory, object> factory)
            => Register(serviceRegistry, serviceType, factory, string.Empty, null);

        /// <summary>
        /// Registers the <paramref name="serviceType"/> with a given <paramref name="lifetime"/> using the non-generic <paramref name="factory"/> to resolve the instance.
        /// </summary>
        /// <param name="serviceRegistry">The target <see cref="IServiceRegistry"/>.</param>
        /// <param name="serviceType">The type of service to register.</param>
        /// <param name="factory">The factory used to resolve the instance.</param>
        /// <param name="lifetime">The <see cref="ILifetime"/> used to control the lifetime of the service.</param>
        /// <returns>The <see cref="IServiceRegistry"/>, for chaining calls.</returns>
        public static IServiceRegistry Register(this IServiceRegistry serviceRegistry, Type serviceType, Func<IServiceFactory, object> factory, ILifetime lifetime)
            => Register(serviceRegistry, serviceType, factory, string.Empty, lifetime);

        /// <summary>
        /// Registers the <paramref name="serviceType"/> using the non-generic <paramref name="factory"/> to resolve the instance.
        /// </summary>
        /// <param name="serviceRegistry">The target <see cref="IServiceRegistry"/>.</param>
        /// <param name="serviceType">The type of service to register.</param>
        /// <param name="factory">The factory used to resolve the instance.</param>
        /// <param name="serviceName">The name the service to register.</param>
        /// <returns>The <see cref="IServiceRegistry"/>, for chaining calls.</returns>
        public static IServiceRegistry Register(this IServiceRegistry serviceRegistry, Type serviceType, Func<IServiceFactory, object> factory, string serviceName)
            => Register(serviceRegistry, serviceType, factory, serviceName, null);

        /// <summary>
        /// Registers the <paramref name="serviceType"/> with a given <paramref name="lifetime"/> using the non-generic <paramref name="factory"/> to resolve the instance.
        /// </summary>
        /// <param name="serviceRegistry">The target <see cref="IServiceRegistry"/>.</param>
        /// <param name="serviceType">The type of service to register.</param>
        /// <param name="factory">The factory used to resolve the instance.</param>
        /// <param name="serviceName">The name the service to register.</param>
        /// <param name="lifetime">The <see cref="ILifetime"/> used to control the lifetime of the service.</param>
        /// <returns>The <see cref="IServiceRegistry"/>, for chaining calls.</returns>
        public static IServiceRegistry Register(this IServiceRegistry serviceRegistry, Type serviceType, Func<IServiceFactory, object> factory, string serviceName, ILifetime lifetime)
        {
            var serviceRegistration = new ServiceRegistration
            {
                FactoryExpression = factory,
                ServiceType = serviceType,
                ServiceName = serviceName,
                Lifetime = lifetime,
            };
            return serviceRegistry.Register(serviceRegistration);
        }

        /// <summary>
        /// Registers a singleton <paramref name="serviceType"/> using the non-generic <paramref name="factory"/> to resolve the instance.
        /// </summary>
        /// <param name="serviceRegistry">The target <see cref="IServiceRegistry"/>.</param>
        /// <param name="serviceType">The type of service to register.</param>
        /// <param name="factory">The factory used to resolve the instance.</param>
        /// <returns>The <see cref="IServiceRegistry"/>, for chaining calls.</returns>
        public static IServiceRegistry RegisterSingleton(this IServiceRegistry serviceRegistry, Type serviceType, Func<IServiceFactory, object> factory)
        {
            return serviceRegistry.RegisterSingleton(serviceType, factory, string.Empty);
        }

        /// <summary>
        /// Registers a singleton <paramref name="serviceType"/> using the non-generic <paramref name="factory"/> to resolve the instance.
        /// </summary>
        /// <param name="serviceRegistry">The target <see cref="IServiceRegistry"/>.</param>
        /// <param name="serviceType">The type of service to register.</param>
        /// <param name="factory">The factory used to resolve the instance.</param>
        /// <param name="serviceName">The name the service to register.</param>
        /// <returns>The <see cref="IServiceRegistry"/>, for chaining calls.</returns>
        public static IServiceRegistry RegisterSingleton(this IServiceRegistry serviceRegistry, Type serviceType, Func<IServiceFactory, object> factory, string serviceName)
        {
            return serviceRegistry.Register(serviceType, factory, serviceName, new PerContainerLifetime());
        }

        /// <summary>
        /// Registers a scoped <paramref name="serviceType"/> using the non-generic <paramref name="factory"/> to resolve the instance.
        /// </summary>
        /// <param name="serviceRegistry">The target <see cref="IServiceRegistry"/>.</param>
        /// <param name="serviceType">The type of service to register.</param>
        /// <param name="factory">The factory used to resolve the instance.</param>
        /// <returns>The <see cref="IServiceRegistry"/>, for chaining calls.</returns>
        public static IServiceRegistry RegisterScoped(this IServiceRegistry serviceRegistry, Type serviceType, Func<IServiceFactory, object> factory)
        {
            return serviceRegistry.RegisterScoped(serviceType, factory, string.Empty);
        }

        /// <summary>
        /// Registers a scoped <paramref name="serviceType"/> using the non-generic <paramref name="factory"/> to resolve the instance.
        /// </summary>
        /// <param name="serviceRegistry">The target <see cref="IServiceRegistry"/>.</param>
        /// <param name="serviceType">The type of service to register.</param>
        /// <param name="factory">The factory used to resolve the instance.</param>
        /// <param name="serviceName">The name the service to register.</param>
        /// <returns>The <see cref="IServiceRegistry"/>, for chaining calls.</returns>
        public static IServiceRegistry RegisterScoped(this IServiceRegistry serviceRegistry, Type serviceType, Func<IServiceFactory, object> factory, string serviceName)
        {
            return serviceRegistry.Register(serviceType, factory, serviceName, new PerScopeLifetime());
        }

        /// <summary>
        /// Registers a transient <paramref name="serviceType"/> using the non-generic <paramref name="factory"/> to resolve the instance.
        /// </summary>
        /// <param name="serviceRegistry">The target <see cref="IServiceRegistry"/>.</param>
        /// <param name="serviceType">The type of service to register.</param>
        /// <param name="factory">The factory used to resolve the instance.</param>
        /// <returns>The <see cref="IServiceRegistry"/>, for chaining calls.</returns>
        public static IServiceRegistry RegisterTransient(this IServiceRegistry serviceRegistry, Type serviceType, Func<IServiceFactory, object> factory)
        {
            return serviceRegistry.RegisterTransient(serviceType, factory, string.Empty);
        }

        /// <summary>
        /// Registers a transient <paramref name="serviceType"/> using the non-generic <paramref name="factory"/> to resolve the instance.
        /// </summary>
        /// <param name="serviceRegistry">The target <see cref="IServiceRegistry"/>.</param>
        /// <param name="serviceType">The type of service to register.</param>
        /// <param name="factory">The factory used to resolve the instance.</param>
        /// <param name="serviceName">The name the service to register.</param>
        /// <returns>The <see cref="IServiceRegistry"/>, for chaining calls.</returns>
        public static IServiceRegistry RegisterTransient(this IServiceRegistry serviceRegistry, Type serviceType, Func<IServiceFactory, object> factory, string serviceName)
        {
            return serviceRegistry.Register(serviceType, factory, serviceName);
        }

        /// <summary>
        /// Registers a singleton service of type <typeparamref name="TService"/> with an implementing type of <typeparamref name="TImplementation"/>.
        /// </summary>
        /// <param name="serviceRegistry">The target <see cref="IServiceRegistry"/>.</param>
        /// <typeparam name="TService">The type of service to register.</typeparam>
        /// <typeparam name="TImplementation">The type implementing the service type.</typeparam>
        /// <returns>The <see cref="IServiceRegistry"/>, for chaining calls.</returns>
        public static IServiceRegistry RegisterSingleton<TService, TImplementation>(this IServiceRegistry serviceRegistry)
            where TImplementation : TService
            => serviceRegistry.Register<TService, TImplementation>(new PerContainerLifetime());

        /// <summary>
        /// Registers a singleton service of type <typeparamref name="TService"/> as a concrete service.
        /// </summary>
        /// <param name="serviceRegistry">The target <see cref="IServiceRegistry"/>.</param>
        /// <typeparam name="TService">The type of service to register.</typeparam>
        /// <returns>The <see cref="IServiceRegistry"/>, for chaining calls.</returns>
        public static IServiceRegistry RegisterSingleton<TService>(this IServiceRegistry serviceRegistry)
            => serviceRegistry.Register<TService>(new PerContainerLifetime());

        /// <summary>
        /// Registers a singleton service of type <typeparamref name="TService"/> with an implementing type of <typeparamref name="TImplementation"/>.
        /// </summary>
        /// <param name="serviceRegistry">The target <see cref="IServiceRegistry"/>.</param>
        /// <param name="serviceName">The name of the service to register.</param>
        /// <typeparam name="TService">The type of service to register.</typeparam>
        /// <typeparam name="TImplementation">The type implementing the service type.</typeparam>
        /// <returns>The <see cref="IServiceRegistry"/>, for chaining calls.</returns>
        public static IServiceRegistry RegisterSingleton<TService, TImplementation>(this IServiceRegistry serviceRegistry, string serviceName)
            where TImplementation : TService
            => serviceRegistry.Register<TService, TImplementation>(serviceName, new PerContainerLifetime());

        /// <summary>
        /// Registers a singleton service of type <paramref name="serviceType"/> with an implementing type of <paramref name="implementingType"/>.
        /// </summary>
        /// <param name="serviceRegistry">The target <see cref="IServiceRegistry"/>.</param>
        /// <param name="serviceType">The type of service to register.</param>
        /// <param name="implementingType">The type implementing the service type.</param>
        /// <returns>The <see cref="IServiceRegistry"/>, for chaining calls.</returns>
        public static IServiceRegistry RegisterSingleton(this IServiceRegistry serviceRegistry, Type serviceType, Type implementingType)
            => serviceRegistry.Register(serviceType, implementingType, new PerContainerLifetime());

        /// <summary>
        /// Registers a singleton service of type <paramref name="serviceType"/> as a concrete service type.
        /// </summary>
        /// <param name="serviceRegistry">The target <see cref="IServiceRegistry"/>.</param>
        /// <param name="serviceType">The type of service to register.</param>
        /// <returns>The <see cref="IServiceRegistry"/>, for chaining calls.</returns>
        public static IServiceRegistry RegisterSingleton(this IServiceRegistry serviceRegistry, Type serviceType)
            => serviceRegistry.Register(serviceType, new PerContainerLifetime());

        /// <summary>
        /// Registers a singleton service of type <paramref name="serviceType"/> with an implementing type of <paramref name="implementingType"/>.
        /// </summary>
        /// <param name="serviceRegistry">The target <see cref="IServiceRegistry"/>.</param>
        /// <param name="serviceType">The type of service to register.</param>
        /// <param name="implementingType">The type implementing the service type.</param>
        /// <param name="serviceName">The name of the service to register.</param>
        /// <returns>The <see cref="IServiceRegistry"/>, for chaining calls.</returns>
        public static IServiceRegistry RegisterSingleton(this IServiceRegistry serviceRegistry, Type serviceType, Type implementingType, string serviceName)
            => serviceRegistry.Register(serviceType, implementingType, serviceName, new PerContainerLifetime());

        /// <summary>
        /// Registers a singleton service of type <typeparamref name="TService"/> using a factory function.
        /// </summary>
        /// <param name="serviceRegistry">The target <see cref="IServiceRegistry"/>.</param>
        /// <param name="factory">The factory function used to create the service instance.</param>
        /// <typeparam name="TService">The type of service to register.</typeparam>
        /// <returns>The <see cref="IServiceRegistry"/>, for chaining calls.</returns>
        public static IServiceRegistry RegisterSingleton<TService>(this IServiceRegistry serviceRegistry, Func<IServiceFactory, TService> factory)
            => serviceRegistry.Register<TService>(factory, new PerContainerLifetime());

        /// <summary>
        /// Registers a singleton service of type <typeparamref name="TService"/> using a factory function.
        /// </summary>
        /// <param name="serviceRegistry">The target <see cref="IServiceRegistry"/>.</param>
        /// <param name="factory">The factory function used to create the service instance.</param>
        /// <param name="serviceName">The name of the service to register.</param>
        /// <typeparam name="TService">The type of service to register.</typeparam>
        /// <returns>The <see cref="IServiceRegistry"/>, for chaining calls.</returns>
        public static IServiceRegistry RegisterSingleton<TService>(this IServiceRegistry serviceRegistry, Func<IServiceFactory, TService> factory, string serviceName)
            => serviceRegistry.Register<TService>(factory, serviceName, new PerContainerLifetime());

        /// <summary>
        /// Registers a scoped service of type <typeparamref name="TService"/> with an implementing type of <typeparamref name="TImplementation"/>.
        /// </summary>
        /// <param name="serviceRegistry">The target <see cref="IServiceRegistry"/>.</param>
        /// <typeparam name="TService">The type of service to register.</typeparam>
        /// <typeparam name="TImplementation">The type implementing the service type.</typeparam>
        /// <returns>The <see cref="IServiceRegistry"/>, for chaining calls.</returns>
        public static IServiceRegistry RegisterScoped<TService, TImplementation>(this IServiceRegistry serviceRegistry)
            where TImplementation : TService
            => serviceRegistry.Register<TService, TImplementation>(new PerScopeLifetime());

        /// <summary>
        /// Registers a scoped service of type <typeparamref name="TService"/> as a concrete service.
        /// </summary>
        /// <param name="serviceRegistry">The target <see cref="IServiceRegistry"/>.</param>
        /// <typeparam name="TService">The type of service to register.</typeparam>
        /// <returns>The <see cref="IServiceRegistry"/>, for chaining calls.</returns>
        public static IServiceRegistry RegisterScoped<TService>(this IServiceRegistry serviceRegistry)
            => serviceRegistry.Register<TService>(new PerScopeLifetime());

        /// <summary>
        /// Registers a scoped service of type <typeparamref name="TService"/> with an implementing type of <typeparamref name="TImplementation"/>.
        /// </summary>
        /// <param name="serviceRegistry">The target <see cref="IServiceRegistry"/>.</param>
        /// <param name="serviceName">The name of the service to register.</param>
        /// <typeparam name="TService">The type of service to register.</typeparam>
        /// <typeparam name="TImplementation">The type implementing the service type.</typeparam>
        /// <returns>The <see cref="IServiceRegistry"/>, for chaining calls.</returns>
        public static IServiceRegistry RegisterScoped<TService, TImplementation>(this IServiceRegistry serviceRegistry, string serviceName)
            where TImplementation : TService
            => serviceRegistry.Register<TService, TImplementation>(serviceName, new PerScopeLifetime());

        /// <summary>
        /// Registers a scoped service of type <paramref name="serviceType"/> with an implementing type of <paramref name="implementingType"/>.
        /// </summary>
        /// <param name="serviceRegistry">The target <see cref="IServiceRegistry"/>.</param>
        /// <param name="serviceType">The type of service to register.</param>
        /// <param name="implementingType">The type implementing the service type.</param>
        /// <returns>The <see cref="IServiceRegistry"/>, for chaining calls.</returns>
        public static IServiceRegistry RegisterScoped(this IServiceRegistry serviceRegistry, Type serviceType, Type implementingType)
            => serviceRegistry.Register(serviceType, implementingType, new PerScopeLifetime());

        /// <summary>
        /// Registers a scoped service of type <paramref name="serviceType"/> as a concrete service type.
        /// </summary>
        /// <param name="serviceRegistry">The target <see cref="IServiceRegistry"/>.</param>
        /// <param name="serviceType">The type of service to register.</param>
        /// <returns>The <see cref="IServiceRegistry"/>, for chaining calls.</returns>
        public static IServiceRegistry RegisterScoped(this IServiceRegistry serviceRegistry, Type serviceType)
            => serviceRegistry.Register(serviceType, new PerScopeLifetime());

        /// <summary>
        /// Registers a scoped service of type <paramref name="serviceType"/> with an implementing type of <paramref name="implementingType"/>.
        /// </summary>
        /// <param name="serviceRegistry">The target <see cref="IServiceRegistry"/>.</param>
        /// <param name="serviceType">The type of service to register.</param>
        /// <param name="implementingType">The type implementing the service type.</param>
        /// <param name="serviceName">The name of the service to register.</param>
        /// <returns>The <see cref="IServiceRegistry"/>, for chaining calls.</returns>
        public static IServiceRegistry RegisterScoped(this IServiceRegistry serviceRegistry, Type serviceType, Type implementingType, string serviceName)
            => serviceRegistry.Register(serviceType, implementingType, serviceName, new PerScopeLifetime());

        /// <summary>
        /// Registers a scoped service of type <typeparamref name="TService"/> using a factory function.
        /// </summary>
        /// <param name="serviceRegistry">The target <see cref="IServiceRegistry"/>.</param>
        /// <param name="factory">The factory function used to create the service instance.</param>
        /// <typeparam name="TService">The type of service to register.</typeparam>
        /// <returns>The <see cref="IServiceRegistry"/>, for chaining calls.</returns>
        public static IServiceRegistry RegisterScoped<TService>(this IServiceRegistry serviceRegistry, Func<IServiceFactory, TService> factory)
            => serviceRegistry.Register<TService>(factory, new PerScopeLifetime());

        /// <summary>
        /// Registers a scoped service of type <typeparamref name="TService"/> using a factory function.
        /// </summary>
        /// <param name="serviceRegistry">The target <see cref="IServiceRegistry"/>.</param>
        /// <param name="factory">The factory function used to create the service instance.</param>
        /// <param name="serviceName">The name of the service to register.</param>
        /// <typeparam name="TService">The type of service to register.</typeparam>
        /// <returns>The <see cref="IServiceRegistry"/>, for chaining calls.</returns>
        public static IServiceRegistry RegisterScoped<TService>(this IServiceRegistry serviceRegistry, Func<IServiceFactory, TService> factory, string serviceName)
            => serviceRegistry.Register<TService>(factory, serviceName, new PerScopeLifetime());

        /// <summary>
        /// Registers a transient service of type <typeparamref name="TService"/> with an implementing type of <typeparamref name="TImplementation"/>.
        /// </summary>
        /// <param name="serviceRegistry">The target <see cref="IServiceRegistry"/>.</param>
        /// <typeparam name="TService">The type of service to register.</typeparam>
        /// <typeparam name="TImplementation">The type implementing the service type.</typeparam>
        /// <returns>The <see cref="IServiceRegistry"/>, for chaining calls.</returns>
        public static IServiceRegistry RegisterTransient<TService, TImplementation>(this IServiceRegistry serviceRegistry)
            where TImplementation : TService
            => serviceRegistry.Register<TService, TImplementation>();

        /// <summary>
        /// Registers a transient service of type <typeparamref name="TService"/> as a concrete service.
        /// </summary>
        /// <param name="serviceRegistry">The target <see cref="IServiceRegistry"/>.</param>
        /// <typeparam name="TService">The type of service to register.</typeparam>
        /// <returns>The <see cref="IServiceRegistry"/>, for chaining calls.</returns>
        public static IServiceRegistry RegisterTransient<TService>(this IServiceRegistry serviceRegistry)
            => serviceRegistry.Register<TService>();

        /// <summary>
        /// Registers a transient service of type <typeparamref name="TService"/> with an implementing type of <typeparamref name="TImplementation"/>.
        /// </summary>
        /// <param name="serviceRegistry">The target <see cref="IServiceRegistry"/>.</param>
        /// <param name="serviceName">The name of the service to register.</param>
        /// <typeparam name="TService">The type of service to register.</typeparam>
        /// <typeparam name="TImplementation">The type implementing the service type.</typeparam>
        /// <returns>The <see cref="IServiceRegistry"/>, for chaining calls.</returns>
        public static IServiceRegistry RegisterTransient<TService, TImplementation>(this IServiceRegistry serviceRegistry, string serviceName)
            where TImplementation : TService
            => serviceRegistry.Register<TService, TImplementation>(serviceName);

        /// <summary>
        /// Registers a transient service of type <paramref name="serviceType"/> with an implementing type of <paramref name="implementingType"/>.
        /// </summary>
        /// <param name="serviceRegistry">The target <see cref="IServiceRegistry"/>.</param>
        /// <param name="serviceType">The type of service to register.</param>
        /// <param name="implementingType">The type implementing the service type.</param>
        /// <returns>The <see cref="IServiceRegistry"/>, for chaining calls.</returns>
        public static IServiceRegistry RegisterTransient(this IServiceRegistry serviceRegistry, Type serviceType, Type implementingType)
            => serviceRegistry.Register(serviceType, implementingType);

        /// <summary>
        /// Registers a singleton service of type <paramref name="serviceType"/> as a concrete service type.
        /// </summary>
        /// <param name="serviceRegistry">The target <see cref="IServiceRegistry"/>.</param>
        /// <param name="serviceType">The type of service to register.</param>
        /// <returns>The <see cref="IServiceRegistry"/>, for chaining calls.</returns>
        public static IServiceRegistry RegisterTransient(this IServiceRegistry serviceRegistry, Type serviceType)
            => serviceRegistry.Register(serviceType);

        /// <summary>
        /// Registers a transient service of type <paramref name="serviceType"/> with an implementing type of <paramref name="implementingType"/>.
        /// </summary>
        /// <param name="serviceRegistry">The target <see cref="IServiceRegistry"/>.</param>
        /// <param name="serviceType">The type of service to register.</param>
        /// <param name="implementingType">The type implementing the service type.</param>
        /// <param name="serviceName">The name of the service to register.</param>
        /// <returns>The <see cref="IServiceRegistry"/>, for chaining calls.</returns>
        public static IServiceRegistry RegisterTransient(this IServiceRegistry serviceRegistry, Type serviceType, Type implementingType, string serviceName)
            => serviceRegistry.Register(serviceType, implementingType, serviceName);

        /// <summary>
        /// Registers a transient service of type <typeparamref name="TService"/> using a factory function.
        /// </summary>
        /// <param name="serviceRegistry">The target <see cref="IServiceRegistry"/>.</param>
        /// <param name="factory">The factory function used to create the service instance.</param>
        /// <typeparam name="TService">The type of service to register.</typeparam>
        /// <returns>The <see cref="IServiceRegistry"/>, for chaining calls.</returns>
        public static IServiceRegistry RegisterTransient<TService>(this IServiceRegistry serviceRegistry, Func<IServiceFactory, TService> factory)
            => serviceRegistry.Register<TService>(factory);

        /// <summary>
        /// Registers a transient service of type <typeparamref name="TService"/> using a factory function.
        /// </summary>
        /// <param name="serviceRegistry">The target <see cref="IServiceRegistry"/>.</param>
        /// <param name="factory">The factory function used to create the service instance.</param>
        /// <param name="serviceName">The name of the service to register.</param>
        /// <typeparam name="TService">The type of service to register.</typeparam>
        /// <returns>The <see cref="IServiceRegistry"/>, for chaining calls.</returns>
        public static IServiceRegistry RegisterTransient<TService>(this IServiceRegistry serviceRegistry, Func<IServiceFactory, TService> factory, string serviceName)
            => serviceRegistry.Register<TService>(factory, serviceName);
    }
}