namespace LightInject
{
    /// <summary>
    /// Builds a <see cref="ConstructionInfo"/> instance based on the implementing <see cref="Type"/>.
    /// </summary>
    public class TypeConstructionInfoBuilder : IConstructionInfoBuilder
    {
        private readonly IConstructorSelector constructorSelector;
        private readonly IConstructorDependencySelector constructorDependencySelector;
        private readonly IPropertyDependencySelector propertyDependencySelector;
        private readonly Func<Type, string, Delegate> getConstructorDependencyExpression;

        private readonly Func<Type, string, Delegate> getPropertyDependencyExpression;

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeConstructionInfoBuilder"/> class.
        /// </summary>
        /// <param name="constructorSelector">The <see cref="IConstructorSelector"/> that is responsible
        /// for selecting the constructor to be used for constructor injection.</param>
        /// <param name="constructorDependencySelector">The <see cref="IConstructorDependencySelector"/> that is
        /// responsible for selecting the constructor dependencies for a given <see cref="ConstructionInfo"/>.</param>
        /// <param name="propertyDependencySelector">The <see cref="IPropertyDependencySelector"/> that is responsible
        /// for selecting the property dependencies for a given <see cref="Type"/>.</param>
        /// <param name="getConstructorDependencyExpression">A function delegate that returns the registered constructor dependency expression, if any.</param>
        /// <param name="getPropertyDependencyExpression">A function delegate that returns the registered property dependency expression, if any.</param>
        public TypeConstructionInfoBuilder(
            IConstructorSelector constructorSelector,
            IConstructorDependencySelector constructorDependencySelector,
            IPropertyDependencySelector propertyDependencySelector,
            Func<Type, string, Delegate> getConstructorDependencyExpression,
            Func<Type, string, Delegate> getPropertyDependencyExpression)
        {
            this.constructorSelector = constructorSelector;
            this.constructorDependencySelector = constructorDependencySelector;
            this.propertyDependencySelector = propertyDependencySelector;
            this.getConstructorDependencyExpression = getConstructorDependencyExpression;
            this.getPropertyDependencyExpression = getPropertyDependencyExpression;
        }

        /// <summary>
        /// Analyzes the <paramref name="registration"/> and returns a <see cref="ConstructionInfo"/> instance.
        /// </summary>
        /// <param name="registration">The <see cref="Registration"/> that represents the implementing type to analyze.</param>
        /// <returns>A <see cref="ConstructionInfo"/> instance.</returns>
        public ConstructionInfo Execute(Registration registration)
        {
            if (registration.FactoryExpression != null)
            {
                return new ConstructionInfo() { FactoryDelegate = registration.FactoryExpression };
            }

            var implementingType = registration.ImplementingType;
            var constructionInfo = new ConstructionInfo
            {
                ImplementingType = implementingType,
            };
            constructionInfo.PropertyDependencies.AddRange(GetPropertyDependencies(implementingType));
            constructionInfo.Constructor = constructorSelector.Execute(implementingType);
            constructionInfo.ConstructorDependencies.AddRange(GetConstructorDependencies(constructionInfo.Constructor));

            return constructionInfo;
        }

        private IEnumerable<ConstructorDependency> GetConstructorDependencies(ConstructorInfo constructorInfo)
        {
            var constructorDependencies = constructorDependencySelector.Execute(constructorInfo).ToArray();
            foreach (var constructorDependency in constructorDependencies)
            {
                constructorDependency.FactoryExpression =
                    getConstructorDependencyExpression(
                        constructorDependency.ServiceType,
                        constructorDependency.ServiceName);
            }

            return constructorDependencies;
        }

        private IEnumerable<PropertyDependency> GetPropertyDependencies(Type implementingType)
        {
            var propertyDependencies = propertyDependencySelector.Execute(implementingType).ToArray();
            foreach (var property in propertyDependencies)
            {
                property.FactoryExpression =
                    getPropertyDependencyExpression(
                        property.ServiceType,
                        property.ServiceName);
            }

            return propertyDependencies;
        }
    }
}