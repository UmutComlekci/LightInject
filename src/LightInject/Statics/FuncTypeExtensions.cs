namespace LightInject
{
    using System;

    internal static class FuncTypeExtensions
    {
        private static readonly ThreadSafeDictionary<Type, Type> FuncTypes = new ThreadSafeDictionary<Type, Type>();

        public static Type GetFuncType(this Type returnType)
        {
            return FuncTypes.GetOrAdd(returnType, CreateFuncType);
        }

        private static Type CreateFuncType(Type type)
        {
            return typeof(Func<>).MakeGenericType(type);
        }
    }
}