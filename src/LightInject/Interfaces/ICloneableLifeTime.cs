namespace LightInject
{
     /// <summary>
    /// Optionally implemented by <see cref="ILifetime"/> implementations
    /// to provide a way to clone the lifetime.
    /// </summary>
    public interface ICloneableLifeTime
    {
        /// <summary>
        /// Returns a clone of this <see cref="ILifetime"/>.
        /// </summary>
        /// <returns><see cref="ILifetime"/>.</returns>
        ILifetime Clone();
    }
}