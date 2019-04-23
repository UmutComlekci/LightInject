namespace LightInject
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Extracts concrete types from an <see cref="Assembly"/>.
    /// </summary>
    public class ConcreteTypeExtractor : ITypeExtractor
    {
        private static readonly List<Type> InternalTypes = new List<Type>();

        static ConcreteTypeExtractor()
        {
            InternalTypes.Add(typeof(ConstructorDependency));
            InternalTypes.Add(typeof(PropertyDependency));
            InternalTypes.Add(typeof(ThreadSafeDictionary<,>));
            InternalTypes.Add(typeof(Scope));
            InternalTypes.Add(typeof(PerContainerLifetime));
            InternalTypes.Add(typeof(PerScopeLifetime));
            InternalTypes.Add(typeof(ServiceRegistration));
            InternalTypes.Add(typeof(DecoratorRegistration));
            InternalTypes.Add(typeof(ServiceRequest));
            InternalTypes.Add(typeof(Registration));
            InternalTypes.Add(typeof(ServiceContainer));
            InternalTypes.Add(typeof(ConstructionInfo));
#if NET452 || NET46 || NETSTANDARD1_6 || NETSTANDARD2_0 || NETCOREAPP2_0
            InternalTypes.Add(typeof(AssemblyLoader));
#endif
            InternalTypes.Add(typeof(TypeConstructionInfoBuilder));
            InternalTypes.Add(typeof(ConstructionInfoProvider));
            InternalTypes.Add(typeof(MostResolvableConstructorSelector));
            InternalTypes.Add(typeof(PerRequestLifeTime));
            InternalTypes.Add(typeof(PropertySelector));
            InternalTypes.Add(typeof(AssemblyScanner));
            InternalTypes.Add(typeof(ConstructorDependencySelector));
            InternalTypes.Add(typeof(PropertyDependencySelector));
            InternalTypes.Add(typeof(CompositionRootTypeAttribute));
            InternalTypes.Add(typeof(ConcreteTypeExtractor));
            InternalTypes.Add(typeof(CompositionRootExecutor));
            InternalTypes.Add(typeof(CompositionRootTypeExtractor));
            InternalTypes.Add(typeof(CachedTypeExtractor));
            InternalTypes.Add(typeof(ImmutableList<>));
            InternalTypes.Add(typeof(KeyValue<,>));
            InternalTypes.Add(typeof(ImmutableHashTree<,>));
            InternalTypes.Add(typeof(ImmutableHashTable<,>));
            InternalTypes.Add(typeof(PerThreadScopeManagerProvider));
            InternalTypes.Add(typeof(Emitter));
            InternalTypes.Add(typeof(Instruction));
            InternalTypes.Add(typeof(Instruction<>));
            InternalTypes.Add(typeof(GetInstanceDelegate));
            InternalTypes.Add(typeof(ContainerOptions));
            InternalTypes.Add(typeof(CompositionRootAttributeExtractor));
#if NET452 || NET46 || NETSTANDARD1_3 || NETSTANDARD1_6 || NETSTANDARD2_0 || NETCOREAPP2_0
            InternalTypes.Add(typeof(PerLogicalCallContextScopeManagerProvider));
            InternalTypes.Add(typeof(PerLogicalCallContextScopeManager));
            InternalTypes.Add(typeof(LogicalThreadStorage<>));
#endif
#if NETSTANDARD1_1 || NETSTANDARD1_3 || NETSTANDARD1_6 || NETSTANDARD2_0
            InternalTypes.Add(typeof(DynamicMethod));
            InternalTypes.Add(typeof(ILGenerator));
            InternalTypes.Add(typeof(LocalBuilder));
#endif
        }

        /// <summary>
        /// Extracts concrete types found in the given <paramref name="assembly"/>.
        /// </summary>
        /// <param name="assembly">The <see cref="Assembly"/> for which to extract types.</param>
        /// <returns>A set of concrete types found in the given <paramref name="assembly"/>.</returns>
        public Type[] Execute(Assembly assembly)
        {
            return
                assembly.DefinedTypes.Where(info => IsConcreteType(info))
                    .Except(InternalTypes.Select(i => i.GetTypeInfo()))
                    .Cast<Type>()
                    .ToArray();
        }

        private static bool IsConcreteType(TypeInfo typeInfo)
        {
            return typeInfo.IsClass
                   && !typeInfo.IsNestedPrivate
                   && !typeInfo.IsAbstract
                   && !Equals(typeInfo.Assembly, typeof(string).GetTypeInfo().Assembly)
                   && !IsCompilerGenerated(typeInfo);
        }

        private static bool IsCompilerGenerated(TypeInfo typeInfo)
        {
            return typeInfo.IsDefined(typeof(CompilerGeneratedAttribute), false);
        }
    }
}