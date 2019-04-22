namespace LightInject
{
   /// <summary>
    /// Represents an inversion of control container.
    /// </summary>
    public interface IServiceContainer : IServiceRegistry, IServiceFactory, IDisposable
    {
        /// <summary>
        /// Gets or sets the <see cref="IScopeManagerProvider"/> that is responsible
        /// for providing the <see cref="IScopeManager"/> used to manage scopes.
        /// </summary>
        IScopeManagerProvider ScopeManagerProvider { get; set; }

        /// <summary>
        /// Returns <b>true</b> if the container can create the requested service, otherwise <b>false</b>.
        /// </summary>
        /// <param name="serviceType">The <see cref="Type"/> of the service.</param>
        /// <param name="serviceName">The name of the service.</param>
        /// <returns><b>true</b> if the container can create the requested service, otherwise <b>false</b>.</returns>
        bool CanGetInstance(Type serviceType, string serviceName);

        /// <summary>
        /// Injects the property dependencies for a given <paramref name="instance"/>.
        /// </summary>
        /// <param name="instance">The target instance for which to inject its property dependencies.</param>
        /// <returns>The <paramref name="instance"/> with its property dependencies injected.</returns>
        object InjectProperties(object instance);

        /// <summary>
        /// Compiles all registered services.
        /// </summary>
        void Compile();

        /// <summary>
        /// Compiles services that matches the given <paramref name="predicate"/>.
        /// </summary>
        /// <param name="predicate">The predicate that determines if a service should be compiled.</param>
        void Compile(Func<ServiceRegistration, bool> predicate);

        /// <summary>
        /// Compiles the service identified by <typeparamref name="TService"/>
        /// and optionally the <paramref name="serviceName"/>.
        /// </summary>
        /// <typeparam name="TService">The service type to be compiled.</typeparam>
        /// <param name="serviceName">The name of the service to be compiled.</param>
        void Compile<TService>(string serviceName = null);
    }
}