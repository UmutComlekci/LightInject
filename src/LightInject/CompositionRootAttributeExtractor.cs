namespace LightInject
{
    /// <summary>
    /// A class that is capable of extracting attributes of type
    /// <see cref="CompositionRootTypeAttribute"/> from an <see cref="Assembly"/>.
    /// </summary>
    public class CompositionRootAttributeExtractor : ICompositionRootAttributeExtractor
    {
        /// <summary>
        /// Gets a list of attributes of type <see cref="CompositionRootTypeAttribute"/> from
        /// the given <paramref name="assembly"/>.
        /// </summary>
        /// <param name="assembly">The assembly from which to extract
        /// <see cref="CompositionRootTypeAttribute"/> attributes.</param>
        /// <returns>A list of attributes of type <see cref="CompositionRootTypeAttribute"/>.</returns>
        public CompositionRootTypeAttribute[] GetAttributes(Assembly assembly)
        {
            return assembly.GetCustomAttributes(typeof(CompositionRootTypeAttribute))
                .Cast<CompositionRootTypeAttribute>().ToArray();
        }
    }
}