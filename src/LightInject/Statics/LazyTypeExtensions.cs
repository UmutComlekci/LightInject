namespace LightInject
{
    using System;
    using System.Linq;
    using System.Reflection;

    internal static class LazyTypeExtensions
    {
        private static readonly ThreadSafeDictionary<Type, ConstructorInfo> Constructors = new ThreadSafeDictionary<Type, ConstructorInfo>();

        public static ConstructorInfo GetLazyConstructor(this Type type)
        {
            return Constructors.GetOrAdd(type, GetConstructor);
        }

        private static ConstructorInfo GetConstructor(Type type)
        {
            Type closedGenericLazyType = typeof(Lazy<>).MakeGenericType(type);
            return closedGenericLazyType.GetTypeInfo().DeclaredConstructors.Where(c => c.GetParameters().Length == 1 && c.GetParameters()[0].ParameterType == type.GetFuncType()).Single();
        }
    }
}