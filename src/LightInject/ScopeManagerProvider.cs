namespace LightInject
{
    /// <summary>
    /// A base class for implementing <see cref="IScopeManagerProvider"/>
    /// that ensures that only one <see cref="IScopeManager"/> is created.
    /// </summary>
    public abstract class ScopeManagerProvider : IScopeManagerProvider
    {
        private readonly object lockObject = new object();

        private IScopeManager scopeManager;

        /// <summary>
        /// Returns the <see cref="IScopeManager"/> that is responsible for managing scopes.
        /// </summary>
        /// <param name="serviceFactory">The <see cref="IServiceFactory"/> to be associated with this <see cref="ScopeManager"/>.</param>
        /// <returns>The <see cref="IScopeManager"/> that is responsible for managing scopes.</returns>
        public IScopeManager GetScopeManager(IServiceFactory serviceFactory)
        {
            if (scopeManager == null)
            {
                lock (lockObject)
                {
                    if (scopeManager == null)
                    {
                        scopeManager = CreateScopeManager(serviceFactory);
                    }
                }
            }

            return scopeManager;
        }

        /// <summary>
        /// Creates a new <see cref="IScopeManager"/> instance.
        /// </summary>
        /// <param name="serviceFactory">The <see cref="IServiceFactory"/> to be associated with the <see cref="IScopeManager"/>.</param>
        /// <returns><see cref="IScopeManager"/>.</returns>
        protected abstract IScopeManager CreateScopeManager(IServiceFactory serviceFactory);
    }
}