namespace LightInject
{
   /// <summary>
    /// A <see cref="IScopeManagerProvider"/> that provides a <see cref="PerThreadScopeManager"/> per thread.
    /// </summary>
    public class PerThreadScopeManagerProvider : ScopeManagerProvider
    {
        /// <summary>
        /// Creates a new <see cref="IScopeManager"/> instance.
        /// </summary>
        /// <param name="serviceFactory">The <see cref="IServiceFactory"/> to be associated with the <see cref="IScopeManager"/>.</param>
        /// <returns><see cref="IScopeManager"/>.</returns>
        protected override IScopeManager CreateScopeManager(IServiceFactory serviceFactory)
        {
            return new PerThreadScopeManager(serviceFactory);
        }
    }
}