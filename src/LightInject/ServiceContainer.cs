namespace LightInject
{
   /// <summary>
    /// An ultra lightweight service container.
    /// </summary>
    public class ServiceContainer : IServiceContainer
    {
        private const string UnresolvedDependencyError = "Unresolved dependency {0}";
        private readonly Action<LogEntry> log;
        private readonly Func<Type, Type[], IMethodSkeleton> methodSkeletonFactory;
        private readonly ServiceRegistry<Action<IEmitter>> emitters = new ServiceRegistry<Action<IEmitter>>();
        private readonly ServiceRegistry<Delegate> constructorDependencyFactories = new ServiceRegistry<Delegate>();
        private readonly ServiceRegistry<Delegate> propertyDependencyFactories = new ServiceRegistry<Delegate>();
        private readonly ServiceRegistry<ServiceRegistration> availableServices = new ServiceRegistry<ServiceRegistration>();

        private readonly object lockObject = new object();
        private readonly ContainerOptions options;
        private readonly Storage<object> constants = new Storage<object>();
        private readonly Storage<ILifetime> disposableLifeTimes = new Storage<ILifetime>();
        private readonly Storage<DecoratorRegistration> decorators = new Storage<DecoratorRegistration>();
        private readonly Storage<ServiceOverride> overrides = new Storage<ServiceOverride>();
        private readonly Storage<FactoryRule> factoryRules = new Storage<FactoryRule>();
        private readonly Storage<Initializer> initializers = new Storage<Initializer>();

        private readonly Stack<Action<IEmitter>> dependencyStack = new Stack<Action<IEmitter>>();

        private readonly Lazy<IConstructionInfoProvider> constructionInfoProvider;

        private readonly ThreadSafeDictionary<ServiceRegistration, int> servicesToDelegatesIndex =
            new ThreadSafeDictionary<ServiceRegistration, int>();

        private ImmutableHashTable<Type, GetInstanceDelegate> delegates =
            ImmutableHashTable<Type, GetInstanceDelegate>.Empty;

        private ImmutableHashTable<Tuple<Type, string>, GetInstanceDelegate> namedDelegates =
            ImmutableHashTable<Tuple<Type, string>, GetInstanceDelegate>.Empty;

        private ImmutableHashTree<Type, Func<object[], object, object>> propertyInjectionDelegates =
            ImmutableHashTree<Type, Func<object[], object, object>>.Empty;

        private bool isLocked;
        private Type defaultLifetimeType;

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceContainer"/> class.
        /// </summary>
        public ServiceContainer()
            : this(ContainerOptions.Default)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceContainer"/> class.
        /// </summary>
        /// <param name="options">The <see cref="ContainerOptions"/> instances that represents the configurable options.</param>
        public ServiceContainer(ContainerOptions options)
        {
            this.options = options;
            log = options.LogFactory(typeof(ServiceContainer));
            var concreteTypeExtractor = new CachedTypeExtractor(new ConcreteTypeExtractor());
            CompositionRootTypeExtractor = new CachedTypeExtractor(new CompositionRootTypeExtractor(new CompositionRootAttributeExtractor()));
            CompositionRootExecutor = new CompositionRootExecutor(this, type => (ICompositionRoot)Activator.CreateInstance(type));
            ServiceNameProvider = new ServiceNameProvider();
            PropertyDependencySelector = options.EnablePropertyInjection
                ? (IPropertyDependencySelector)new PropertyDependencySelector(new PropertySelector())
                : new PropertyDependencyDisabler();
            GenericArgumentMapper = new GenericArgumentMapper();
            AssemblyScanner = new AssemblyScanner(concreteTypeExtractor, CompositionRootTypeExtractor, CompositionRootExecutor, GenericArgumentMapper);
            ConstructorDependencySelector = new ConstructorDependencySelector();
            ConstructorSelector = new MostResolvableConstructorSelector(CanGetInstance);
            constructionInfoProvider = new Lazy<IConstructionInfoProvider>(CreateConstructionInfoProvider);
            methodSkeletonFactory = (returnType, parameterTypes) => new DynamicMethodSkeleton(returnType, parameterTypes);
#if NET452 || NETSTANDARD1_3 || NETSTANDARD1_6 || NETSTANDARD2_0 || NET46 || NETCOREAPP2_0
            ScopeManagerProvider = new PerLogicalCallContextScopeManagerProvider();
#else
            ScopeManagerProvider = new PerThreadScopeManagerProvider();
#endif
#if NET452 || NET46 || NETSTANDARD1_6 || NETSTANDARD2_0 || NETCOREAPP2_0
            AssemblyLoader = new AssemblyLoader();
#endif
        }

        private ServiceContainer(
            ContainerOptions options,
            ServiceRegistry<Delegate> constructorDependencyFactories,
            ServiceRegistry<Delegate> propertyDependencyFactories,
            ServiceRegistry<ServiceRegistration> availableServices,
            ServiceRegistry<Action<IEmitter>> emitters,
            Storage<DecoratorRegistration> decorators,
            Storage<ServiceOverride> overrides,
            Storage<FactoryRule> factoryRules,
            Storage<Initializer> initializers,
            Lazy<IConstructionInfoProvider> constructionInfoProvider,
            Func<Type, Type[], IMethodSkeleton> methodSkeletonFactory,
            Action<LogEntry> log,
            ICompositionRootExecutor compositionRootExecutor,
            IServiceNameProvider serviceNameProvider,
            IPropertyDependencySelector propertyDependencySelector,
            IGenericArgumentMapper genericArgumentMapper,
            IAssemblyScanner assemblyScanner,
            IConstructorDependencySelector constructorDependencySelector,
            IConstructorSelector constructorSelector,
#if NET452 || NET46 || NETSTANDARD1_6 || NETCOREAPP2_0
            IAssemblyLoader assemblyLoader,
#endif
            IScopeManagerProvider scopeManagerProvider)
        {
            this.options = options;
            this.constructorDependencyFactories = constructorDependencyFactories;
            this.propertyDependencyFactories = propertyDependencyFactories;
            this.availableServices = availableServices;
            this.emitters = emitters;
            this.decorators = decorators;
            this.overrides = overrides;
            this.factoryRules = factoryRules;
            this.initializers = initializers;
            this.constructionInfoProvider = constructionInfoProvider;
            this.methodSkeletonFactory = methodSkeletonFactory;
            this.log = log;
            CompositionRootExecutor = compositionRootExecutor;
            ServiceNameProvider = serviceNameProvider;
            PropertyDependencySelector = propertyDependencySelector;
            GenericArgumentMapper = genericArgumentMapper;
            AssemblyScanner = assemblyScanner;
            ConstructorDependencySelector = constructorDependencySelector;
            ConstructorSelector = constructorSelector;
            ScopeManagerProvider = scopeManagerProvider;
#if NET452 || NET46 || NETSTANDARD1_6 || NETCOREAPP2_0
            AssemblyLoader = assemblyLoader;
#endif
        }

        /// <summary>
        /// Gets or sets the <see cref="IScopeManagerProvider"/> that is responsible
        /// for providing the <see cref="IScopeManager"/> used to manage scopes.
        /// </summary>
        public IScopeManagerProvider ScopeManagerProvider { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="IPropertyDependencySelector"/> instance that
        /// is responsible for selecting the property dependencies for a given type.
        /// </summary>
        public IPropertyDependencySelector PropertyDependencySelector { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="ITypeExtractor"/> that is responsible
        /// for extracting composition roots types from an assembly.
        /// </summary>
        public ITypeExtractor CompositionRootTypeExtractor { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="IServiceNameProvider"/> that is responsible
        /// for providing a service name for a given service during assembly scanning.
        /// </summary>
        public IServiceNameProvider ServiceNameProvider { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="ICompositionRootExecutor"/> that is responsible
        /// for executing composition roots.
        /// </summary>
        public ICompositionRootExecutor CompositionRootExecutor { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="IConstructorDependencySelector"/> instance that
        /// is responsible for selecting the constructor dependencies for a given constructor.
        /// </summary>
        public IConstructorDependencySelector ConstructorDependencySelector { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="IConstructorSelector"/> instance that is responsible
        /// for selecting the constructor to be used when creating new service instances.
        /// </summary>
        public IConstructorSelector ConstructorSelector { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="IGenericArgumentMapper"/> that is responsible for
        /// mapping generic arguments.
        /// </summary>
        public IGenericArgumentMapper GenericArgumentMapper { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="IAssemblyScanner"/> instance that is responsible for scanning assemblies.
        /// </summary>
        public IAssemblyScanner AssemblyScanner { get; set; }
#if NET452 || NETSTANDARD1_6 || NETSTANDARD2_0 || NET46 || NETCOREAPP2_0

        /// <summary>
        /// Gets or sets the <see cref="IAssemblyLoader"/> instance that is responsible for loading assemblies during assembly scanning.
        /// </summary>
        public IAssemblyLoader AssemblyLoader { get; set; }
#endif

        /// <summary>
        /// Gets a list of <see cref="ServiceRegistration"/> instances that represents the registered services.
        /// </summary>
        public IEnumerable<ServiceRegistration> AvailableServices
        {
            get
            {
                return availableServices.Values.SelectMany(t => t.Values);
            }
        }

        private ILifetime DefaultLifetime => (ILifetime)(defaultLifetimeType != null ? Activator.CreateInstance(defaultLifetimeType) : null);

        /// <summary>
        /// Returns <b>true</b> if the container can create the requested service, otherwise <b>false</b>.
        /// </summary>
        /// <param name="serviceType">The <see cref="Type"/> of the service.</param>
        /// <param name="serviceName">The name of the service.</param>
        /// <returns><b>true</b> if the container can create the requested service, otherwise <b>false</b>.</returns>
        public bool CanGetInstance(Type serviceType, string serviceName)
        {
            if (serviceType.IsFunc() || serviceType.IsFuncWithParameters() || serviceType.IsLazy())
            {
                var returnType = serviceType.GenericTypeArguments.Last();
                return GetEmitMethod(returnType, serviceName) != null || availableServices.ContainsKey(serviceType);
            }

            return GetEmitMethod(serviceType, serviceName) != null;
        }

        /// <summary>
        /// Starts a new <see cref="Scope"/>.
        /// </summary>
        /// <returns><see cref="Scope"/>.</returns>
        public Scope BeginScope()
        {
            return ScopeManagerProvider.GetScopeManager(this).BeginScope();
        }

        /// <summary>
        /// Injects the property dependencies for a given <paramref name="instance"/>.
        /// </summary>
        /// <param name="instance">The target instance for which to inject its property dependencies.</param>
        /// <returns>The <paramref name="instance"/> with its property dependencies injected.</returns>
        public object InjectProperties(object instance)
        {
            var type = instance.GetType();

            var del = propertyInjectionDelegates.Search(type);

            if (del == null)
            {
                del = CreatePropertyInjectionDelegate(type);
                propertyInjectionDelegates = propertyInjectionDelegates.Add(type, del);
            }

            return del(constants.Items, instance);
        }

        /// <summary>
        /// Registers the <typeparamref name="TService"/> with the <paramref name="factory"/> that
        /// describes the dependencies of the service.
        /// </summary>
        /// <typeparam name="TService">The service type to register.</typeparam>
        /// <param name="factory">The lambdaExpression that describes the dependencies of the service.</param>
        /// <param name="serviceName">The name of the service.</param>
        /// <param name="lifetime">The <see cref="ILifetime"/> instance that controls the lifetime of the registered service.</param>
        /// <returns>The <see cref="IServiceRegistry"/>, for chaining calls.</returns>
        public IServiceRegistry Register<TService>(Func<IServiceFactory, TService> factory, string serviceName, ILifetime lifetime)
        {
            RegisterServiceFromLambdaExpression<TService>(factory, lifetime, serviceName);
            return this;
        }

        /// <summary>
        /// Registers a custom factory delegate used to create services that is otherwise unknown to the service container.
        /// </summary>
        /// <param name="predicate">Determines if the service can be created by the <paramref name="factory"/> delegate.</param>
        /// <param name="factory">Creates a service instance according to the <paramref name="predicate"/> predicate.</param>
        /// <returns>The <see cref="IServiceRegistry"/>, for chaining calls.</returns>
        public IServiceRegistry RegisterFallback(Func<Type, string, bool> predicate, Func<ServiceRequest, object> factory)
        {
            return RegisterFallback(predicate, factory, DefaultLifetime);
        }

        /// <summary>
        /// Registers a custom factory delegate used to create services that is otherwise unknown to the service container.
        /// </summary>
        /// <param name="predicate">Determines if the service can be created by the <paramref name="factory"/> delegate.</param>
        /// <param name="factory">Creates a service instance according to the <paramref name="predicate"/> predicate.</param>
        /// <param name="lifetime">The <see cref="ILifetime"/> instance that controls the lifetime of the registered service.</param>
        /// <returns>The <see cref="IServiceRegistry"/>, for chaining calls.</returns>
        public IServiceRegistry RegisterFallback(Func<Type, string, bool> predicate, Func<ServiceRequest, object> factory, ILifetime lifetime)
        {
            factoryRules.Add(new FactoryRule { CanCreateInstance = predicate, Factory = factory, LifeTime = lifetime });
            return this;
        }

        /// <summary>
        /// Registers a service based on a <see cref="ServiceRegistration"/> instance.
        /// </summary>
        /// <param name="serviceRegistration">The <see cref="ServiceRegistration"/> instance that contains service metadata.</param>
        /// <returns>The <see cref="IServiceRegistry"/>, for chaining calls.</returns>
        public IServiceRegistry Register(ServiceRegistration serviceRegistration)
        {
            var services = GetAvailableServices(serviceRegistration.ServiceType);
            var sr = serviceRegistration;
            services.AddOrUpdate(
                serviceRegistration.ServiceName,
                s => AddServiceRegistration(sr),
                (k, existing) => UpdateServiceRegistration(existing, sr));
            return this;
        }

        /// <summary>
        /// Registers composition roots from the given <paramref name="assembly"/>.
        /// </summary>
        /// <param name="assembly">The assembly to be scanned for services.</param>
        /// <remarks>
        /// If the target <paramref name="assembly"/> contains an implementation of the <see cref="ICompositionRoot"/> interface, this
        /// will be used to configure the container.
        /// </remarks>
        /// <returns>The <see cref="IServiceRegistry"/>, for chaining calls.</returns>
        public IServiceRegistry RegisterAssembly(Assembly assembly)
        {
            Type[] compositionRootTypes = CompositionRootTypeExtractor.Execute(assembly);
            if (compositionRootTypes.Length == 0)
            {
                RegisterAssembly(assembly, (serviceType, implementingType) => true);
            }
            else
            {
                AssemblyScanner.Scan(assembly, this);
            }

            return this;
        }

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
        public IServiceRegistry RegisterAssembly(Assembly assembly, Func<Type, Type, bool> shouldRegister)
        {
            return RegisterAssembly(assembly, () => DefaultLifetime, shouldRegister);
        }

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
        public IServiceRegistry RegisterAssembly(Assembly assembly, Func<ILifetime> lifetimeFactory)
        {
            return RegisterAssembly(assembly, lifetimeFactory, (serviceType, implementingType) => true);
        }

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
        public IServiceRegistry RegisterAssembly(Assembly assembly, Func<ILifetime> lifetimeFactory, Func<Type, Type, bool> shouldRegister)
        {
            return RegisterAssembly(assembly, lifetimeFactory, shouldRegister, ServiceNameProvider.GetServiceName);
        }

        /// <summary>
        /// Registers services from the given <paramref name="assembly"/>.
        /// </summary>
        /// <param name="assembly">The assembly to be scanned for services.</param>
        /// <param name="lifetimeFactory">The <see cref="ILifetime"/> factory that controls the lifetime of the registered service.</param>
        /// <param name="shouldRegister">A function delegate that determines if a service implementation should be registered.</param>
        /// <param name="serviceNameProvider">A function that is used to determine the service name based on the service type and the implementing type.</param>
        /// <remarks>
        /// If the target <paramref name="assembly"/> contains an implementation of the <see cref="ICompositionRoot"/> interface, this
        /// will be used to configure the container.
        /// </remarks>
        /// <returns>The <see cref="IServiceRegistry"/>, for chaining calls.</returns>
        public IServiceRegistry RegisterAssembly(Assembly assembly, Func<ILifetime> lifetimeFactory, Func<Type, Type, bool> shouldRegister, Func<Type, Type, string> serviceNameProvider)
        {
            AssemblyScanner.Scan(assembly, this, lifetimeFactory, shouldRegister, serviceNameProvider);
            return this;
        }

        /// <summary>
        /// Registers services from the given <typeparamref name="TCompositionRoot"/> type.
        /// </summary>
        /// <typeparam name="TCompositionRoot">The type of <see cref="ICompositionRoot"/> to register from.</typeparam>
        /// <returns>The <see cref="IServiceRegistry"/>, for chaining calls.</returns>
        public IServiceRegistry RegisterFrom<TCompositionRoot>()
            where TCompositionRoot : ICompositionRoot, new()
        {
            CompositionRootExecutor.Execute(typeof(TCompositionRoot));
            return this;
        }

        /// <summary>
        /// Registers a factory delegate to be used when resolving a constructor dependency for
        /// a implicitly registered service.
        /// </summary>
        /// <typeparam name="TDependency">The dependency type.</typeparam>
        /// <param name="factory">The factory delegate used to create an instance of the dependency.</param>
        /// <returns>The <see cref="IServiceRegistry"/>, for chaining calls.</returns>
        public IServiceRegistry RegisterConstructorDependency<TDependency>(Func<IServiceFactory, ParameterInfo, TDependency> factory)
        {
            if (isLocked)
            {
                var message =
                    $"Attempt to register a constructor dependency {typeof(TDependency)} after the first call to GetInstance." +
                    $"This might lead to incorrect behavior if a service with a {typeof(TDependency)} dependency has already been resolved";

                log.Warning(message);
            }

            GetConstructorDependencyFactories(typeof(TDependency)).AddOrUpdate(
                string.Empty,
                s => factory,
                (s, e) => isLocked ? e : factory);
            return this;
        }

        /// <summary>
        /// Registers a factory delegate to be used when resolving a constructor dependency for
        /// a implicitly registered service.
        /// </summary>
        /// <typeparam name="TDependency">The dependency type.</typeparam>
        /// <param name="factory">The factory delegate used to create an instance of the dependency.</param>
        /// <returns>The <see cref="IServiceRegistry"/>, for chaining calls.</returns>
        public IServiceRegistry RegisterConstructorDependency<TDependency>(Func<IServiceFactory, ParameterInfo, object[], TDependency> factory)
        {
            if (isLocked)
            {
                var message =
                    $"Attempt to register a constructor dependency {typeof(TDependency)} after the first call to GetInstance." +
                    $"This might lead to incorrect behaviour if a service with a {typeof(TDependency)} dependency has already been resolved";

                log.Warning(message);
            }

            GetConstructorDependencyFactories(typeof(TDependency)).AddOrUpdate(
                string.Empty,
                s => factory,
                (s, e) => isLocked ? e : factory);
            return this;
        }

        /// <summary>
        /// Registers a factory delegate to be used when resolving a constructor dependency for
        /// a implicitly registered service.
        /// </summary>
        /// <typeparam name="TDependency">The dependency type.</typeparam>
        /// <param name="factory">The factory delegate used to create an instance of the dependency.</param>
        /// <returns>The <see cref="IServiceRegistry"/>, for chaining calls.</returns>
        public IServiceRegistry RegisterPropertyDependency<TDependency>(Func<IServiceFactory, PropertyInfo, TDependency> factory)
        {
            if (isLocked)
            {
                var message =
                    $"Attempt to register a property dependency {typeof(TDependency)} after the first call to GetInstance." +
                    $"This might lead to incorrect behavior if a service with a {typeof(TDependency)} dependency has already been resolved";

                log.Warning(message);
            }

            GetPropertyDependencyFactories(typeof(TDependency)).AddOrUpdate(
                string.Empty,
                s => factory,
                (s, e) => isLocked ? e : factory);
            return this;
        }

#if NET452 || NETSTANDARD1_6 || NETSTANDARD2_0 || NET46 || NETCOREAPP2_0
        /// <summary>
        /// Registers composition roots from assemblies in the base directory that matches the <paramref name="searchPattern"/>.
        /// </summary>
        /// <param name="searchPattern">The search pattern used to filter the assembly files.</param>
        /// <returns>The <see cref="IServiceRegistry"/>, for chaining calls.</returns>
        public IServiceRegistry RegisterAssembly(string searchPattern)
        {
            foreach (Assembly assembly in AssemblyLoader.Load(searchPattern))
            {
                RegisterAssembly(assembly);
            }

            return this;
        }
#endif

        /// <summary>
        /// Decorates the <paramref name="serviceType"/> with the given <paramref name="decoratorType"/>.
        /// </summary>
        /// <param name="serviceType">The target service type.</param>
        /// <param name="decoratorType">The decorator type used to decorate the <paramref name="serviceType"/>.</param>
        /// <param name="predicate">A function delegate that determines if the <paramref name="decoratorType"/>
        /// should be applied to the target <paramref name="serviceType"/>.</param>
        /// <returns>The <see cref="IServiceRegistry"/>, for chaining calls.</returns>
        public IServiceRegistry Decorate(Type serviceType, Type decoratorType, Func<ServiceRegistration, bool> predicate)
        {
            var decoratorRegistration = new DecoratorRegistration { ServiceType = serviceType, ImplementingType = decoratorType, CanDecorate = predicate };
            Decorate(decoratorRegistration);
            return this;
        }

        /// <summary>
        /// Decorates the <paramref name="serviceType"/> with the given <paramref name="decoratorType"/>.
        /// </summary>
        /// <param name="serviceType">The target service type.</param>
        /// <param name="decoratorType">The decorator type used to decorate the <paramref name="serviceType"/>.</param>
        /// <returns>The <see cref="IServiceRegistry"/>, for chaining calls.</returns>
        public IServiceRegistry Decorate(Type serviceType, Type decoratorType)
        {
            Decorate(serviceType, decoratorType, si => true);
            return this;
        }

        /// <summary>
        /// Decorates the <typeparamref name="TService"/> with the given <typeparamref name="TDecorator"/>.
        /// </summary>
        /// <typeparam name="TService">The target service type.</typeparam>
        /// <typeparam name="TDecorator">The decorator type used to decorate the <typeparamref name="TService"/>.</typeparam>
        /// <returns>The <see cref="IServiceRegistry"/>, for chaining calls.</returns>
        public IServiceRegistry Decorate<TService, TDecorator>()
            where TDecorator : TService
        {
            Decorate(typeof(TService), typeof(TDecorator));
            return this;
        }

        /// <summary>
        /// Decorates the <typeparamref name="TService"/> using the given decorator <paramref name="factory"/>.
        /// </summary>
        /// <typeparam name="TService">The target service type.</typeparam>
        /// <param name="factory">A factory delegate used to create a decorator instance.</param>
        /// <returns>The <see cref="IServiceRegistry"/>, for chaining calls.</returns>
        public IServiceRegistry Decorate<TService>(Func<IServiceFactory, TService, TService> factory)
        {
            var decoratorRegistration = new DecoratorRegistration { FactoryExpression = factory, ServiceType = typeof(TService), CanDecorate = si => true };
            Decorate(decoratorRegistration);
            return this;
        }

        /// <summary>
        /// Registers a decorator based on a <see cref="DecoratorRegistration"/> instance.
        /// </summary>
        /// <param name="decoratorRegistration">The <see cref="DecoratorRegistration"/> instance that contains the decorator metadata.</param>
        /// <returns>The <see cref="IServiceRegistry"/>, for chaining calls.</returns>
        public IServiceRegistry Decorate(DecoratorRegistration decoratorRegistration)
        {
            int index = decorators.Add(decoratorRegistration);
            decoratorRegistration.Index = index;
            return this;
        }

        /// <summary>
        /// Allows a registered service to be overridden by another <see cref="ServiceRegistration"/>.
        /// </summary>
        /// <param name="serviceSelector">A function delegate that is used to determine the service that should be
        /// overridden using the <see cref="ServiceRegistration"/> returned from the <paramref name="serviceRegistrationFactory"/>.</param>
        /// <param name="serviceRegistrationFactory">The factory delegate used to create a <see cref="ServiceRegistration"/> that overrides
        /// the incoming <see cref="ServiceRegistration"/>.</param>
        /// <returns>The <see cref="IServiceRegistry"/>, for chaining calls.</returns>
        public IServiceRegistry Override(Func<ServiceRegistration, bool> serviceSelector, Func<IServiceFactory, ServiceRegistration, ServiceRegistration> serviceRegistrationFactory)
        {
            var serviceOverride = new ServiceOverride
            {
                CanOverride = serviceSelector,
                ServiceRegistrationFactory = serviceRegistrationFactory,
            };
            overrides.Add(serviceOverride);
            return this;
        }

        /// <summary>
        /// Allows post-processing of a service instance.
        /// </summary>
        /// <param name="predicate">A function delegate that determines if the given service can be post-processed.</param>
        /// <param name="processor">An action delegate that exposes the created service instance.</param>
        /// <returns>The <see cref="IServiceRegistry"/>, for chaining calls.</returns>
        public IServiceRegistry Initialize(Func<ServiceRegistration, bool> predicate, Action<IServiceFactory, object> processor)
        {
            initializers.Add(new Initializer { Predicate = predicate, Initialize = processor });
            return this;
        }

        /// <summary>
        /// Registers the <paramref name="serviceType"/> with the <paramref name="implementingType"/>.
        /// </summary>
        /// <param name="serviceType">The service type to register.</param>
        /// <param name="implementingType">The implementing type.</param>
        /// <param name="lifetime">The <see cref="ILifetime"/> instance that controls the lifetime of the registered service.</param>
        /// <returns>The <see cref="IServiceRegistry"/>, for chaining calls.</returns>
        public IServiceRegistry Register(Type serviceType, Type implementingType, ILifetime lifetime)
        {
            Register(serviceType, implementingType, string.Empty, lifetime);
            return this;
        }

        /// <summary>
        /// Registers the <paramref name="serviceType"/> with the <paramref name="implementingType"/>.
        /// </summary>
        /// <param name="serviceType">The service type to register.</param>
        /// <param name="implementingType">The implementing type.</param>
        /// <param name="serviceName">The name of the service.</param>
        /// <param name="lifetime">The <see cref="ILifetime"/> instance that controls the lifetime of the registered service.</param>
        /// <returns>The <see cref="IServiceRegistry"/>, for chaining calls.</returns>
        public IServiceRegistry Register(Type serviceType, Type implementingType, string serviceName, ILifetime lifetime)
        {
            RegisterService(serviceType, implementingType, lifetime, serviceName);
            return this;
        }

        /// <summary>
        /// Registers the <typeparamref name="TService"/> with the <typeparamref name="TImplementation"/>.
        /// </summary>
        /// <typeparam name="TService">The service type to register.</typeparam>
        /// <typeparam name="TImplementation">The implementing type.</typeparam>
        /// <returns>The <see cref="IServiceRegistry"/>, for chaining calls.</returns>
        public IServiceRegistry Register<TService, TImplementation>()
            where TImplementation : TService
        {
            Register(typeof(TService), typeof(TImplementation));
            return this;
        }

        /// <summary>
        /// Registers the <typeparamref name="TService"/> with the <typeparamref name="TImplementation"/>.
        /// </summary>
        /// <typeparam name="TService">The service type to register.</typeparam>
        /// <typeparam name="TImplementation">The implementing type.</typeparam>
        /// <param name="lifetime">The <see cref="ILifetime"/> instance that controls the lifetime of the registered service.</param>
        /// <returns>The <see cref="IServiceRegistry"/>, for chaining calls.</returns>
        public IServiceRegistry Register<TService, TImplementation>(ILifetime lifetime)
            where TImplementation : TService
        {
            Register(typeof(TService), typeof(TImplementation), lifetime);
            return this;
        }

        /// <summary>
        /// Registers the <typeparamref name="TService"/> with the <typeparamref name="TImplementation"/>.
        /// </summary>
        /// <typeparam name="TService">The service type to register.</typeparam>
        /// <typeparam name="TImplementation">The implementing type.</typeparam>
        /// <param name="serviceName">The name of the service.</param>
        /// <returns>The <see cref="IServiceRegistry"/>, for chaining calls.</returns>
        public IServiceRegistry Register<TService, TImplementation>(string serviceName)
            where TImplementation : TService
        {
            Register<TService, TImplementation>(serviceName, lifetime: null);
            return this;
        }

        /// <summary>
        /// Registers the <typeparamref name="TService"/> with the <typeparamref name="TImplementation"/>.
        /// </summary>
        /// <typeparam name="TService">The service type to register.</typeparam>
        /// <typeparam name="TImplementation">The implementing type.</typeparam>
        /// <param name="serviceName">The name of the service.</param>
        /// <param name="lifetime">The <see cref="ILifetime"/> instance that controls the lifetime of the registered service.</param>
        /// <returns>The <see cref="IServiceRegistry"/>, for chaining calls.</returns>
        public IServiceRegistry Register<TService, TImplementation>(string serviceName, ILifetime lifetime)
            where TImplementation : TService
        {
            Register(typeof(TService), typeof(TImplementation), serviceName, lifetime);
            return this;
        }

        /// <summary>
        /// Registers the <typeparamref name="TService"/> with the <paramref name="factory"/> that
        /// describes the dependencies of the service.
        /// </summary>
        /// <typeparam name="TService">The service type to register.</typeparam>
        /// <param name="factory">The lambdaExpression that describes the dependencies of the service.</param>
        /// <param name="lifetime">The <see cref="ILifetime"/> instance that controls the lifetime of the registered service.</param>
        /// <returns>The <see cref="IServiceRegistry"/>, for chaining calls.</returns>
        public IServiceRegistry Register<TService>(Func<IServiceFactory, TService> factory, ILifetime lifetime)
        {
            RegisterServiceFromLambdaExpression<TService>(factory, lifetime, string.Empty);
            return this;
        }

        /// <summary>
        /// Registers the <typeparamref name="TService"/> with the <paramref name="factory"/> that
        /// describes the dependencies of the service.
        /// </summary>
        /// <typeparam name="TService">The service type to register.</typeparam>
        /// <param name="factory">The lambdaExpression that describes the dependencies of the service.</param>
        /// <param name="serviceName">The name of the service.</param>
        /// <returns>The <see cref="IServiceRegistry"/>, for chaining calls.</returns>
        public IServiceRegistry Register<TService>(Func<IServiceFactory, TService> factory, string serviceName)
        {
            RegisterServiceFromLambdaExpression<TService>(factory, null, serviceName);
            return this;
        }

        /// <summary>
        /// Registers a concrete type as a service.
        /// </summary>
        /// <typeparam name="TService">The service type to register.</typeparam>
        /// <returns>The <see cref="IServiceRegistry"/>, for chaining calls.</returns>
        public IServiceRegistry Register<TService>()
        {
            Register<TService, TService>();
            return this;
        }

        /// <summary>
        /// Registers a concrete type as a service.
        /// </summary>
        /// <param name="serviceType">The concrete type to register.</param>
        /// <returns>The <see cref="IServiceRegistry"/>, for chaining calls.</returns>
        public IServiceRegistry Register(Type serviceType)
        {
            Register(serviceType, serviceType);
            return this;
        }

        /// <summary>
        /// Registers a concrete type as a service.
        /// </summary>
        /// <param name="serviceType">The concrete type to register.</param>
        /// <param name="lifetime">The <see cref="ILifetime"/> instance that controls the lifetime of the registered service.</param>
        /// <returns>The <see cref="IServiceRegistry"/>, for chaining calls.</returns>
        public IServiceRegistry Register(Type serviceType, ILifetime lifetime)
        {
            Register(serviceType, serviceType, lifetime);
            return this;
        }

        /// <summary>
        /// Registers a concrete type as a service.
        /// </summary>
        /// <typeparam name="TService">The service type to register.</typeparam>
        /// <param name="lifetime">The <see cref="ILifetime"/> instance that controls the lifetime of the registered service.</param>
        /// <returns>The <see cref="IServiceRegistry"/>, for chaining calls.</returns>
        public IServiceRegistry Register<TService>(ILifetime lifetime)
        {
            Register<TService, TService>(lifetime);
            return this;
        }

        /// <summary>
        /// Registers the <typeparamref name="TService"/> with the given <paramref name="instance"/>.
        /// </summary>
        /// <typeparam name="TService">The service type to register.</typeparam>
        /// <param name="instance">The instance returned when this service is requested.</param>
        /// <param name="serviceName">The name of the service.</param>
        /// <returns>The <see cref="IServiceRegistry"/>, for chaining calls.</returns>
        public IServiceRegistry RegisterInstance<TService>(TService instance, string serviceName)
        {
            RegisterInstance(typeof(TService), instance, serviceName);
            return this;
        }

        /// <summary>
        /// Registers the <typeparamref name="TService"/> with the given <paramref name="instance"/>.
        /// </summary>
        /// <typeparam name="TService">The service type to register.</typeparam>
        /// <param name="instance">The instance returned when this service is requested.</param>
        /// <returns>The <see cref="IServiceRegistry"/>, for chaining calls.</returns>
        public IServiceRegistry RegisterInstance<TService>(TService instance)
        {
            RegisterInstance(typeof(TService), instance);
            return this;
        }

        /// <summary>
        /// Registers the <paramref name="serviceType"/> with the given <paramref name="instance"/>.
        /// </summary>
        /// <param name="serviceType">The service type to register.</param>
        /// <param name="instance">The instance returned when this service is requested.</param>
        /// <returns>The <see cref="IServiceRegistry"/>, for chaining calls.</returns>
        public IServiceRegistry RegisterInstance(Type serviceType, object instance)
        {
            RegisterInstance(serviceType, instance, string.Empty);
            return this;
        }

        /// <summary>
        /// Registers the <paramref name="serviceType"/> with the given <paramref name="instance"/>.
        /// </summary>
        /// <param name="serviceType">The service type to register.</param>
        /// <param name="instance">The instance returned when this service is requested.</param>
        /// <param name="serviceName">The name of the service.</param>
        /// <returns>The <see cref="IServiceRegistry"/>, for chaining calls.</returns>
        public IServiceRegistry RegisterInstance(Type serviceType, object instance, string serviceName)
        {
            Ensure.IsNotNull(instance, "instance");
            Ensure.IsNotNull(serviceType, "type");
            Ensure.IsNotNull(serviceName, "serviceName");
            RegisterValue(serviceType, instance, serviceName);
            return this;
        }

        /// <summary>
        /// Registers the <typeparamref name="TService"/> with the <paramref name="factory"/> that
        /// describes the dependencies of the service.
        /// </summary>
        /// <typeparam name="TService">The service type to register.</typeparam>
        /// <param name="factory">The lambdaExpression that describes the dependencies of the service.</param>
        /// <returns>The <see cref="IServiceRegistry"/>, for chaining calls.</returns>
        public IServiceRegistry Register<TService>(Func<IServiceFactory, TService> factory)
        {
            RegisterServiceFromLambdaExpression<TService>(factory, null, string.Empty);
            return this;
        }

        /// <summary>
        /// Registers the <typeparamref name="TService"/> with the <paramref name="factory"/> that
        /// describes the dependencies of the service.
        /// </summary>
        /// <typeparam name="T">The parameter type.</typeparam>
        /// <typeparam name="TService">The service type to register.</typeparam>
        /// <param name="factory">A factory delegate used to create the <typeparamref name="TService"/> instance.</param>
        /// <returns>The <see cref="IServiceRegistry"/>, for chaining calls.</returns>
        public IServiceRegistry Register<T, TService>(Func<IServiceFactory, T, TService> factory)
        {
            RegisterServiceFromLambdaExpression<TService>(factory, null, string.Empty);
            return this;
        }

        /// <summary>
        /// Registers the <typeparamref name="TService"/> with the <paramref name="factory"/> that
        /// describes the dependencies of the service.
        /// </summary>
        /// <typeparam name="T">The parameter type.</typeparam>
        /// <typeparam name="TService">The service type to register.</typeparam>
        /// <param name="factory">A factory delegate used to create the <typeparamref name="TService"/> instance.</param>
        /// <param name="serviceName">The name of the service.</param>
        /// <returns>The <see cref="IServiceRegistry"/>, for chaining calls.</returns>
        public IServiceRegistry Register<T, TService>(Func<IServiceFactory, T, TService> factory, string serviceName)
        {
            RegisterServiceFromLambdaExpression<TService>(factory, null, serviceName);
            return this;
        }

        /// <summary>
        /// Registers the <typeparamref name="TService"/> with the <paramref name="factory"/> that
        /// describes the dependencies of the service.
        /// </summary>
        /// <typeparam name="T1">The type of the first parameter.</typeparam>
        /// <typeparam name="T2">The type of the second parameter.</typeparam>
        /// <typeparam name="TService">The service type to register.</typeparam>
        /// <param name="factory">A factory delegate used to create the <typeparamref name="TService"/> instance.</param>
        /// <returns>The <see cref="IServiceRegistry"/>, for chaining calls.</returns>
        public IServiceRegistry Register<T1, T2, TService>(Func<IServiceFactory, T1, T2, TService> factory)
        {
            RegisterServiceFromLambdaExpression<TService>(factory, null, string.Empty);
            return this;
        }

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
        public IServiceRegistry Register<T1, T2, TService>(Func<IServiceFactory, T1, T2, TService> factory, string serviceName)
        {
            RegisterServiceFromLambdaExpression<TService>(factory, null, serviceName);
            return this;
        }

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
        public IServiceRegistry Register<T1, T2, T3, TService>(Func<IServiceFactory, T1, T2, T3, TService> factory)
        {
            RegisterServiceFromLambdaExpression<TService>(factory, null, string.Empty);
            return this;
        }

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
        public IServiceRegistry Register<T1, T2, T3, TService>(Func<IServiceFactory, T1, T2, T3, TService> factory, string serviceName)
        {
            RegisterServiceFromLambdaExpression<TService>(factory, null, serviceName);
            return this;
        }

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
        public IServiceRegistry Register<T1, T2, T3, T4, TService>(Func<IServiceFactory, T1, T2, T3, T4, TService> factory)
        {
            RegisterServiceFromLambdaExpression<TService>(factory, null, string.Empty);
            return this;
        }

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
        public IServiceRegistry Register<T1, T2, T3, T4, TService>(Func<IServiceFactory, T1, T2, T3, T4, TService> factory, string serviceName)
        {
            RegisterServiceFromLambdaExpression<TService>(factory, null, serviceName);
            return this;
        }

        /// <summary>
        /// Registers the <paramref name="serviceType"/> with the <paramref name="implementingType"/>.
        /// </summary>
        /// <param name="serviceType">The service type to register.</param>
        /// <param name="implementingType">The implementing type.</param>
        /// <param name="serviceName">The name of the service.</param>
        /// <returns>The <see cref="IServiceRegistry"/>, for chaining calls.</returns>
        public IServiceRegistry Register(Type serviceType, Type implementingType, string serviceName)
        {
            RegisterService(serviceType, implementingType, null, serviceName);
            return this;
        }

        /// <summary>
        /// Registers the <paramref name="serviceType"/> with the <paramref name="implementingType"/>.
        /// </summary>
        /// <param name="serviceType">The service type to register.</param>
        /// <param name="implementingType">The implementing type.</param>
        /// <returns>The <see cref="IServiceRegistry"/>, for chaining calls.</returns>
        public IServiceRegistry Register(Type serviceType, Type implementingType)
        {
            RegisterService(serviceType, implementingType, null, string.Empty);
            return this;
        }

        /// <summary>
        /// Registers the <paramref name="serviceType"/> with a set of <paramref name="implementingTypes"/> and
        /// ensures that service instance ordering matches the ordering of the <paramref name="implementingTypes"/>.
        /// </summary>
        /// <param name="serviceType">The service type to register.</param>
        /// <param name="implementingTypes">The implementing types.</param>
        /// <param name="lifeTimeFactory">The <see cref="ILifetime"/> factory that controls the lifetime of each entry in <paramref name="implementingTypes"/>.</param>
        /// <returns>The <see cref="IServiceRegistry"/>, for chaining calls.</returns>
        public IServiceRegistry RegisterOrdered(Type serviceType, Type[] implementingTypes, Func<Type, ILifetime> lifeTimeFactory)
        {
            return RegisterOrdered(serviceType, implementingTypes, lifeTimeFactory, i => i.ToString().PadLeft(3, '0'));
        }

        /// <summary>
        /// Registers the <paramref name="serviceType"/> with a set of <paramref name="implementingTypes"/> and
        /// ensures that service instance ordering matches the ordering of the <paramref name="implementingTypes"/>.
        /// </summary>
        /// <param name="serviceType">The service type to register.</param>
        /// <param name="implementingTypes">The implementing types.</param>
        /// <param name="lifeTimeFactory">The <see cref="ILifetime"/> factory that controls the lifetime of each entry in <paramref name="implementingTypes"/>.</param>
        /// <param name="serviceNameFormatter">The function used to format the service name based on current registration index.</param>
        /// <returns>The <see cref="IServiceRegistry"/>, for chaining calls.</returns>
        public IServiceRegistry RegisterOrdered(
            Type serviceType,
            Type[] implementingTypes,
            Func<Type, ILifetime> lifeTimeFactory,
            Func<int, string> serviceNameFormatter)
        {
            var offset = GetAvailableServices(serviceType).Count;
            foreach (var implementingType in implementingTypes)
            {
                offset++;
                Register(serviceType, implementingType, serviceNameFormatter(offset), lifeTimeFactory(implementingType));
            }

            return this;
        }

        /// <summary>
        /// Compiles services that matches the given <paramref name="predicate"/>.
        /// </summary>
        /// <param name="predicate">The predicate that determines if a service should be compiled.</param>
        public void Compile(Func<ServiceRegistration, bool> predicate)
        {
            var rootServices = AvailableServices.Where(predicate).ToArray();
            foreach (var rootService in rootServices)
            {
                if (rootService.ServiceType.GetTypeInfo().IsGenericTypeDefinition)
                {
                    log.Warning($"Unable to precompile open generic type '{GetPrettyName(rootService.ServiceType)}'");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(rootService.ServiceName))
                {
                    CreateDefaultDelegate(rootService.ServiceType, true);
                }
                else
                {
                    CreateNamedDelegate(Tuple.Create(rootService.ServiceType, rootService.ServiceName), true);
                }
            }

            string GetPrettyName(Type type)
            {
                if (type.GetTypeInfo().IsGenericType)
                {
                    return $"{type.FullName.Substring(0, type.FullName.LastIndexOf("`", StringComparison.OrdinalIgnoreCase))}<{string.Join(", ", type.GetTypeInfo().GenericTypeParameters.Select(GetPrettyName))}>";
                }

                return type.Name;
            }
        }

        /// <summary>
        /// Compiles all registered services.
        /// </summary>
        public void Compile()
        {
            Compile(sr => true);
        }

        /// <summary>
        /// Compiles the service identified by <typeparamref name="TService"/>
        /// and optionally the <paramref name="serviceName"/>.
        /// </summary>
        /// <typeparam name="TService">The service type to be compiled.</typeparam>
        /// <param name="serviceName">The name of the service to be compiled.</param>
        public void Compile<TService>(string serviceName = null)
        {
            if (string.IsNullOrWhiteSpace(serviceName))
            {
                CreateDefaultDelegate(typeof(TService), true);
            }
            else
            {
                CreateNamedDelegate(Tuple.Create(typeof(TService), serviceName), true);
            }
        }

        /// <summary>
        /// Gets an instance of the given <paramref name="serviceType"/>.
        /// </summary>
        /// <param name="serviceType">The type of the requested service.</param>
        /// <returns>The requested service instance.</returns>
        public object GetInstance(Type serviceType)
        {
            var instanceDelegate = delegates.Search(serviceType);
            if (instanceDelegate == null)
            {
                instanceDelegate = CreateDefaultDelegate(serviceType, throwError: true);
            }

            return instanceDelegate(constants.Items);
        }

        /// <summary>
        /// Gets an instance of the given <paramref name="serviceType"/>.
        /// </summary>
        /// <param name="serviceType">The type of the requested service.</param>
        /// <param name="arguments">The arguments to be passed to the target instance.</param>
        /// <returns>The requested service instance.</returns>
        public object GetInstance(Type serviceType, object[] arguments)
        {
            var instanceDelegate = delegates.Search(serviceType);
            if (instanceDelegate == null)
            {
                instanceDelegate = CreateDefaultDelegate(serviceType, throwError: true);
            }

            object[] constantsWithArguments = constants.Items.Concat(new object[] { arguments }).ToArray();

            return instanceDelegate(constantsWithArguments);
        }

        /// <summary>
        /// Gets an instance of the given <paramref name="serviceType"/>.
        /// </summary>
        /// <param name="serviceType">The type of the requested service.</param>
        /// <param name="serviceName">The name of the requested service.</param>
        /// <param name="arguments">The arguments to be passed to the target instance.</param>
        /// <returns>The requested service instance.</returns>
        public object GetInstance(Type serviceType, string serviceName, object[] arguments)
        {
            var key = Tuple.Create(serviceType, serviceName);
            var instanceDelegate = namedDelegates.Search(key);
            if (instanceDelegate == null)
            {
                instanceDelegate = CreateNamedDelegate(key, throwError: true);
            }

            object[] constantsWithArguments = constants.Items.Concat(new object[] { arguments }).ToArray();

            return instanceDelegate(constantsWithArguments);
        }

        /// <summary>
        /// Gets an instance of the given <paramref name="serviceType"/>.
        /// </summary>
        /// <param name="serviceType">The type of the requested service.</param>
        /// <returns>The requested service instance if available, otherwise null.</returns>
        public object TryGetInstance(Type serviceType)
        {
            var instanceDelegate = delegates.Search(serviceType);
            if (instanceDelegate == null)
            {
                instanceDelegate = CreateDefaultDelegate(serviceType, throwError: false);
            }

            return instanceDelegate(constants.Items);
        }

        /// <summary>
        /// Gets a named instance of the given <paramref name="serviceType"/>.
        /// </summary>
        /// <param name="serviceType">The type of the requested service.</param>
        /// <param name="serviceName">The name of the requested service.</param>
        /// <returns>The requested service instance if available, otherwise null.</returns>
        public object TryGetInstance(Type serviceType, string serviceName)
        {
            var key = Tuple.Create(serviceType, serviceName);
            var instanceDelegate = namedDelegates.Search(key);
            if (instanceDelegate == null)
            {
                instanceDelegate = CreateNamedDelegate(key, throwError: false);
            }

            return instanceDelegate(constants.Items);
        }

        /// <summary>
        /// Gets a named instance of the given <paramref name="serviceType"/>.
        /// </summary>
        /// <param name="serviceType">The type of the requested service.</param>
        /// <param name="serviceName">The name of the requested service.</param>
        /// <returns>The requested service instance.</returns>
        public object GetInstance(Type serviceType, string serviceName)
        {
            var key = Tuple.Create(serviceType, serviceName);
            var instanceDelegate = namedDelegates.Search(key);
            if (instanceDelegate == null)
            {
                instanceDelegate = CreateNamedDelegate(key, throwError: true);
            }

            return instanceDelegate(constants.Items);
        }

        /// <summary>
        /// Gets all instances of the given <paramref name="serviceType"/>.
        /// </summary>
        /// <param name="serviceType">The type of services to resolve.</param>
        /// <returns>A list that contains all implementations of the <paramref name="serviceType"/>.</returns>
        public IEnumerable<object> GetAllInstances(Type serviceType)
        {
            return (IEnumerable<object>)GetInstance(serviceType.GetEnumerableType());
        }

        /// <summary>
        /// Creates an instance of a concrete class.
        /// </summary>
        /// <param name="serviceType">The type of class for which to create an instance.</param>
        /// <returns>An instance of the <paramref name="serviceType"/>.</returns>
        public object Create(Type serviceType)
        {
            Register(serviceType);
            return GetInstance(serviceType);
        }

        /// <summary>
        /// Sets the default lifetime for types registered without an explicit lifetime. Will only affect new registrations (after this call).
        /// </summary>
        /// <typeparam name="T">The default lifetime type.</typeparam>
        /// <returns>The <see cref="IServiceRegistry"/>, for chaining calls.</returns>
        public IServiceRegistry SetDefaultLifetime<T>()
            where T : ILifetime, new()
        {
            defaultLifetimeType = typeof(T);
            return this;
        }

        /// <summary>
        /// Disposes any services registered using a disposable lifetime.
        /// </summary>
        public void Dispose()
        {
            var disposableLifetimeInstances = disposableLifeTimes.Items
                .Where(lt => lt is IDisposable).Cast<IDisposable>().Reverse();
            foreach (var disposableLifetimeInstance in disposableLifetimeInstances)
            {
                disposableLifetimeInstance.Dispose();
            }
        }

        /// <summary>
        /// Creates a clone of the current <see cref="ServiceContainer"/>.
        /// </summary>
        /// <returns>A new <see cref="ServiceContainer"/> instance.</returns>
        public ServiceContainer Clone()
        {
            return new ServiceContainer(
                options,
                constructorDependencyFactories,
                propertyDependencyFactories,
                availableServices,
                emitters,
                decorators,
                overrides,
                factoryRules,
                initializers,
                constructionInfoProvider,
                methodSkeletonFactory,
                log,
                CompositionRootExecutor,
                ServiceNameProvider,
                PropertyDependencySelector,
                GenericArgumentMapper,
                AssemblyScanner,
                ConstructorDependencySelector,
                ConstructorSelector,
#if NET452 || NET46 || NETSTANDARD1_6 || NETCOREAPP2_0
                AssemblyLoader,
#endif
                ScopeManagerProvider);
        }

        private static void EmitEnumerable(IList<Action<IEmitter>> serviceEmitters, Type elementType, IEmitter emitter)
        {
            EmitNewArray(serviceEmitters, elementType, emitter);
        }

        private static void EmitNewArray(IList<Action<IEmitter>> emitMethods, Type elementType, IEmitter emitter)
        {
            LocalBuilder array = emitter.DeclareLocal(elementType.MakeArrayType());
            emitter.Push(emitMethods.Count);
            emitter.PushNewArray(elementType);
            emitter.Store(array);

            for (int index = 0; index < emitMethods.Count; index++)
            {
                emitter.Push(array);
                emitter.Push(index);
                emitMethods[index](emitter);
                emitter.UnboxOrCast(elementType);
                emitter.Emit(OpCodes.Stelem, elementType);
            }

            emitter.Push(array);
        }

        private static ILifetime CloneLifeTime(ILifetime lifetime)
        {
            if (lifetime is ICloneableLifeTime cloneable)
            {
                return cloneable.Clone();
            }

            return lifetime == null ? null : (ILifetime)Activator.CreateInstance(lifetime.GetType());
        }

        private static ConstructorDependency GetConstructorDependencyThatRepresentsDecoratorTarget(
            DecoratorRegistration decoratorRegistration, ConstructionInfo constructionInfo)
        {
            var constructorDependency =
                constructionInfo.ConstructorDependencies.FirstOrDefault(
                    cd =>
                        cd.ServiceType == decoratorRegistration.ServiceType
                        || (cd.ServiceType.IsLazy()
                            && cd.ServiceType.GetTypeInfo().GenericTypeArguments[0] == decoratorRegistration.ServiceType));
            return constructorDependency;
        }

        private static void PushRuntimeArguments(IEmitter emitter)
        {
            MethodInfo loadMethod = typeof(RuntimeArgumentsLoader).GetTypeInfo().GetDeclaredMethod("Load");
            emitter.Emit(OpCodes.Ldarg_0);
            emitter.Emit(OpCodes.Call, loadMethod);
        }

        private DecoratorRegistration CreateClosedGenericDecoratorRegistration(
            ServiceRegistration serviceRegistration, DecoratorRegistration openGenericDecorator)
        {
            Type implementingType = openGenericDecorator.ImplementingType;
            Type serviceType = serviceRegistration.ServiceType;
            Type[] genericTypeArguments = serviceType.GenericTypeArguments;

            if (!TryCreateClosedGenericDecoratorType(serviceType, implementingType, out var closedGenericDecoratorType))
            {
                log.Info($"Skipping decorator [{implementingType.FullName}] since it is incompatible with the service type [{serviceType.FullName}]");
                return null;
            }

            var decoratorInfo = new DecoratorRegistration
            {
                ServiceType = serviceRegistration.ServiceType,
                ImplementingType = closedGenericDecoratorType,
                CanDecorate = openGenericDecorator.CanDecorate,
                Index = openGenericDecorator.Index,
            };
            return decoratorInfo;
        }

        private bool TryCreateClosedGenericDecoratorType(Type serviceType, Type implementingType, out Type closedGenericDecoratorType)
        {
            closedGenericDecoratorType = null;
            var mapResult = GenericArgumentMapper.Map(serviceType, implementingType);
            if (!mapResult.IsValid)
            {
                return false;
            }

            closedGenericDecoratorType = TypeHelper.TryMakeGenericType(implementingType, mapResult.GetMappedArguments());
            if (closedGenericDecoratorType == null)
            {
                return false;
            }

            if (!serviceType.GetTypeInfo().IsAssignableFrom(closedGenericDecoratorType.GetTypeInfo()))
            {
                return false;
            }

            return true;
        }

        private Func<object[], object, object> CreatePropertyInjectionDelegate(Type concreteType)
        {
            lock (lockObject)
            {
                IMethodSkeleton methodSkeleton = methodSkeletonFactory(typeof(object), new[] { typeof(object[]), typeof(object) });

                ConstructionInfo constructionInfo = new ConstructionInfo();
                constructionInfo.PropertyDependencies.AddRange(PropertyDependencySelector.Execute(concreteType));
                constructionInfo.ImplementingType = concreteType;

                var emitter = methodSkeleton.GetEmitter();
                emitter.PushArgument(1);
                emitter.Cast(concreteType);
                try
                {
                    EmitPropertyDependencies(constructionInfo, emitter);
                }
                catch (Exception)
                {
                    dependencyStack.Clear();
                    throw;
                }

                emitter.Return();

                isLocked = true;

                return (Func<object[], object, object>)methodSkeleton.CreateDelegate(typeof(Func<object[], object, object>));
            }
        }

        private ConstructionInfoProvider CreateConstructionInfoProvider()
        {
            return new ConstructionInfoProvider(CreateTypeConstructionInfoBuilder());
        }

        private TypeConstructionInfoBuilder CreateTypeConstructionInfoBuilder()
        {
            return new TypeConstructionInfoBuilder(
                ConstructorSelector,
                ConstructorDependencySelector,
                PropertyDependencySelector,
                GetConstructorDependencyDelegate,
                GetPropertyDependencyExpression);
        }

        private Delegate GetConstructorDependencyDelegate(Type type, string serviceName)
        {
            GetConstructorDependencyFactories(type).TryGetValue(serviceName, out Delegate dependencyDelegate);
            return dependencyDelegate;
        }

        private Delegate GetPropertyDependencyExpression(Type type, string serviceName)
        {
            GetPropertyDependencyFactories(type).TryGetValue(serviceName, out Delegate dependencyDelegate);
            return dependencyDelegate;
        }

        private GetInstanceDelegate CreateDynamicMethodDelegate(Action<IEmitter> serviceEmitter)
        {
            var methodSkeleton = methodSkeletonFactory(typeof(object), new[] { typeof(object[]) });
            IEmitter emitter = methodSkeleton.GetEmitter();
            serviceEmitter(emitter);
            if (emitter.StackType.GetTypeInfo().IsValueType)
            {
                emitter.Emit(OpCodes.Box, emitter.StackType);
            }

            Instruction lastInstruction = emitter.Instructions.Last();

            if (lastInstruction.Code == OpCodes.Castclass)
            {
                emitter.Instructions.Remove(lastInstruction);
            }

            emitter.Return();

            isLocked = true;

            return (GetInstanceDelegate)methodSkeleton.CreateDelegate(typeof(GetInstanceDelegate));
        }

        private Func<object> WrapAsFuncDelegate(GetInstanceDelegate instanceDelegate)
        {
            return () => instanceDelegate(constants.Items);
        }

        private Action<IEmitter> GetEmitMethod(Type serviceType, string serviceName)
        {
            Action<IEmitter> emitMethod = GetRegisteredEmitMethod(serviceType, serviceName);

            if (emitMethod == null)
            {
                emitMethod = TryGetFallbackEmitMethod(serviceType, serviceName);
            }

            if (emitMethod == null)
            {
                AssemblyScanner.Scan(serviceType.GetTypeInfo().Assembly, this);
                emitMethod = GetRegisteredEmitMethod(serviceType, serviceName);
            }

            if (emitMethod == null)
            {
                emitMethod = TryGetFallbackEmitMethod(serviceType, serviceName);
            }

            return CreateEmitMethodWrapper(emitMethod, serviceType, serviceName);
        }

        private Action<IEmitter> TryGetFallbackEmitMethod(Type serviceType, string serviceName)
        {
            Action<IEmitter> emitMethod = null;
            var rule = factoryRules.Items.FirstOrDefault(r => r.CanCreateInstance(serviceType, serviceName));
            if (rule != null)
            {
                emitMethod = CreateServiceEmitterBasedOnFactoryRule(rule, serviceType, serviceName);

                RegisterEmitMethod(serviceType, serviceName, emitMethod);
            }

            return emitMethod;
        }

        private Action<IEmitter> CreateEmitMethodWrapper(Action<IEmitter> emitter, Type serviceType, string serviceName)
        {
            if (emitter == null)
            {
                return null;
            }

            return ms =>
            {
                if (dependencyStack.Contains(emitter))
                {
                    throw new InvalidOperationException(
                        string.Format("Recursive dependency detected: ServiceType:{0}, ServiceName:{1}]", serviceType, serviceName));
                }

                dependencyStack.Push(emitter);
                try
                {
                    emitter(ms);
                }
                finally
                {
                    if (dependencyStack.Count > 0)
                    {
                        dependencyStack.Pop();
                    }
                }
            };
        }

        private Action<IEmitter> GetRegisteredEmitMethod(Type serviceType, string serviceName)
        {
            var registrations = GetEmitMethods(serviceType);

            if (string.IsNullOrWhiteSpace(serviceName))
            {
                if (registrations.Count > 1)
                {
                    var serviceNames = registrations.Keys.OrderBy(k => k).ToArray();
                    var defaultServiceName = string.Empty;
                    serviceName = options.DefaultServiceSelector(serviceNames);
                }
            }

            registrations.TryGetValue(serviceName, out Action<IEmitter> emitMethod);
            return emitMethod ?? CreateEmitMethodForUnknownService(serviceType, serviceName);
        }

        private ServiceRegistration AddServiceRegistration(ServiceRegistration serviceRegistration)
        {
            var emitMethod = ResolveEmitMethod(serviceRegistration);
            RegisterEmitMethod(serviceRegistration.ServiceType, serviceRegistration.ServiceName, emitMethod);

            return serviceRegistration;
        }

        private void RegisterEmitMethod(Type serviceType, string serviceName, Action<IEmitter> emitMethod)
        {
            GetEmitMethods(serviceType).TryAdd(serviceName, emitMethod);
        }

        private ServiceRegistration UpdateServiceRegistration(ServiceRegistration existingRegistration, ServiceRegistration newRegistration)
        {
            if (isLocked)
            {
                var message = $"Cannot overwrite existing serviceregistration {existingRegistration} after the first call to GetInstance.";
                log.Warning(message);
                return existingRegistration;
            }

            Action<IEmitter> emitMethod = ResolveEmitMethod(newRegistration);
            var serviceEmitters = GetEmitMethods(newRegistration.ServiceType);
            serviceEmitters[newRegistration.ServiceName] = emitMethod;
            return newRegistration;
        }

        private void EmitNewInstanceWithDecorators(ServiceRegistration serviceRegistration, IEmitter emitter)
        {
            var serviceOverrides = overrides.Items.Where(so => so.CanOverride(serviceRegistration)).ToArray();
            foreach (var serviceOverride in serviceOverrides)
            {
                serviceRegistration = serviceOverride.ServiceRegistrationFactory(this, serviceRegistration);
            }

            var serviceDecorators = GetDecorators(serviceRegistration);
            if (serviceDecorators.Length > 0)
            {
                EmitDecorators(serviceRegistration, serviceDecorators, emitter, dm => EmitNewInstance(serviceRegistration, dm));
            }
            else
            {
                EmitNewInstance(serviceRegistration, emitter);
            }
        }

        private DecoratorRegistration[] GetDecorators(ServiceRegistration serviceRegistration)
        {
            var registeredDecorators = decorators.Items.Where(d => d.ServiceType == serviceRegistration.ServiceType).ToList();

            registeredDecorators.AddRange(GetOpenGenericDecoratorRegistrations(serviceRegistration));
            registeredDecorators.AddRange(GetDeferredDecoratorRegistrations(serviceRegistration));
            return registeredDecorators.OrderBy(d => d.Index).ToArray();
        }

        private IEnumerable<DecoratorRegistration> GetOpenGenericDecoratorRegistrations(
            ServiceRegistration serviceRegistration)
        {
            var registrations = new List<DecoratorRegistration>();
            var serviceTypeInfo = serviceRegistration.ServiceType.GetTypeInfo();
            if (serviceTypeInfo.IsGenericType)
            {
                var openGenericServiceType = serviceTypeInfo.GetGenericTypeDefinition();
                var openGenericDecorators = decorators.Items.Where(d => d.ServiceType == openGenericServiceType);
                registrations.AddRange(
                    openGenericDecorators.Select(
                        openGenericDecorator =>
                            CreateClosedGenericDecoratorRegistration(serviceRegistration, openGenericDecorator)).Where(dr => dr != null));
            }

            return registrations;
        }

        private IEnumerable<DecoratorRegistration> GetDeferredDecoratorRegistrations(
            ServiceRegistration serviceRegistration)
        {
            var registrations = new List<DecoratorRegistration>();

            var deferredDecorators =
                decorators.Items.Where(ds => ds.CanDecorate(serviceRegistration) && ds.HasDeferredImplementingType);
            foreach (var deferredDecorator in deferredDecorators)
            {
                var decoratorRegistration = new DecoratorRegistration
                {
                    ServiceType = serviceRegistration.ServiceType,
                    ImplementingType =
                        deferredDecorator.ImplementingTypeFactory(this, serviceRegistration),
                    CanDecorate = sr => true,
                    Index = deferredDecorator.Index,
                };
                registrations.Add(decoratorRegistration);
            }

            return registrations;
        }

        private void EmitNewDecoratorInstance(DecoratorRegistration decoratorRegistration, IEmitter emitter, Action<IEmitter> pushInstance)
        {
            ConstructionInfo constructionInfo = GetConstructionInfo(decoratorRegistration);
            var constructorDependency = GetConstructorDependencyThatRepresentsDecoratorTarget(
                decoratorRegistration, constructionInfo);

            if (constructorDependency != null)
            {
                constructorDependency.IsDecoratorTarget = true;
            }

            if (constructionInfo.FactoryDelegate != null)
            {
                EmitNewDecoratorUsingFactoryDelegate(constructionInfo.FactoryDelegate, emitter, pushInstance);
            }
            else
            {
                EmitNewInstanceUsingImplementingType(emitter, constructionInfo, pushInstance);
            }
        }

        private void EmitNewDecoratorUsingFactoryDelegate(Delegate factoryDelegate, IEmitter emitter, Action<IEmitter> pushInstance)
        {
            var factoryDelegateIndex = constants.Add(factoryDelegate);
            var serviceFactoryIndex = constants.Add(this);
            Type funcType = factoryDelegate.GetType();
            emitter.PushConstant(factoryDelegateIndex, funcType);
            emitter.PushConstant(serviceFactoryIndex, typeof(IServiceFactory));
            pushInstance(emitter);
            MethodInfo invokeMethod = funcType.GetTypeInfo().GetDeclaredMethod("Invoke");
            emitter.Emit(OpCodes.Callvirt, invokeMethod);
        }

        private void EmitNewInstance(ServiceRegistration serviceRegistration, IEmitter emitter)
        {
            if (serviceRegistration.Value != null)
            {
                int index = constants.Add(serviceRegistration.Value);
                Type serviceType = serviceRegistration.ServiceType;
                emitter.PushConstant(index, serviceType);
            }
            else
            {
                var constructionInfo = GetConstructionInfo(serviceRegistration);

                if (serviceRegistration.FactoryExpression != null)
                {
                    EmitNewInstanceUsingFactoryDelegate(serviceRegistration, emitter);
                }
                else
                {
                    EmitNewInstanceUsingImplementingType(emitter, constructionInfo, null);
                }
            }

            var processors = initializers.Items.Where(i => i.Predicate(serviceRegistration)).ToArray();
            if (processors.Length == 0)
            {
                return;
            }

            LocalBuilder instanceVariable = emitter.DeclareLocal(serviceRegistration.ServiceType);
            emitter.Store(instanceVariable);
            foreach (var postProcessor in processors)
            {
                Type delegateType = postProcessor.Initialize.GetType();
                var delegateIndex = constants.Add(postProcessor.Initialize);
                emitter.PushConstant(delegateIndex, delegateType);
                var serviceFactoryIndex = constants.Add(this);
                emitter.PushConstant(serviceFactoryIndex, typeof(IServiceFactory));
                emitter.Push(instanceVariable);
                MethodInfo invokeMethod = delegateType.GetTypeInfo().GetDeclaredMethod("Invoke");
                emitter.Call(invokeMethod);
            }

            emitter.Push(instanceVariable);
        }

        private void EmitDecorators(ServiceRegistration serviceRegistration, IEnumerable<DecoratorRegistration> serviceDecorators, IEmitter emitter, Action<IEmitter> decoratorTargetEmitMethod)
        {
            foreach (DecoratorRegistration decorator in serviceDecorators)
            {
                if (!decorator.CanDecorate(serviceRegistration))
                {
                    continue;
                }

                Action<IEmitter> currentDecoratorTargetEmitter = decoratorTargetEmitMethod;
                DecoratorRegistration currentDecorator = decorator;
                decoratorTargetEmitMethod = e => EmitNewDecoratorInstance(currentDecorator, e, currentDecoratorTargetEmitter);
            }

            decoratorTargetEmitMethod(emitter);
        }

        private void EmitNewInstanceUsingImplementingType(IEmitter emitter, ConstructionInfo constructionInfo, Action<IEmitter> decoratorTargetEmitMethod)
        {
            EmitConstructorDependencies(constructionInfo, emitter, decoratorTargetEmitMethod);
            emitter.Emit(OpCodes.Newobj, constructionInfo.Constructor);
            EmitPropertyDependencies(constructionInfo, emitter);
        }

        private void EmitNewInstanceUsingFactoryDelegate(ServiceRegistration serviceRegistration, IEmitter emitter)
        {
            var factoryDelegateIndex = constants.Add(serviceRegistration.FactoryExpression);
            Type funcType = serviceRegistration.FactoryExpression.GetType();
            MethodInfo invokeMethod = funcType.GetTypeInfo().GetDeclaredMethod("Invoke");
            emitter.PushConstant(factoryDelegateIndex, funcType);
            var parameters = invokeMethod.GetParameters();
            if (parameters.Length == 1 && parameters[0].ParameterType == typeof(ServiceRequest))
            {
                var serviceRequest = new ServiceRequest(serviceRegistration.ServiceType, serviceRegistration.ServiceName, this);
                var serviceRequestIndex = constants.Add(serviceRequest);
                emitter.PushConstant(serviceRequestIndex, typeof(ServiceRequest));
                emitter.Call(invokeMethod);
                emitter.UnboxOrCast(serviceRegistration.ServiceType);
            }
            else
            {
                var serviceFactoryIndex = constants.Add(this);
                emitter.PushConstant(serviceFactoryIndex, typeof(IServiceFactory));

                if (parameters.Length > 1)
                {
                    emitter.PushArguments(parameters.Skip(1).ToArray());
                }

                emitter.Call(invokeMethod);
            }
        }

        private void EmitConstructorDependencies(ConstructionInfo constructionInfo, IEmitter emitter, Action<IEmitter> decoratorTargetEmitter)
        {
            foreach (ConstructorDependency dependency in constructionInfo.ConstructorDependencies)
            {
                if (!dependency.IsDecoratorTarget)
                {
                    EmitConstructorDependency(emitter, dependency);
                }
                else
                {
                    if (dependency.ServiceType.IsLazy())
                    {
                        Action<IEmitter> instanceEmitter = decoratorTargetEmitter;
                        decoratorTargetEmitter = CreateEmitMethodBasedOnLazyServiceRequest(
                            dependency.ServiceType, t => CreateTypedInstanceDelegate(instanceEmitter, t));
                    }

                    decoratorTargetEmitter(emitter);
                }
            }
        }

        private Delegate CreateTypedInstanceDelegate(Action<IEmitter> emitter, Type serviceType)
        {
            var openGenericMethod = GetType().GetTypeInfo().GetDeclaredMethod("CreateGenericDynamicMethodDelegate");
            var closedGenericMethod = openGenericMethod.MakeGenericMethod(serviceType);
            var del = WrapAsFuncDelegate(CreateDynamicMethodDelegate(emitter));
            return (Delegate)closedGenericMethod.Invoke(this, new object[] { del });
        }

        // ReSharper disable UnusedMember.Local
        private Func<T> CreateGenericDynamicMethodDelegate<T>(Func<object> del)

        // ReSharper restore UnusedMember.Local
        {
            return () => (T)del();
        }

        private void EmitConstructorDependency(IEmitter emitter, Dependency dependency)
        {
            var emitMethod = GetEmitMethodForDependency(dependency);

            try
            {
                emitMethod(emitter);
                emitter.UnboxOrCast(dependency.ServiceType);
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException(string.Format(UnresolvedDependencyError, dependency), ex);
            }
        }

        private void EmitPropertyDependency(IEmitter emitter, PropertyDependency propertyDependency, LocalBuilder instanceVariable)
        {
            var propertyDependencyEmitMethod = GetEmitMethodForDependency(propertyDependency);

            if (propertyDependencyEmitMethod == null)
            {
                return;
            }

            emitter.Push(instanceVariable);
            propertyDependencyEmitMethod(emitter);
            emitter.UnboxOrCast(propertyDependency.ServiceType);
            emitter.Call(propertyDependency.Property.SetMethod);
        }

        private Action<IEmitter> GetEmitMethodForDependency(Dependency dependency)
        {
            if (dependency.FactoryExpression != null)
            {
                return skeleton => EmitDependencyUsingFactoryExpression(skeleton, dependency);
            }

            Action<IEmitter> emitter = null;
            var dependencyName = string.IsNullOrWhiteSpace(dependency.ServiceName) ? dependency.Name : dependency.ServiceName;

            var registrations = GetEmitMethods(dependency.ServiceType);
            if (registrations.Count > 1)
            {
                if (registrations.TryGetValue(dependencyName, out emitter))
                {
                    return emitter;
                }
            }

            emitter = GetEmitMethod(dependency.ServiceType, dependency.ServiceName);
            if (emitter == null)
            {
                emitter = GetEmitMethod(dependency.ServiceType, dependency.Name);
                if (emitter == null && dependency.IsRequired)
                {
                    throw new InvalidOperationException(string.Format(UnresolvedDependencyError, dependency));
                }
            }

            return emitter;
        }

        private void EmitDependencyUsingFactoryExpression(IEmitter emitter, Dependency dependency)
        {
            var actions = new List<Action<IEmitter>>();
            var parameters = dependency.FactoryExpression.GetMethodInfo().GetParameters();

            foreach (var parameter in parameters)
            {
                if (parameter.ParameterType == typeof(IServiceFactory))
                {
                    actions.Add(e => e.PushConstant(constants.Add(this), typeof(IServiceFactory)));
                }

                if (parameter.ParameterType == typeof(ParameterInfo))
                {
                    actions.Add(e => e.PushConstant(constants.Add(((ConstructorDependency)dependency).Parameter), typeof(ParameterInfo)));
                }

                if (parameter.ParameterType == typeof(PropertyInfo))
                {
                    actions.Add(e => e.PushConstant(constants.Add(((PropertyDependency)dependency).Property), typeof(PropertyInfo)));
                }

                if (parameter.ParameterType == typeof(object[]))
                {
                    actions.Add(e => PushRuntimeArguments(e));
                }
            }

            var factoryDelegateIndex = constants.Add(dependency.FactoryExpression);
            Type funcType = dependency.FactoryExpression.GetType();
            MethodInfo invokeMethod = funcType.GetTypeInfo().GetDeclaredMethod("Invoke");
            emitter.PushConstant(factoryDelegateIndex, funcType);

            foreach (var action in actions)
            {
                action(emitter);
            }

            emitter.Call(invokeMethod);
        }

        private void EmitPropertyDependencies(ConstructionInfo constructionInfo, IEmitter emitter)
        {
            if (constructionInfo.PropertyDependencies.Count == 0)
            {
                return;
            }

            LocalBuilder instanceVariable = emitter.DeclareLocal(constructionInfo.ImplementingType);
            emitter.Store(instanceVariable);
            foreach (var propertyDependency in constructionInfo.PropertyDependencies)
            {
                EmitPropertyDependency(emitter, propertyDependency, instanceVariable);
            }

            emitter.Push(instanceVariable);
        }

        private Action<IEmitter> CreateEmitMethodForUnknownService(Type serviceType, string serviceName)
        {
            Action<IEmitter> emitter = null;
            if (CanRedirectRequestForDefaultServiceToSingleNamedService(serviceType, serviceName))
            {
                emitter = CreateServiceEmitterBasedOnSingleNamedInstance(serviceType);
            }
            else if (serviceType.IsLazy())
            {
                if (string.IsNullOrWhiteSpace(serviceName))
                {
                    emitter = CreateEmitMethodBasedOnLazyServiceRequest(serviceType, t => t.CreateGetInstanceDelegate(this));
                }
                else
                {
                    emitter = CreateEmitMethodBasedOnLazyServiceRequest(serviceType, t => t.CreateNamedGetInstanceDelegate(serviceName, this));
                }
            }
            else if (serviceType.IsFuncWithParameters())
            {
                emitter = CreateEmitMethodBasedParameterizedFuncRequest(serviceType, serviceName);
            }
            else if (serviceType.IsFunc())
            {
                emitter = CreateEmitMethodBasedOnFuncServiceRequest(serviceType, serviceName);
            }
            else if (serviceType.IsEnumerableOfT())
            {
                emitter = CreateEmitMethodBasedOnClosedGenericServiceRequest(serviceType, serviceName);
                if (emitter == null)
                {
                    emitter = CreateEmitMethodForEnumerableServiceServiceRequest(serviceType);
                }
            }
            else if (serviceType.IsArray)
            {
                emitter = CreateEmitMethodForArrayServiceRequest(serviceType);
            }
            else if (serviceType.IsReadOnlyCollectionOfT() || serviceType.IsReadOnlyListOfT())
            {
                emitter = CreateEmitMethodBasedOnClosedGenericServiceRequest(serviceType, serviceName);
                if (emitter == null)
                {
                    emitter = CreateEmitMethodForReadOnlyCollectionServiceRequest(serviceType);
                }
            }
            else if (serviceType.IsListOfT())
            {
                emitter = CreateEmitMethodBasedOnClosedGenericServiceRequest(serviceType, serviceName);
                if (emitter == null)
                {
                    emitter = CreateEmitMethodForListServiceRequest(serviceType);
                }
            }
            else if (serviceType.IsCollectionOfT())
            {
                emitter = CreateEmitMethodBasedOnClosedGenericServiceRequest(serviceType, serviceName);
                if (emitter == null)
                {
                    emitter = CreateEmitMethodForListServiceRequest(serviceType);
                }
            }
            else if (serviceType.IsClosedGeneric())
            {
                emitter = CreateEmitMethodBasedOnClosedGenericServiceRequest(serviceType, serviceName);
            }

            return emitter;
        }

        private Action<IEmitter> CreateEmitMethodBasedOnFuncServiceRequest(Type serviceType, string serviceName)
        {
            Delegate getInstanceDelegate;
            var returnType = serviceType.GetTypeInfo().GenericTypeArguments[0];
            if (string.IsNullOrEmpty(serviceName))
            {
                getInstanceDelegate = returnType.CreateGetInstanceDelegate(this);
            }
            else
            {
                getInstanceDelegate = returnType.CreateNamedGetInstanceDelegate(serviceName, this);
            }

            var constantIndex = constants.Add(getInstanceDelegate);
            return e => e.PushConstant(constantIndex, serviceType);
        }

        private Action<IEmitter> CreateEmitMethodBasedParameterizedFuncRequest(Type serviceType, string serviceName)
        {
            Delegate getInstanceDelegate;
            if (string.IsNullOrEmpty(serviceName))
            {
                getInstanceDelegate = CreateGetInstanceWithParametersDelegate(serviceType);
            }
            else
            {
                getInstanceDelegate = ReflectionHelper.CreateGetNamedInstanceWithParametersDelegate(
                    this,
                    serviceType,
                    serviceName);
            }

            var constantIndex = constants.Add(getInstanceDelegate);
            return e => e.PushConstant(constantIndex, serviceType);
        }

        private Delegate CreateGetInstanceWithParametersDelegate(Type serviceType)
        {
            var getInstanceMethod = ReflectionHelper.GetGetInstanceWithParametersMethod(serviceType);
            return getInstanceMethod.CreateDelegate(serviceType, this);
        }

        private Action<IEmitter> CreateServiceEmitterBasedOnFactoryRule(FactoryRule rule, Type serviceType, string serviceName)
        {
            var serviceRegistration = new ServiceRegistration
            {
                ServiceType = serviceType,
                ServiceName = serviceName,
                FactoryExpression = rule.Factory,
                Lifetime = CloneLifeTime(rule.LifeTime) ?? DefaultLifetime,
            };
            if (rule.LifeTime != null)
            {
                return emitter => EmitLifetime(serviceRegistration, e => EmitNewInstanceWithDecorators(serviceRegistration, e), emitter);
            }

            return emitter => EmitNewInstanceWithDecorators(serviceRegistration, emitter);
        }

        private Action<IEmitter> CreateEmitMethodForArrayServiceRequest(Type serviceType)
        {
            Action<IEmitter> enumerableEmitter = CreateEmitMethodForEnumerableServiceServiceRequest(serviceType);
            return enumerableEmitter;
        }

        private Action<IEmitter> CreateEmitMethodForListServiceRequest(Type serviceType)
        {
            // Note replace this with getEmitMethod();
            Action<IEmitter> enumerableEmitter = CreateEmitMethodForEnumerableServiceServiceRequest(serviceType);

            MethodInfo openGenericToArrayMethod = typeof(Enumerable).GetTypeInfo().GetDeclaredMethod("ToList");
            MethodInfo closedGenericToListMethod = openGenericToArrayMethod.MakeGenericMethod(TypeHelper.GetElementType(serviceType));
            return ms =>
            {
                enumerableEmitter(ms);
                ms.Emit(OpCodes.Call, closedGenericToListMethod);
            };
        }

        private Action<IEmitter> CreateEmitMethodForReadOnlyCollectionServiceRequest(Type serviceType)
        {
            Type elementType = TypeHelper.GetElementType(serviceType);
            Type closedGenericReadOnlyCollectionType = typeof(ReadOnlyCollection<>).MakeGenericType(elementType);
            ConstructorInfo constructorInfo =
                closedGenericReadOnlyCollectionType.GetTypeInfo().DeclaredConstructors.Single();

            Action<IEmitter> listEmitMethod = CreateEmitMethodForListServiceRequest(serviceType);

            return emitter =>
            {
                listEmitMethod(emitter);
                emitter.New(constructorInfo);
            };
        }

        private Action<IEmitter> CreateEmitMethodBasedOnLazyServiceRequest(Type serviceType, Func<Type, Delegate> valueFactoryDelegate)
        {
            Type actualServiceType = serviceType.GetTypeInfo().GenericTypeArguments[0];
            Type funcType = actualServiceType.GetFuncType();
            ConstructorInfo lazyConstructor = actualServiceType.GetLazyConstructor();
            Delegate getInstanceDelegate = valueFactoryDelegate(actualServiceType);
            var constantIndex = constants.Add(getInstanceDelegate);

            return emitter =>
            {
                emitter.PushConstant(constantIndex, funcType);
                emitter.New(lazyConstructor);
            };
        }

        private ThreadSafeDictionary<string, ServiceRegistration> GetOpenGenericServiceRegistrations(Type openGenericServiceType)
        {
            var services = GetAvailableServices(openGenericServiceType);
            return services;
        }

        private Action<IEmitter> CreateEmitMethodBasedOnClosedGenericServiceRequest(Type closedGenericServiceType, string serviceName)
        {
            Type openGenericServiceType = closedGenericServiceType.GetGenericTypeDefinition();
            var openGenericServiceRegistrations =
                GetOpenGenericServiceRegistrations(openGenericServiceType);

            Dictionary<string, (Type closedGenericImplentingType, ILifetime lifetime)> candidates = new Dictionary<string, (Type closedGenericImplentingType, ILifetime lifetime)>(StringComparer.OrdinalIgnoreCase);

            foreach (var openGenericServiceRegistration in openGenericServiceRegistrations.Values)
            {
                var closedGenericImplementingTypeCandidate = GenericArgumentMapper.TryMakeGenericType(closedGenericServiceType, openGenericServiceRegistration.ImplementingType);
                if (closedGenericImplementingTypeCandidate != null)
                {
                    candidates.Add(openGenericServiceRegistration.ServiceName, (closedGenericImplementingTypeCandidate, openGenericServiceRegistration.Lifetime));
                }
            }

            (Type closedGenericImplentingType, ILifetime lifetime) candidate;

            // We have a request for the default service
            if (string.IsNullOrWhiteSpace(serviceName))
            {
                var defaultServiceName = string.Empty;
                if (candidates.Count > 0)
                {
                    defaultServiceName = options.DefaultServiceSelector(candidates.Keys.OrderBy(k => k).ToArray());
                }

                if (candidates.TryGetValue(defaultServiceName, out candidate))
                {
                    return RegisterAndGetEmitMethod();
                }
                else if (candidates.Count == 1)
                {
                    candidate = candidates.First().Value;
                    return RegisterAndGetEmitMethod();
                }
                else
                {
                    return null;
                }
            }
            else
            {
                if (candidates.TryGetValue(serviceName, out candidate))
                {
                    return RegisterAndGetEmitMethod();
                }
                else
                {
                    return null;
                }
            }

            Action<IEmitter> RegisterAndGetEmitMethod()
            {
                var serviceRegistration = new ServiceRegistration
                {
                    ServiceType = closedGenericServiceType,
                    ImplementingType = candidate.closedGenericImplentingType,
                    ServiceName = serviceName,
                    Lifetime = CloneLifeTime(candidate.lifetime) ?? DefaultLifetime,
                };
                Register(serviceRegistration);
                return GetEmitMethod(serviceRegistration.ServiceType, serviceRegistration.ServiceName);
            }
        }

        private Action<IEmitter> CreateEmitMethodForEnumerableServiceServiceRequest(Type serviceType)
        {
            Type actualServiceType = TypeHelper.GetElementType(serviceType);

            if (actualServiceType.GetTypeInfo().IsGenericType)
            {
                Type openGenericServiceType = actualServiceType.GetGenericTypeDefinition();
                var openGenericServiceRegistrations = GetOpenGenericServiceRegistrations(openGenericServiceType);

                var constructableOpenGenericServices = openGenericServiceRegistrations.Values.Select(r => new { r.Lifetime, r.ServiceName, closedGenericImplementingType = GenericArgumentMapper.TryMakeGenericType(actualServiceType, r.ImplementingType) })
                .Where(t => t.closedGenericImplementingType != null);

                foreach (var constructableOpenGenericService in constructableOpenGenericServices)
                {
                    var serviceRegistration = new ServiceRegistration
                    {
                        ServiceType = actualServiceType,
                        ImplementingType = constructableOpenGenericService.closedGenericImplementingType,
                        ServiceName = constructableOpenGenericService.ServiceName,
                        Lifetime = CloneLifeTime(constructableOpenGenericService.Lifetime) ?? DefaultLifetime,
                    };
                    Register(serviceRegistration);
                }
            }

            List<Action<IEmitter>> emitMethods;

            if (options.EnableVariance && options.VarianceFilter(serviceType))
            {
                emitMethods = emitters
                    .Where(kv => actualServiceType.GetTypeInfo().IsAssignableFrom(kv.Key.GetTypeInfo()))
                    .SelectMany(kv => kv.Value).OrderBy(kv => kv.Key).Select(kv => kv.Value)
                    .ToList();
            }
            else
            {
                emitMethods = GetEmitMethods(actualServiceType).OrderBy(kv => kv.Key).Select(kv => kv.Value).ToList();
            }

            if (dependencyStack.Count > 0 && emitMethods.Contains(dependencyStack.Peek()))
            {
                emitMethods.Remove(dependencyStack.Peek());
            }

            return e => EmitEnumerable(emitMethods, actualServiceType, e);
        }

        private Action<IEmitter> CreateServiceEmitterBasedOnSingleNamedInstance(Type serviceType)
        {
            return GetEmitMethod(serviceType, GetEmitMethods(serviceType).First().Key);
        }

        private bool CanRedirectRequestForDefaultServiceToSingleNamedService(Type serviceType, string serviceName)
        {
            return string.IsNullOrEmpty(serviceName) && GetEmitMethods(serviceType).Count == 1;
        }

        private ConstructionInfo GetConstructionInfo(Registration registration)
        {
            return constructionInfoProvider.Value.GetConstructionInfo(registration);
        }

        private ThreadSafeDictionary<string, Action<IEmitter>> GetEmitMethods(Type serviceType)
        {
            return emitters.GetOrAdd(serviceType, s => new ThreadSafeDictionary<string, Action<IEmitter>>(StringComparer.CurrentCultureIgnoreCase));
        }

        private ThreadSafeDictionary<string, ServiceRegistration> GetAvailableServices(Type serviceType)
        {
            return availableServices.GetOrAdd(serviceType, s => new ThreadSafeDictionary<string, ServiceRegistration>(StringComparer.CurrentCultureIgnoreCase));
        }

        private ThreadSafeDictionary<string, Delegate> GetConstructorDependencyFactories(Type dependencyType)
        {
            return constructorDependencyFactories.GetOrAdd(
                dependencyType,
                d => new ThreadSafeDictionary<string, Delegate>(StringComparer.CurrentCultureIgnoreCase));
        }

        private ThreadSafeDictionary<string, Delegate> GetPropertyDependencyFactories(Type dependencyType)
        {
            return propertyDependencyFactories.GetOrAdd(
                dependencyType,
                d => new ThreadSafeDictionary<string, Delegate>(StringComparer.CurrentCultureIgnoreCase));
        }

        private void RegisterService(Type serviceType, Type implementingType, ILifetime lifetime, string serviceName)
        {
            Ensure.IsNotNull(serviceType, "type");
            Ensure.IsNotNull(implementingType, "implementingType");
            Ensure.IsNotNull(serviceName, "serviceName");
            EnsureConstructable(serviceType, implementingType);
            var serviceRegistration = new ServiceRegistration { ServiceType = serviceType, ImplementingType = implementingType, ServiceName = serviceName, Lifetime = lifetime ?? DefaultLifetime };
            Register(serviceRegistration);
        }

        private void EnsureConstructable(Type serviceType, Type implementingType)
        {
            if (implementingType.GetTypeInfo().ContainsGenericParameters)
            {
                try
                {
                    GenericArgumentMapper.Map(serviceType, implementingType).GetMappedArguments();
                }
                catch (InvalidOperationException ex)
                {
                    throw new ArgumentOutOfRangeException(nameof(implementingType), ex.Message);
                }
            }
            else
            if (!serviceType.GetTypeInfo().IsAssignableFrom(implementingType.GetTypeInfo()))
            {
                throw new ArgumentOutOfRangeException(nameof(implementingType), $"The implementing type {implementingType.FullName} is not assignable from {serviceType.FullName}.");
            }
        }

        private Action<IEmitter> ResolveEmitMethod(ServiceRegistration serviceRegistration)
        {
            if (serviceRegistration.Lifetime == null)
            {
                return methodSkeleton => EmitNewInstanceWithDecorators(serviceRegistration, methodSkeleton);
            }

            return methodSkeleton => EmitLifetime(serviceRegistration, ms => EmitNewInstanceWithDecorators(serviceRegistration, ms), methodSkeleton);
        }

        private void EmitLifetime(ServiceRegistration serviceRegistration, Action<IEmitter> emitMethod, IEmitter emitter)
        {
            if (serviceRegistration.Lifetime is PerContainerLifetime)
            {
                Func<object> instanceDelegate =
                    () => WrapAsFuncDelegate(CreateDynamicMethodDelegate(emitMethod))();
                var instance = serviceRegistration.Lifetime.GetInstance(instanceDelegate, null);
                var instanceIndex = constants.Add(instance);
                emitter.PushConstant(instanceIndex, instance.GetType());
            }
            else
            {
                int instanceDelegateIndex = servicesToDelegatesIndex.GetOrAdd(serviceRegistration, _ => CreateInstanceDelegateIndex(emitMethod));
                int lifetimeIndex = CreateLifetimeIndex(serviceRegistration.Lifetime);
                int scopeManagerIndex = CreateScopeManagerIndex();
                var getInstanceMethod = LifetimeHelper.GetInstanceMethod;
                emitter.PushConstant(lifetimeIndex, typeof(ILifetime));
                emitter.PushConstant(instanceDelegateIndex, typeof(Func<object>));
                emitter.PushConstant(scopeManagerIndex, typeof(IScopeManager));
                emitter.Call(LifetimeHelper.GetCurrentScopeMethod);
                emitter.Call(getInstanceMethod);
            }

            if (IsNotServiceFactory(serviceRegistration.ServiceType))
            {
                disposableLifeTimes.Add(serviceRegistration.Lifetime);
            }

            bool IsNotServiceFactory(Type serviceType)
            {
                return !typeof(IServiceFactory).GetTypeInfo().IsAssignableFrom(serviceType.GetTypeInfo());
            }
        }

        private int CreateScopeManagerIndex()
        {
            return constants.Add(ScopeManagerProvider.GetScopeManager(this));
        }

        private int CreateInstanceDelegateIndex(Action<IEmitter> emitMethod)
        {
            return constants.Add(WrapAsFuncDelegate(CreateDynamicMethodDelegate(emitMethod)));
        }

        private int CreateLifetimeIndex(ILifetime lifetime)
        {
            return constants.Add(lifetime);
        }

        private GetInstanceDelegate CreateDefaultDelegate(Type serviceType, bool throwError)
        {
            log.Info($"Compiling delegate for resolving service : {serviceType}");

            var instanceDelegate = CreateDelegate(serviceType, string.Empty, throwError);
            if (instanceDelegate == null)
            {
                return c => null;
            }

            Interlocked.Exchange(ref delegates, delegates.Add(serviceType, instanceDelegate));
            return instanceDelegate;
        }

        private GetInstanceDelegate CreateNamedDelegate(Tuple<Type, string> key, bool throwError)
        {
            log.Info($"Compiling delegate for resolving service : {key.Item1}, name: {key.Item2}");
            var instanceDelegate = CreateDelegate(key.Item1, key.Item2, throwError);
            if (instanceDelegate == null)
            {
                return c => null;
            }

            Interlocked.Exchange(ref namedDelegates, namedDelegates.Add(key, instanceDelegate));
            return instanceDelegate;
        }

        private GetInstanceDelegate CreateDelegate(Type serviceType, string serviceName, bool throwError)
        {
            lock (lockObject)
            {
                var serviceEmitter = GetEmitMethod(serviceType, serviceName);
                if (serviceEmitter == null && throwError)
                {
                    throw new InvalidOperationException(
                        string.Format("Unable to resolve type: {0}, service name: {1}", serviceType, serviceName));
                }

                if (serviceEmitter != null)
                {
                    try
                    {
                        return CreateDynamicMethodDelegate(serviceEmitter);
                    }
                    catch (InvalidOperationException ex)
                    {
                        dependencyStack.Clear();
                        throw new InvalidOperationException(
                            string.Format("Unable to resolve type: {0}, service name: {1}", serviceType, serviceName),
                            ex);
                    }
                }

                return null;
            }
        }

        private void RegisterValue(Type serviceType, object value, string serviceName)
        {
            var serviceRegistration = new ServiceRegistration
            {
                ServiceType = serviceType,
                ServiceName = serviceName,
                Value = value,
                Lifetime = new PerContainerLifetime(),
            };
            Register(serviceRegistration);
        }

        private void RegisterServiceFromLambdaExpression<TService>(Delegate factory, ILifetime lifetime, string serviceName)
        {
            var serviceRegistration = new ServiceRegistration
            {
                ServiceType = typeof(TService),
                FactoryExpression = factory,
                ServiceName = serviceName,
                Lifetime = lifetime ?? DefaultLifetime,
            };
            Register(serviceRegistration);
        }

        private class Storage<T>
        {
            public T[] Items = new T[0];

            private readonly object lockObject = new object();

            public int Add(T value)
            {
                int index = Array.IndexOf(Items, value);
                if (index == -1)
                {
                    return TryAddValue(value);
                }

                return index;
            }

            private int TryAddValue(T value)
            {
                lock (lockObject)
                {
                    int index = Array.IndexOf(Items, value);
                    if (index == -1)
                    {
                        index = AddValue(value);
                    }

                    return index;
                }
            }

            private int AddValue(T value)
            {
                int index = Items.Length;
                T[] snapshot = CreateSnapshot();
                snapshot[index] = value;
                Items = snapshot;
                return index;
            }

            private T[] CreateSnapshot()
            {
                var snapshot = new T[Items.Length + 1];
                Array.Copy(Items, snapshot, Items.Length);
                return snapshot;
            }
        }

        private class PropertyDependencyDisabler : IPropertyDependencySelector
        {
            public IEnumerable<PropertyDependency> Execute(Type type)
            {
                return new PropertyDependency[0];
            }
        }

        private class DynamicMethodSkeleton : IMethodSkeleton
        {
            private IEmitter emitter;
            private DynamicMethod dynamicMethod;

            public DynamicMethodSkeleton(Type returnType, Type[] parameterTypes)
            {
                CreateDynamicMethod(returnType, parameterTypes);
            }

            public IEmitter GetEmitter()
            {
                return emitter;
            }

            public Delegate CreateDelegate(Type delegateType)
            {
                return dynamicMethod.CreateDelegate(delegateType);
            }

#if NET452 || NET46 || NETCOREAPP2_0
            private void CreateDynamicMethod(Type returnType, Type[] parameterTypes)
            {
                dynamicMethod = new DynamicMethod(
                    "DynamicMethod", returnType, parameterTypes, typeof(ServiceContainer).GetTypeInfo().Module, true);
                emitter = new Emitter(dynamicMethod.GetILGenerator(), parameterTypes);
            }
#endif
#if NETSTANDARD1_1 || NETSTANDARD1_3 || NETSTANDARD1_6 || NETSTANDARD2_0
            private void CreateDynamicMethod(Type returnType, Type[] parameterTypes)
            {
                dynamicMethod = new DynamicMethod(returnType, parameterTypes);
                emitter = new Emitter(dynamicMethod.GetILGenerator(), parameterTypes);
            }
#endif
        }

        private class ServiceRegistry<T> : ThreadSafeDictionary<Type, ThreadSafeDictionary<string, T>>
        {
        }

        private class FactoryRule
        {
            public Func<Type, string, bool> CanCreateInstance { get; set; }

            public Func<ServiceRequest, object> Factory { get; set; }

            public ILifetime LifeTime { get; set; }
        }

        private class Initializer
        {
            public Func<ServiceRegistration, bool> Predicate { get; set; }

            public Action<IServiceFactory, object> Initialize { get; set; }
        }

        private class ServiceOverride
        {
            public Func<ServiceRegistration, bool> CanOverride { get; set; }

            public Func<IServiceFactory, ServiceRegistration, ServiceRegistration> ServiceRegistrationFactory { get; set; }
        }
    }
}