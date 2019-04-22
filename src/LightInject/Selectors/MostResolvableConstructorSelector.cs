namespace LightInject
{
    /// <summary>
    /// Selects the <see cref="ConstructionInfo"/> from a given type that represents the most resolvable constructor.
    /// </summary>
    public class MostResolvableConstructorSelector : IConstructorSelector
    {
        private readonly Func<Type, string, bool> canGetInstance;

        /// <summary>
        /// Initializes a new instance of the <see cref="MostResolvableConstructorSelector"/> class.
        /// </summary>
        /// <param name="canGetInstance">A function delegate that determines if a service type can be resolved.</param>
        public MostResolvableConstructorSelector(Func<Type, string, bool> canGetInstance)
        {
            this.canGetInstance = canGetInstance;
        }

        /// <summary>
        /// Selects the constructor to be used when creating a new instance of the <paramref name="implementingType"/>.
        /// </summary>
        /// <param name="implementingType">The <see cref="Type"/> for which to return a <see cref="ConstructionInfo"/>.</param>
        /// <returns>A <see cref="ConstructionInfo"/> instance that represents the constructor to be used
        /// when creating a new instance of the <paramref name="implementingType"/>.</returns>
        public ConstructorInfo Execute(Type implementingType)
        {
            ConstructorInfo[] constructorCandidates = implementingType.GetTypeInfo().DeclaredConstructors.Where(c => c.IsPublic && !c.IsStatic).ToArray();
            if (constructorCandidates.Length == 0)
            {
                throw new InvalidOperationException("Missing public constructor for Type: " + implementingType.FullName);
            }

            if (constructorCandidates.Length == 1)
            {
                return constructorCandidates[0];
            }

            foreach (var constructorCandidate in constructorCandidates.OrderByDescending(c => c.GetParameters().Count()))
            {
                ParameterInfo[] parameters = constructorCandidate.GetParameters();
                if (CanCreateParameterDependencies(parameters))
                {
                    return constructorCandidate;
                }
            }

            throw new InvalidOperationException("No resolvable constructor found for Type: " + implementingType.FullName);
        }

        /// <summary>
        /// Gets the service name based on the given <paramref name="parameter"/>.
        /// </summary>
        /// <param name="parameter">The <see cref="ParameterInfo"/> for which to get the service name.</param>
        /// <returns>The name of the service for the given <paramref name="parameter"/>.</returns>
        protected virtual string GetServiceName(ParameterInfo parameter)
        {
            return parameter.Name;
        }

        private bool CanCreateParameterDependencies(IEnumerable<ParameterInfo> parameters)
        {
            return parameters.All(CanCreateParameterDependency);
        }

        private bool CanCreateParameterDependency(ParameterInfo parameterInfo)
        {
            return canGetInstance(parameterInfo.ParameterType, string.Empty) || canGetInstance(parameterInfo.ParameterType, GetServiceName(parameterInfo));
        }
    }
}