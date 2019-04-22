namespace LightInject
{
   /// <summary>
    /// Represents a class that acts as a composition root for an <see cref="IServiceRegistry"/> instance.
    /// </summary>
    public interface ICompositionRoot
    {
        /// <summary>
        /// Composes services by adding services to the <paramref name="serviceRegistry"/>.
        /// </summary>
        /// <param name="serviceRegistry">The target <see cref="IServiceRegistry"/>.</param>
        void Compose(IServiceRegistry serviceRegistry);
    }
}