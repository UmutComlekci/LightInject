namespace LightInject
{
    /// <summary>
    /// Defines a set of methods used to register services into the service container.
    /// </summary>
    public interface IServiceRegistry
    {
        /// <summary>
        /// Gets a list of <see cref="ServiceRegistration"/> instances that represents the
        /// registered services.
        /// </summary>
        IEnumerable<ServiceRegistration> AvailableServices { get; }

        /// <summary>
        /// Registers the <paramref name="serviceType"/> with the <paramref name="implementingType"/>.
        /// </summary>
        /// <param name="serviceType">The service type to register.</param>
        /// <param name="implementingType">The implementing type.</param>
        /// <returns>The <see cref="IServiceRegistry"/>, for chaining calls.</returns>
        IServiceRegistry Register(Type serviceType, Type implementingType);

        /// <summary>
        /// Registers the <paramref name="serviceType"/> with the <paramref name="implementingType"/>.
        /// </summary>
        /// <param name="serviceType">The service type to register.</param>
        /// <param name="implementingType">The implementing type.</param>
        /// <param name="lifetime">The <see cref="ILifetime"/> instance that controls the lifetime of the registered service.</param>
        /// <returns>The <see cref="IServiceRegistry"/>, for chaining calls.</returns>
        IServiceRegistry Register(Type serviceType, Type implementingType, ILifetime lifetime);

        /// <summary>
        /// Registers the <paramref name="serviceType"/> with the <paramref name="implementingType"/>.
        /// </summary>
        /// <param name="serviceType">The service type to register.</param>
        /// <param name="implementingType">The implementing type.</param>
        /// <param name="serviceName">The name of the service.</param>
        /// <returns>The <see cref="IServiceRegistry"/>, for chaining calls.</returns>
        IServiceRegistry Register(Type serviceType, Type implementingType, string serviceName);

        /// <summary>
        /// Registers the <paramref name="serviceType"/> with the <paramref name="implementingType"/>.
        /// </summary>
        /// <param name="serviceType">The service type to register.</param>
        /// <param name="implementingType">The implementing type.</param>
        /// <param name="serviceName">The name of the service.</param>
        /// <param name="lifetime">The <see cref="ILifetime"/> instance that controls the lifetime of the registered service.</param>
        /// <returns>The <see cref="IServiceRegistry"/>, for chaining calls.</returns>
        IServiceRegistry Register(Type serviceType, Type implementingType, string serviceName, ILifetime lifetime);

        /// <summary>
        /// Registers the <typeparamref name="TService"/> with the <typeparamref name="TImplementation"/>.
        /// </summary>
        /// <typeparam name="TService">The service type to register.</typeparam>
        /// <typeparam name="TImplementation">The implementing type.</typeparam>
        /// <returns>The <see cref="IServiceRegistry"/>, for chaining calls.</returns>
        IServiceRegistry Register<TService, TImplementation>()
            where TImplementation : TService;

        /// <summary>
        /// Registers the <typeparamref name="TService"/> with the <typeparamref name="TImplementation"/>.
        /// </summary>
        /// <typeparam name="TService">The service type to register.</typeparam>
        /// <typeparam name="TImplementation">The implementing type.</typeparam>
        /// <param name="lifetime">The <see cref="ILifetime"/> instance that controls the lifetime of the registered service.</param>
        /// <returns>The <see cref="IServiceRegistry"/>, for chaining calls.</returns>
        IServiceRegistry Register<TService, TImplementation>(ILifetime lifetime)
            where TImplementation : TService;

        /// <summary>
        /// Registers the <typeparamref name="TService"/> with the <typeparamref name="TImplementation"/>.
        /// </summary>
        /// <typeparam name="TService">The service type to register.</typeparam>
        /// <typeparam name="TImplementation">The implementing type.</typeparam>
        /// <param name="serviceName">The name of the service.</param>
        /// <returns>The <see cref="IServiceRegistry"/>, for chaining calls.</returns>
        IServiceRegistry Register<TService, TImplementation>(string serviceName)
            where TImplementation : TService;

        /// <summary>
        /// Registers the <typeparamref name="TService"/> with the <typeparamref name="TImplementation"/>.
        /// </summary>
        /// <typeparam name="TService">The service type to register.</typeparam>
        /// <typeparam name="TImplementation">The implementing type.</typeparam>
        /// <param name="serviceName">The name of the service.</param>
        /// <param name="lifetime">The <see cref="ILifetime"/> instance that controls the lifetime of the registered service.</param>
        /// <returns>The <see cref="IServiceRegistry"/>, for chaining calls.</returns>
        IServiceRegistry Register<TService, TImplementation>(string serviceName, ILifetime lifetime)
            where TImplementation : TService;

        /// <summary>
        /// Registers the <typeparamref name="TService"/> with the given <paramref name="instance"/>.
        /// </summary>
        /// <typeparam name="TService">The service type to register.</typeparam>
        /// <param name="instance">The instance returned when this service is requested.</param>
        /// <returns>The <see cref="IServiceRegistry"/>, for chaining calls.</returns>
        IServiceRegistry RegisterInstance<TService>(TService instance);

        /// <summary>
        /// Registers the <typeparamref name="TService"/> with the given <paramref name="instance"/>.
        /// </summary>
        /// <typeparam name="TService">The service type to register.</typeparam>
        /// <param name="instance">The instance returned when this service is requested.</param>
        /// <param name="serviceName">The name of the service.</param>
        /// <returns>The <see cref="IServiceRegistry"/>, for chaining calls.</returns>
        IServiceRegistry RegisterInstance<TService>(TService instance, string serviceName);

        /// <summary>
        /// Registers the <paramref name="serviceType"/> with the given <paramref name="instance"/>.
        /// </summary>
        /// <param name="serviceType">The service type to register.</param>
        /// <param name="instance">The instance returned when this service is requested.</param>
        /// <returns>The <see cref="IServiceRegistry"/>, for chaining calls.</returns>
        IServiceRegistry RegisterInstance(Type serviceType, object instance);

        /// <summary>
        /// Registers the <paramref name="serviceType"/> with the given <paramref name="instance"/>.
        /// </summary>
        /// <param name="serviceType">The service type to register.</param>
        /// <param name="instance">The instance returned when this service is requested.</param>
        /// <param name="serviceName">The name of the service.</param>
        /// <returns>The <see cref="IServiceRegistry"/>, for chaining calls.</returns>
        IServiceRegistry RegisterInstance(Type serviceType, object instance, string serviceName);

        /// <summary>
        /// Registers a concrete type as a service.
        /// </summary>
        /// <typeparam name="TService">The service type to register.</typeparam>
        /// <returns>The <see cref="IServiceRegistry"/>, for chaining calls.</returns>
        IServiceRegistry Register<TService>();

        /// <summary>
        /// Registers a concrete type as a service.
        /// </summary>
        /// <typeparam name="TService">The service type to register.</typeparam>
        /// <param name="lifetime">The <see cref="ILifetime"/> instance that controls the lifetime of the registered service.</param>
        /// <returns>The <see cref="IServiceRegistry"/>, for chaining calls.</returns>
        IServiceRegistry Register<TService>(ILifetime lifetime);

        /// <summary>
        /// Registers a concrete type as a service.
        /// </summary>
        /// <param name="serviceType">The concrete type to register.</param>
        /// <returns>The <see cref="IServiceRegistry"/>, for chaining calls.</returns>
        IServiceRegistry Register(Type serviceType);

        /// <summary>
        /// Registers a concrete type as a service.
        /// </summary>
        /// <param name="serviceType">The concrete type to register.</param>
        /// <param name="lifetime">The <see cref="ILifetime"/> instance that controls the lifetime of the registered service.</param>
        /// <returns>The <see cref="IServiceRegistry"/>, for chaining calls.</returns>
        IServiceRegistry Register(Type serviceType, ILifetime lifetime);

        /// <summary>
        /// Registers the <typeparamref name="TService"/> with the <paramref name="factory"/> that
        /// describes the dependencies of the service.
        /// </summary>
        /// <typeparam name="TService">The service type to register.</typeparam>
        /// <param name="factory">A factory delegate used to create the <typeparamref name="TService"/> instance.</param>
        /// <returns>The <see cref="IServiceRegistry"/>, for chaining calls.</returns>
        IServiceRegistry Register<TService>(Func<IServiceFactory, TService> factory);

        /// <summary>
        /// Registers the <typeparamref name="TService"/> with the <paramref name="factory"/> that
        /// describes the dependencies of the service.
        /// </summary>
        /// <typeparam name="T">The parameter type.</typeparam>
        /// <typeparam name="TService">The service type to register.</typeparam>
        /// <param name="factory">A factory delegate used to create the <typeparamref name="TService"/> instance.</param>
        /// <returns>The <see cref="IServiceRegistry"/>, for chaining calls.</returns>
        IServiceRegistry Register<T, TService>(Func<IServiceFactory, T, TService> factory);

        /// <summary>
        /// Registers the <typeparamref name="TService"/> with the <paramref name="factory"/> that
        /// describes the dependencies of the service.
        /// </summary>
        /// <typeparam name="T">The parameter type.</typeparam>
        /// <typeparam name="TService">The service type to register.</typeparam>
        /// <param name="factory">A factory delegate used to create the <typeparamref name="TService"/> instance.</param>
        /// <param name="serviceName">The name of the service.</param>
        /// <returns>The <see cref="IServiceRegistry"/>, for chaining calls.</returns>
        IServiceRegistry Register<T, TService>(Func<IServiceFactory, T, TService> factory, string serviceName);

        /// <summary>
        /// Registers the <typeparamref name="TService"/> with the <paramref name="factory"/> that
        /// describes the dependencies of the service.
        /// </summary>
        /// <typeparam name="T1">The type of the first parameter.</typeparam>
        /// <typeparam name="T2">The type of the second parameter.</typeparam>
        /// <typeparam name="TService">The service type to register.</typeparam>
        /// <param name="factory">A factory delegate used to create the <typeparamref name="TService"/> instance.</param>
        /// <returns>The <see cref="IServiceRegistry"/>, for chaining calls.</returns>
        IServiceRegistry Register<T1, T2, TService>(Func<IServiceFactory, T1, T2, TService> factory);

        /// <summary>
        /// Registers the <typeparamref name="TService"/> with the <paramref name="factory"/> that
        /// describes the dependencies of the service.
        /// </summary>
        /// <typeparam name="T1">The type of the first parameter.</typeparam>
        /// <typeparam name="T2">The type of the second parameter.</typeparam>
        /// <typeparam name="TService">The service type to register.</typeparam>
        /// <param name="factory">A factory delegate used to create the <typeparamref name="TService"/> instance.</param>
        /// <param name="serviceName">The name of the service.</param>
        /// <returns>The <see cref="IServiceRegistry"/>, for chaining calls.</returns>
        IServiceRegistry Register<T1, T2, TService>(Func<IServiceFactory, T1, T2, TService> factory, string serviceName);

        /// <summary>
        /// Registers the <typeparamref name="TService"/> with the <paramref name="factory"/> that
        /// describes the dependencies of the service.
        /// </summary>
        /// <typeparam name="T1">The type of the first parameter.</typeparam>
        /// <typeparam name="T2">The type of the second parameter.</typeparam>
        /// <typeparam name="T3">The type of the third parameter.</typeparam>
        /// <typeparam name="TService">The service type to register.</typeparam>
        /// <param name="factory">A factory delegate used to create the <typeparamref name="TService"/> instance.</param>
        /// <returns>The <see cref="IServiceRegistry"/>, for chaining calls.</returns>
        IServiceRegistry Register<T1, T2, T3, TService>(Func<IServiceFactory, T1, T2, T3, TService> factory);

        /// <summary>
        /// Registers the <typeparamref name="TService"/> with the <paramref name="factory"/> that
        /// describes the dependencies of the service.
        /// </summary>
        /// <typeparam name="T1">The type of the first parameter.</typeparam>
        /// <typeparam name="T2">The type of the second parameter.</typeparam>
        /// <typeparam name="T3">The type of the third parameter.</typeparam>
        /// <typeparam name="TService">The service type to register.</typeparam>
        /// <param name="factory">A factory delegate used to create the <typeparamref name="TService"/> instance.</param>
        /// <param name="serviceName">The name of the service.</param>
        /// <returns>The <see cref="IServiceRegistry"/>, for chaining calls.</returns>
        IServiceRegistry Register<T1, T2, T3, TService>(Func<IServiceFactory, T1, T2, T3, TService> factory, string serviceName);

        /// <summary>
        /// Registers the <typeparamref name="TService"/> with the <paramref name="factory"/> that
        /// describes the dependencies of the service.
        /// </summary>
        /// <typeparam name="T1">The type of the first parameter.</typeparam>
        /// <typeparam name="T2">The type of the second parameter.</typeparam>
        /// <typeparam name="T3">The type of the third parameter.</typeparam>
        /// <typeparam name="T4">The type of the fourth parameter.</typeparam>
        /// <typeparam name="TService">The service type to register.</typeparam>
        /// <param name="factory">A factory delegate used to create the <typeparamref name="TService"/> instance.</param>
        /// <returns>The <see cref="IServiceRegistry"/>, for chaining calls.</returns>
        IServiceRegistry Register<T1, T2, T3, T4, TService>(Func<IServiceFactory, T1, T2, T3, T4, TService> factory);

        /// <summary>
        /// Registers the <typeparamref name="TService"/> with the <paramref name="factory"/> that
        /// describes the dependencies of the service.
        /// </summary>
        /// <typeparam name="T1">The type of the first parameter.</typeparam>
        /// <typeparam name="T2">The type of the second parameter.</typeparam>
        /// <typeparam name="T3">The type of the third parameter.</typeparam>
        /// <typeparam name="T4">The type of the fourth parameter.</typeparam>
        /// <typeparam name="TService">The service type to register.</typeparam>
        /// <param name="factory">A factory delegate used to create the <typeparamref name="TService"/> instance.</param>
        /// <param name="serviceName">The name of the service.</param>
        /// <returns>The <see cref="IServiceRegistry"/>, for chaining calls.</returns>
        IServiceRegistry Register<T1, T2, T3, T4, TService>(Func<IServiceFactory, T1, T2, T3, T4, TService> factory, string serviceName);

        /// <summary>
        /// Registers the <typeparamref name="TService"/> with the <paramref name="factory"/> that
        /// describes the dependencies of the service.
        /// </summary>
        /// <typeparam name="TService">The service type to register.</typeparam>
        /// <param name="factory">The lambdaExpression that describes the dependencies of the service.</param>
        /// <param name="lifetime">The <see cref="ILifetime"/> instance that controls the lifetime of the registered service.</param>
        /// <returns>The <see cref="IServiceRegistry"/>, for chaining calls.</returns>
        IServiceRegistry Register<TService>(Func<IServiceFactory, TService> factory, ILifetime lifetime);

        /// <summary>
        /// Registers the <typeparamref name="TService"/> with the <paramref name="factory"/> that
        /// describes the dependencies of the service.
        /// </summary>
        /// <typeparam name="TService">The service type to register.</typeparam>
        /// <param name="factory">The lambdaExpression that describes the dependencies of the service.</param>
        /// <param name="serviceName">The name of the service.</param>
        /// <returns>The <see cref="IServiceRegistry"/>, for chaining calls.</returns>
        IServiceRegistry Register<TService>(Func<IServiceFactory, TService> factory, string serviceName);

        /// <summary>
        /// Registers the <typeparamref name="TService"/> with the <paramref name="factory"/> that
        /// describes the dependencies of the service.
        /// </summary>
        /// <typeparam name="TService">The service type to register.</typeparam>
        /// <param name="factory">The lambdaExpression that describes the dependencies of the service.</param>
        /// <param name="serviceName">The name of the service.</param>
        /// <param name="lifetime">The <see cref="ILifetime"/> instance that controls the lifetime of the registered service.</param>
        /// <returns>The <see cref="IServiceRegistry"/>, for chaining calls.</returns>
        IServiceRegistry Register<TService>(Func<IServiceFactory, TService> factory, string serviceName, ILifetime lifetime);

        /// <summary>
        /// Registers the <paramref name="serviceType"/> with a set of <paramref name="implementingTypes"/> and
        /// ensures that service instance ordering matches the ordering of the <paramref name="implementingTypes"/>.
        /// </summary>
        /// <param name="serviceType">The service type to register.</param>
        /// <param name="implementingTypes">The implementing types.</param>
        /// <param name="lifetimeFactory">The <see cref="ILifetime"/> factory that controls the lifetime of each entry in <paramref name="implementingTypes"/>.</param>
        /// <returns>The <see cref="IServiceRegistry"/>, for chaining calls.</returns>
        IServiceRegistry RegisterOrdered(Type serviceType, Type[] implementingTypes, Func<Type, ILifetime> lifetimeFactory);

        /// <summary>
        /// Registers the <paramref name="serviceType"/> with a set of <paramref name="implementingTypes"/> and
        /// ensures that service instance ordering matches the ordering of the <paramref name="implementingTypes"/>.
        /// </summary>
        /// <param name="serviceType">The service type to register.</param>
        /// <param name="implementingTypes">The implementing types.</param>
        /// <param name="lifeTimeFactory">The <see cref="ILifetime"/> factory that controls the lifetime of each entry in <paramref name="implementingTypes"/>.</param>
        /// <param name="serviceNameFormatter">The function used to format the service name based on current registration index.</param>
        /// <returns>The <see cref="IServiceRegistry"/>, for chaining calls.</returns>
        IServiceRegistry RegisterOrdered(
            Type serviceType,
            Type[] implementingTypes,
            Func<Type, ILifetime> lifeTimeFactory,
            Func<int, string> serviceNameFormatter);

        /// <summary>
        /// Registers a custom factory delegate used to create services that is otherwise unknown to the service container.
        /// </summary>
        /// <param name="predicate">Determines if the service can be created by the <paramref name="factory"/> delegate.</param>
        /// <param name="factory">Creates a service instance according to the <paramref name="predicate"/> predicate.</param>
        /// <returns>The <see cref="IServiceRegistry"/>, for chaining calls.</returns>
        IServiceRegistry RegisterFallback(Func<Type, string, bool> predicate, Func<ServiceRequest, object> factory);

        /// <summary>
        /// Registers a custom factory delegate used to create services that is otherwise unknown to the service container.
        /// </summary>
        /// <param name="predicate">Determines if the service can be created by the <paramref name="factory"/> delegate.</param>
        /// <param name="factory">Creates a service instance according to the <paramref name="predicate"/> predicate.</param>
        /// <param name="lifetime">The <see cref="ILifetime"/> instance that controls the lifetime of the registered service.</param>
        /// <returns>The <see cref="IServiceRegistry"/>, for chaining calls.</returns>
        IServiceRegistry RegisterFallback(Func<Type, string, bool> predicate, Func<ServiceRequest, object> factory, ILifetime lifetime);

        /// <summary>
        /// Registers a service based on a <see cref="ServiceRegistration"/> instance.
        /// </summary>
        /// <param name="serviceRegistration">The <see cref="ServiceRegistration"/> instance that contains service metadata.</param>
        /// <returns>The <see cref="IServiceRegistry"/>, for chaining calls.</returns>
        IServiceRegistry Register(ServiceRegistration serviceRegistration);

        /// <summary>
        /// Registers composition roots from the given <paramref name="assembly"/>.
        /// </summary>
        /// <param name="assembly">The assembly to be scanned for services.</param>
        /// <remarks>
        /// If the target <paramref name="assembly"/> contains an implementation of the <see cref="ICompositionRoot"/> interface, this
        /// will be used to configure the container.
        /// </remarks>
        /// <returns>The <see cref="IServiceRegistry"/>, for chaining calls.</returns>
        IServiceRegistry RegisterAssembly(Assembly assembly);

        /// <summary>
        /// Registers services from the given <paramref name="assembly"/>.
        /// </summary>
        /// <param name="assembly">The assembly to be scanned for services.</param>
        /// <param name="shouldRegister">A function delegate that determines if a service implementation should be registered.</param>
        /// <remarks>
        /// If the target <paramref name="assembly"/> contains an implementation of the <see cref="ICompositionRoot"/> interface, this
        /// will be used to configure the container.
        /// </remarks>
        /// <returns>The <see cref="IServiceRegistry"/>, for chaining calls.</returns>
        IServiceRegistry RegisterAssembly(Assembly assembly, Func<Type, Type, bool> shouldRegister);

        /// <summary>
        /// Registers services from the given <paramref name="assembly"/>.
        /// </summary>
        /// <param name="assembly">The assembly to be scanned for services.</param>
        /// <param name="lifetimeFactory">The <see cref="ILifetime"/> factory that controls the lifetime of the registered service.</param>
        /// <remarks>
        /// If the target <paramref name="assembly"/> contains an implementation of the <see cref="ICompositionRoot"/> interface, this
        /// will be used to configure the container.
        /// </remarks>
        /// <returns>The <see cref="IServiceRegistry"/>, for chaining calls.</returns>
        IServiceRegistry RegisterAssembly(Assembly assembly, Func<ILifetime> lifetimeFactory);

        /// <summary>
        /// Registers services from the given <paramref name="assembly"/>.
        /// </summary>
        /// <param name="assembly">The assembly to be scanned for services.</param>
        /// <param name="lifetimeFactory">The <see cref="ILifetime"/> factory that controls the lifetime of the registered service.</param>
        /// <param name="shouldRegister">A function delegate that determines if a service implementation should be registered.</param>
        /// <remarks>
        /// If the target <paramref name="assembly"/> contains an implementation of the <see cref="ICompositionRoot"/> interface, this
        /// will be used to configure the container.
        /// </remarks>
        /// <returns>The <see cref="IServiceRegistry"/>, for chaining calls.</returns>
        IServiceRegistry RegisterAssembly(Assembly assembly, Func<ILifetime> lifetimeFactory, Func<Type, Type, bool> shouldRegister);

        /// <summary>
        /// Registers services from the given <paramref name="assembly"/>.
        /// </summary>
        /// <param name="assembly">The assembly to be scanned for services.</param>
        /// <param name="lifetimeFactory">The <see cref="ILifetime"/> factory that controls the lifetime of the registered service.</param>
        /// <param name="shouldRegister">A function delegate that determines if a service implementation should be registered.</param>
        /// <param name="serviceNameProvider">A function delegate used to provide the service name for a service during assembly scanning.</param>
        /// <remarks>
        /// If the target <paramref name="assembly"/> contains an implementation of the <see cref="ICompositionRoot"/> interface, this
        /// will be used to configure the container.
        /// </remarks>
        /// <returns>The <see cref="IServiceRegistry"/>, for chaining calls.</returns>
        IServiceRegistry RegisterAssembly(
            Assembly assembly,
            Func<ILifetime> lifetimeFactory,
            Func<Type, Type, bool> shouldRegister,
            Func<Type, Type, string> serviceNameProvider);

        /// <summary>
        /// Registers services from the given <typeparamref name="TCompositionRoot"/> type.
        /// </summary>
        /// <typeparam name="TCompositionRoot">The type of <see cref="ICompositionRoot"/> to register from.</typeparam>
        /// <returns>The <see cref="IServiceRegistry"/>, for chaining calls.</returns>
        IServiceRegistry RegisterFrom<TCompositionRoot>()
            where TCompositionRoot : ICompositionRoot, new();

        /// <summary>
        /// Registers a factory delegate to be used when resolving a constructor dependency for
        /// an implicitly registered service.
        /// </summary>
        /// <typeparam name="TDependency">The dependency type.</typeparam>
        /// <param name="factory">The factory delegate used to create an instance of the dependency.</param>
        /// <returns>The <see cref="IServiceRegistry"/>, for chaining calls.</returns>
        IServiceRegistry RegisterConstructorDependency<TDependency>(
            Func<IServiceFactory, ParameterInfo, TDependency> factory);

        /// <summary>
        /// Registers a factory delegate to be used when resolving a constructor dependency for
        /// an implicitly registered service.
        /// </summary>
        /// <typeparam name="TDependency">The dependency type.</typeparam>
        /// <param name="factory">The factory delegate used to create an instance of the dependency.</param>
        /// <returns>The <see cref="IServiceRegistry"/>, for chaining calls.</returns>
        IServiceRegistry RegisterConstructorDependency<TDependency>(
            Func<IServiceFactory, ParameterInfo, object[], TDependency> factory);

        /// <summary>
        /// Registers a factory delegate to be used when resolving a constructor dependency for
        /// an implicitly registered service.
        /// </summary>
        /// <typeparam name="TDependency">The dependency type.</typeparam>
        /// <param name="factory">The factory delegate used to create an instance of the dependency.</param>
        /// <returns>The <see cref="IServiceRegistry"/>, for chaining calls.</returns>
        IServiceRegistry RegisterPropertyDependency<TDependency>(
            Func<IServiceFactory, PropertyInfo, TDependency> factory);

#if NET452 || NET46 || NETSTANDARD1_6 || NETSTANDARD2_0 || NETCOREAPP2_0
        /// <summary>
        /// Registers composition roots from assemblies in the base directory that match the <paramref name="searchPattern"/>.
        /// </summary>
        /// <param name="searchPattern">The search pattern used to filter the assembly files.</param>
        /// <returns>The <see cref="IServiceRegistry"/>, for chaining calls.</returns>
        IServiceRegistry RegisterAssembly(string searchPattern);
#endif

        /// <summary>
        /// Decorates the <paramref name="serviceType"/> with the given <paramref name="decoratorType"/>.
        /// </summary>
        /// <param name="serviceType">The target service type.</param>
        /// <param name="decoratorType">The decorator type used to decorate the <paramref name="serviceType"/>.</param>
        /// <param name="predicate">A function delegate that determines if the <paramref name="decoratorType"/>
        /// should be applied to the target <paramref name="serviceType"/>.</param>
        /// <returns>The <see cref="IServiceRegistry"/>, for chaining calls.</returns>
        IServiceRegistry Decorate(Type serviceType, Type decoratorType, Func<ServiceRegistration, bool> predicate);

        /// <summary>
        /// Decorates the <paramref name="serviceType"/> with the given <paramref name="decoratorType"/>.
        /// </summary>
        /// <param name="serviceType">The target service type.</param>
        /// <param name="decoratorType">The decorator type used to decorate the <paramref name="serviceType"/>.</param>
        /// <returns>The <see cref="IServiceRegistry"/>, for chaining calls.</returns>
        IServiceRegistry Decorate(Type serviceType, Type decoratorType);

        /// <summary>
        /// Decorates the <typeparamref name="TService"/> with the given <typeparamref name="TDecorator"/>.
        /// </summary>
        /// <typeparam name="TService">The target service type.</typeparam>
        /// <typeparam name="TDecorator">The decorator type used to decorate the <typeparamref name="TService"/>.</typeparam>
        /// <returns>The <see cref="IServiceRegistry"/>, for chaining calls.</returns>
        IServiceRegistry Decorate<TService, TDecorator>()
            where TDecorator : TService;

        /// <summary>
        /// Decorates the <typeparamref name="TService"/> using the given decorator <paramref name="factory"/>.
        /// </summary>
        /// <typeparam name="TService">The target service type.</typeparam>
        /// <param name="factory">A factory delegate used to create a decorator instance.</param>
        /// <returns>The <see cref="IServiceRegistry"/>, for chaining calls.</returns>
        IServiceRegistry Decorate<TService>(Func<IServiceFactory, TService, TService> factory);

        /// <summary>
        /// Registers a decorator based on a <see cref="DecoratorRegistration"/> instance.
        /// </summary>
        /// <param name="decoratorRegistration">The <see cref="DecoratorRegistration"/> instance that contains the decorator metadata.</param>
        /// <returns>The <see cref="IServiceRegistry"/>, for chaining calls.</returns>
        IServiceRegistry Decorate(DecoratorRegistration decoratorRegistration);

        /// <summary>
        /// Allows a registered service to be overridden by another <see cref="ServiceRegistration"/>.
        /// </summary>
        /// <param name="serviceSelector">A function delegate that is used to determine the service that should be
        /// overridden using the <see cref="ServiceRegistration"/> returned from the <paramref name="serviceRegistrationFactory"/>.</param>
        /// <param name="serviceRegistrationFactory">The factory delegate used to create a <see cref="ServiceRegistration"/> that overrides
        /// the incoming <see cref="ServiceRegistration"/>.</param>
        /// <returns>The <see cref="IServiceRegistry"/>, for chaining calls.</returns>
        IServiceRegistry Override(
            Func<ServiceRegistration, bool> serviceSelector,
            Func<IServiceFactory, ServiceRegistration, ServiceRegistration> serviceRegistrationFactory);

        /// <summary>
        /// Allows post-processing of a service instance.
        /// </summary>
        /// <param name="predicate">A function delegate that determines if the given service can be post-processed.</param>
        /// <param name="processor">An action delegate that exposes the created service instance.</param>
        /// <returns>The <see cref="IServiceRegistry"/>, for chaining calls.</returns>
        IServiceRegistry Initialize(Func<ServiceRegistration, bool> predicate, Action<IServiceFactory, object> processor);

        /// <summary>
        /// Sets the default lifetime for types registered without an explicit lifetime. Will only affect new registrations (after this call).
        /// </summary>
        /// <typeparam name="T">The default lifetime type.</typeparam>
        /// <returns>The <see cref="IServiceRegistry"/>, for chaining calls.</returns>
        IServiceRegistry SetDefaultLifetime<T>()
            where T : ILifetime, new();
    }
}