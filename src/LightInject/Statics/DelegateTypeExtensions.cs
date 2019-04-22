using System;
using System.Reflection;

namespace LightInject
{
   internal static class DelegateTypeExtensions
    {
        private static readonly MethodInfo OpenGenericGetInstanceMethodInfo =
            typeof(ServiceFactoryExtensions).GetTypeInfo().DeclaredMethods.Where(m => m.Name == "GetInstance" & m.GetParameters().Length == 1).Single();

        private static readonly ThreadSafeDictionary<Type, MethodInfo> GetInstanceMethods =
            new ThreadSafeDictionary<Type, MethodInfo>();

        public static Delegate CreateGetInstanceDelegate(this Type serviceType, IServiceFactory serviceFactory)
        {
            Type delegateType = serviceType.GetFuncType();
            MethodInfo getInstanceMethod = GetInstanceMethods.GetOrAdd(serviceType, CreateGetInstanceMethod);
            return getInstanceMethod.CreateDelegate(delegateType, serviceFactory);
        }

        private static MethodInfo CreateGetInstanceMethod(Type type)
        {
            return OpenGenericGetInstanceMethodInfo.MakeGenericMethod(type);
        }
    }
}