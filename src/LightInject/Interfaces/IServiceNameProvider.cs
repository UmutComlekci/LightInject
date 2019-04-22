namespace LightInject
{
   /// <summary>
    /// Represents a class that is capable of providing a service name
    /// to be used when a service is registered during assembly scanning.
    /// </summary>
    public interface IServiceNameProvider
    {
        /// <summary>
        /// Gets the service name for which the given <paramref name="serviceType"/> will be registered.
        /// </summary>
        /// <param name="serviceType">The service type for which to provide a service name.</param>
        /// <param name="implementingType">The implementing type for which to provide a service name.</param>
        /// <returns>The service name for which the <paramref name="serviceType"/> and <paramref name="implementingType"/> will be registered.</returns>
        string GetServiceName(Type serviceType, Type implementingType);
    }
}