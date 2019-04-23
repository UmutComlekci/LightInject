namespace LightInject
{
    using System.Reflection;

    /// <summary>
    /// Represents a class that is capable of extracting
    /// attributes of type <see cref="CompositionRootTypeAttribute"/> from an <see cref="Assembly"/>.
    /// </summary>
    public interface ICompositionRootAttributeExtractor
    {
        /// <summary>
        /// Gets a list of attributes of type <see cref="CompositionRootTypeAttribute"/> from
        /// the given <paramref name="assembly"/>.
        /// </summary>
        /// <param name="assembly">The assembly from which to extract
        /// <see cref="CompositionRootTypeAttribute"/> attributes.</param>
        /// <returns>A list of attributes of type <see cref="CompositionRootTypeAttribute"/>.</returns>
        CompositionRootTypeAttribute[] GetAttributes(Assembly assembly);
    }
}