namespace LightInject
{
   /// <summary>
    /// Represents a class that maps the generic arguments/parameters from a generic servicetype
    /// to a open generic implementing type.
    /// </summary>
    public interface IGenericArgumentMapper
    {
        /// <summary>
        /// Maps the generic arguments/parameters from the <paramref name="genericServiceType"/>
        /// to the generic arguments/parameters in the <paramref name="openGenericImplementingType"/>.
        /// </summary>
        /// <param name="genericServiceType">The generic type containing the arguments/parameters to be mapped to the generic arguments/parameters of the <paramref name="openGenericImplementingType"/>.</param>
        /// <param name="openGenericImplementingType">The open generic implementing type.</param>
        /// <returns>A <see cref="GenericMappingResult"/>.</returns>
        GenericMappingResult Map(Type genericServiceType, Type openGenericImplementingType);

        /// <summary>
        /// Attempts to create a generic type based on the <paramref name="genericServiceType"/> and the <paramref name="openGenericImplementingType"/>.
        /// </summary>
        /// <param name="genericServiceType">The generic type containing the arguments/parameters to be mapped to the generic arguments/parameters of the <paramref name="openGenericImplementingType"/>.</param>
        /// <param name="openGenericImplementingType">The open generic implementing type.</param>
        /// <returns>The closed generic type if successful, otherwise null.</returns>
        Type TryMakeGenericType(Type genericServiceType, Type openGenericImplementingType);
    }
}