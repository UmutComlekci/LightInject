namespace LightInject
{
     /// <summary>
    /// Selects the constructor dependencies for a given <see cref="ConstructorInfo"/>.
    /// </summary>
    public class ConstructorDependencySelector : IConstructorDependencySelector
    {
        /// <summary>
        /// Selects the constructor dependencies for the given <paramref name="constructor"/>.
        /// </summary>
        /// <param name="constructor">The <see cref="ConstructionInfo"/> for which to select the constructor dependencies.</param>
        /// <returns>A list of <see cref="ConstructorDependency"/> instances that represents the constructor
        /// dependencies for the given <paramref name="constructor"/>.</returns>
        public virtual IEnumerable<ConstructorDependency> Execute(ConstructorInfo constructor)
        {
            return
                constructor.GetParameters()
                    .OrderBy(p => p.Position)
                    .Select(
                        p =>
                            new ConstructorDependency
                            {
                                ServiceName = string.Empty,
                                ServiceType = p.ParameterType,
                                Parameter = p,
                                IsRequired = true,
                            });
        }
    }
}