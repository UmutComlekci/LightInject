namespace LightInject
{
    /// <summary>
    /// Represents a class that is capable of providing a <see cref="IScopeManager"/>.
    /// </summary>
    public interface IScopeManagerProvider
    {
        /// <summary>
        /// Returns the <see cref="IScopeManager"/> that is responsible for managing scopes.
        /// </summary>
        /// <param name="serviceFactory">The <see cref="IServiceFactory"/> to be associated with this <see cref="ScopeManager"/>.</param>
        /// <returns>The <see cref="IScopeManager"/> that is responsible for managing scopes.</returns>
        IScopeManager GetScopeManager(IServiceFactory serviceFactory);
    }
}