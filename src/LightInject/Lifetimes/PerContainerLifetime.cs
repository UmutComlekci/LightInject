namespace LightInject
{
     /// <summary>
    /// Ensures that only one instance of a given service can exist within the current <see cref="IServiceContainer"/>.
    /// </summary>
    public class PerContainerLifetime : ILifetime, IDisposable, ICloneableLifeTime
    {
        private readonly object syncRoot = new object();
        private volatile object singleton;

        /// <summary>
        /// Returns a service instance according to the specific lifetime characteristics.
        /// </summary>
        /// <param name="createInstance">The function delegate used to create a new service instance.</param>
        /// <param name="scope">The <see cref="Scope"/> of the current service request.</param>
        /// <returns>The requested services instance.</returns>
        public object GetInstance(Func<object> createInstance, Scope scope)
        {
            if (singleton != null)
            {
                return singleton;
            }

            lock (syncRoot)
            {
                if (singleton == null)
                {
                    singleton = createInstance();
                }
            }

            return singleton;
        }

        /// <summary>
        /// Disposes the service instances managed by this <see cref="PerContainerLifetime"/> instance.
        /// </summary>
        public void Dispose()
        {
            if (singleton is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }

        /// <inheritdoc/>
        public ILifetime Clone()
        {
            return new PerContainerLifetime();
        }
    }
}