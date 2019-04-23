namespace LightInject
{
    using System;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Extracts concrete <see cref="ICompositionRoot"/> implementations from an <see cref="Assembly"/>.
    /// </summary>
    public class CompositionRootTypeExtractor : ITypeExtractor
    {
        private readonly ICompositionRootAttributeExtractor compositionRootAttributeExtractor;

        /// <summary>
        /// Initializes a new instance of the <see cref="CompositionRootTypeExtractor"/> class.
        /// </summary>
        /// <param name="compositionRootAttributeExtractor">The <see cref="ICompositionRootAttributeExtractor"/>
        /// that is responsible for extracting attributes of type <see cref="CompositionRootTypeAttribute"/> from
        /// a given <see cref="Assembly"/>.</param>
        public CompositionRootTypeExtractor(ICompositionRootAttributeExtractor compositionRootAttributeExtractor)
        {
            this.compositionRootAttributeExtractor = compositionRootAttributeExtractor;
        }

        /// <summary>
        /// Extracts concrete <see cref="ICompositionRoot"/> implementations found in the given <paramref name="assembly"/>.
        /// </summary>
        /// <param name="assembly">The <see cref="Assembly"/> for which to extract types.</param>
        /// <returns>A set of concrete <see cref="ICompositionRoot"/> implementations found in the given <paramref name="assembly"/>.</returns>
        public Type[] Execute(Assembly assembly)
        {
            CompositionRootTypeAttribute[] compositionRootAttributes =
                compositionRootAttributeExtractor.GetAttributes(assembly);

            if (compositionRootAttributes.Length > 0)
            {
                return compositionRootAttributes.Select(a => a.CompositionRootType).ToArray();
            }

            return
                assembly.DefinedTypes.Where(
                        t => !t.IsAbstract && typeof(ICompositionRoot).GetTypeInfo().IsAssignableFrom(t))
                    .Cast<Type>()
                    .ToArray();
        }
    }
}