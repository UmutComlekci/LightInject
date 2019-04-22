namespace LightInject
{
    /// <summary>
    /// Represents a class that manages <see cref="Scope"/> instances.
    /// </summary>
    public interface IScopeManager
    {
        /// <summary>
        /// Gets or sets the current <see cref="Scope"/>.
        /// </summary>
        Scope CurrentScope { get; set; }

        /// <summary>
        /// Gets the <see cref="IServiceFactory"/> that is associated with this <see cref="IScopeManager"/>.
        /// </summary>
        IServiceFactory ServiceFactory { get; }

        /// <summary>
        /// Starts a new <see cref="Scope"/>.
        /// </summary>
        /// <returns>A new <see cref="Scope"/>.</returns>
        Scope BeginScope();

        /// <summary>
        /// Ends the given <paramref name="scope"/>.
        /// </summary>
        /// <param name="scope">The scope to be ended.</param>
        void EndScope(Scope scope);
    }
}