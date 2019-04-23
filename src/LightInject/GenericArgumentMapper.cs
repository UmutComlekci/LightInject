namespace LightInject
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// A class that maps the generic arguments/parameters from a generic servicetype
    /// to a open generic implementing type.
    /// </summary>
    public class GenericArgumentMapper : IGenericArgumentMapper
    {
        /// <summary>
        /// Maps the generic arguments/parameters from the <paramref name="genericServiceType"/>
        /// to the generic arguments/parameters in the <paramref name="openGenericImplementingType"/>.
        /// </summary>
        /// <param name="genericServiceType">The generic type containing the arguments/parameters to be mapped to the generic arguments/parameters of the <paramref name="openGenericImplementingType"/>.</param>
        /// <param name="openGenericImplementingType">The open generic implementing type.</param>
        /// <returns>A <see cref="GenericMappingResult"/>.</returns>
        public GenericMappingResult Map(Type genericServiceType, Type openGenericImplementingType)
        {
            string[] genericParameterNames =
                openGenericImplementingType.GetTypeInfo().GenericTypeParameters.Select(t => t.Name).ToArray();

            var genericArgumentMap = CreateMap(genericServiceType, openGenericImplementingType, genericParameterNames);

            return new GenericMappingResult(genericParameterNames, genericArgumentMap, genericServiceType, openGenericImplementingType);
        }

        /// <inheritdoc/>
        public Type TryMakeGenericType(Type genericServiceType, Type openGenericImplementingType)
        {
            var mappingResult = Map(genericServiceType, openGenericImplementingType);
            if (!mappingResult.IsValid)
            {
                return null;
            }
            else
            {
                try
                {
                    return openGenericImplementingType.MakeGenericType(mappingResult.GetMappedArguments());
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }

        private static Dictionary<string, Type> CreateMap(Type genericServiceType, Type openGenericImplementingType, string[] genericParameterNames)
        {
            var genericArgumentMap = new Dictionary<string, Type>(genericParameterNames.Length);

            var genericArguments = GetGenericArgumentsOrParameters(genericServiceType);

            if (genericArguments.Length > 0)
            {
                genericServiceType = genericServiceType.GetTypeInfo().GetGenericTypeDefinition();
            }
            else
            {
                return genericArgumentMap;
            }

            Type baseTypeImplementingOpenGenericServiceType = GetBaseTypeImplementingGenericTypeDefinition(
                openGenericImplementingType,
                genericServiceType);

            Type[] baseTypeGenericArguments = GetGenericArgumentsOrParameters(baseTypeImplementingOpenGenericServiceType);

            MapGenericArguments(genericArguments, baseTypeGenericArguments, genericArgumentMap);
            return genericArgumentMap;
        }

        private static Type[] GetGenericArgumentsOrParameters(Type type)
        {
            TypeInfo typeInfo = type.GetTypeInfo();
            if (typeInfo.IsGenericTypeDefinition)
            {
                return typeInfo.GenericTypeParameters;
            }

            return typeInfo.GenericTypeArguments;
        }

        private static void MapGenericArguments(Type[] serviceTypeGenericArguments, Type[] baseTypeGenericArguments, IDictionary<string, Type> map)
        {
            for (int index = 0; index < baseTypeGenericArguments.Length; index++)
            {
                var baseTypeGenericArgument = baseTypeGenericArguments[index];
                var serviceTypeGenericArgument = serviceTypeGenericArguments[index];
                if (baseTypeGenericArgument.GetTypeInfo().IsGenericParameter)
                {
                    map[baseTypeGenericArgument.Name] = serviceTypeGenericArgument;
                }
                else if (baseTypeGenericArgument.GetTypeInfo().IsGenericType)
                {
                    if (serviceTypeGenericArgument.GetTypeInfo().IsGenericType)
                    {
                        MapGenericArguments(serviceTypeGenericArgument.GetTypeInfo().GenericTypeArguments, baseTypeGenericArgument.GetTypeInfo().GenericTypeArguments, map);
                    }
                    else
                    {
                        MapGenericArguments(serviceTypeGenericArguments, baseTypeGenericArgument.GetTypeInfo().GenericTypeArguments, map);
                    }
                }
            }
        }

        private static Type GetBaseTypeImplementingGenericTypeDefinition(Type implementingType, Type genericTypeDefinition)
        {
            Type baseTypeImplementingGenericTypeDefinition = null;

            if (genericTypeDefinition.GetTypeInfo().IsInterface)
            {
                baseTypeImplementingGenericTypeDefinition = implementingType
                    .GetTypeInfo().ImplementedInterfaces
                    .FirstOrDefault(i => i.GetTypeInfo().IsGenericType && i.GetTypeInfo().GetGenericTypeDefinition() == genericTypeDefinition);
            }
            else
            {
                Type baseType = implementingType;
                while (!ImplementsOpenGenericTypeDefinition(genericTypeDefinition, baseType) && baseType != typeof(object))
                {
                    baseType = baseType.GetTypeInfo().BaseType;
                }

                if (baseType != typeof(object))
                {
                    baseTypeImplementingGenericTypeDefinition = baseType;
                }
            }

            if (baseTypeImplementingGenericTypeDefinition == null)
            {
                throw new InvalidOperationException($"The generic type definition {genericTypeDefinition.FullName} not implemented by implementing type {implementingType.FullName}");
            }

            return baseTypeImplementingGenericTypeDefinition;
        }

        private static bool ImplementsOpenGenericTypeDefinition(Type genericTypeDefinition, Type baseType)
        {
            return baseType.GetTypeInfo().IsGenericType && baseType.GetTypeInfo().GetGenericTypeDefinition() == genericTypeDefinition;
        }
    }
}