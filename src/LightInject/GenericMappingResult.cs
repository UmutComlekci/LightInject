namespace LightInject
{
    /// <summary>
    /// Represents the result from mapping generic arguments.
    /// </summary>
    public class GenericMappingResult
    {
        private readonly string[] genericParameterNames;
        private readonly IDictionary<string, Type> genericArgumentMap;
        private readonly Type genericServiceType;
        private readonly Type openGenericImplementingType;

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericMappingResult"/> class.
        /// </summary>
        /// <param name="genericParameterNames">The name of the generic parameters found in the <paramref name="openGenericImplementingType"/>.</param>
        /// <param name="genericArgumentMap">A <see cref="IDictionary{TKey,TValue}"/> that contains the mapping
        /// between a parameter name and the corresponding parameter or argument from the <paramref name="genericServiceType"/>.</param>
        /// <param name="genericServiceType">The generic type containing the arguments/parameters to be mapped to the generic arguments/parameters of the <paramref name="openGenericImplementingType"/>.</param>
        /// <param name="openGenericImplementingType">The open generic implementing type.</param>
        internal GenericMappingResult(string[] genericParameterNames, IDictionary<string, Type> genericArgumentMap, Type genericServiceType, Type openGenericImplementingType)
        {
            this.genericParameterNames = genericParameterNames;
            this.genericArgumentMap = genericArgumentMap;
            this.genericServiceType = genericServiceType;
            this.openGenericImplementingType = openGenericImplementingType;
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="GenericMappingResult"/> is valid.
        /// </summary>
        public bool IsValid
        {
            get
            {
                if (!genericServiceType.GetTypeInfo().IsGenericType && openGenericImplementingType.GetTypeInfo().ContainsGenericParameters)
                {
                    return false;
                }

                return genericParameterNames.All(n => genericArgumentMap.ContainsKey(n));
            }
        }

        /// <summary>
        /// Gets a list of the mapped arguments/parameters.
        /// In the case of an closed generic service, this list can be used to
        /// create a new generic type from the open generic implementing type.
        /// </summary>
        /// <returns>A list of the mapped arguments/parameters.</returns>
        public Type[] GetMappedArguments()
        {
            var missingParameters = genericParameterNames.Where(n => !genericArgumentMap.ContainsKey(n)).ToArray();
            if (missingParameters.Any())
            {
                var missingParametersString = missingParameters.Aggregate((current, next) => current + "," + next);
                string message = $"The generic parameter(s) {missingParametersString} found in type {openGenericImplementingType.FullName} cannot be mapped from {genericServiceType.FullName}";
                throw new InvalidOperationException(message);
            }

            return genericParameterNames.Select(parameterName => genericArgumentMap[parameterName]).ToArray();
        }
    }
}