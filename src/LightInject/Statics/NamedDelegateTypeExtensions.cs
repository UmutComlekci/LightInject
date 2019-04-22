namespace LightInject
{
    internal static class NamedDelegateTypeExtensions
    {
        private static readonly MethodInfo CreateInstanceDelegateMethodInfo =
            typeof(NamedDelegateTypeExtensions).GetTypeInfo().GetDeclaredMethod("CreateInstanceDelegate");

        private static readonly ThreadSafeDictionary<Type, MethodInfo> CreateInstanceDelegateMethods =
            new ThreadSafeDictionary<Type, MethodInfo>();

        public static Delegate CreateNamedGetInstanceDelegate(this Type serviceType, string serviceName, IServiceFactory factory)
        {
            MethodInfo createInstanceDelegateMethodInfo = CreateInstanceDelegateMethods.GetOrAdd(
                serviceType,
                CreateClosedGenericCreateInstanceDelegateMethod);

            return (Delegate)createInstanceDelegateMethodInfo.Invoke(null, new object[] { factory, serviceName });
        }

        private static MethodInfo CreateClosedGenericCreateInstanceDelegateMethod(Type type)
        {
            return CreateInstanceDelegateMethodInfo.MakeGenericMethod(type);
        }

        // ReSharper disable UnusedMember.Local
        private static Func<TService> CreateInstanceDelegate<TService>(IServiceFactory factory, string serviceName)

        // ReSharper restore UnusedMember.Local
        {
            return () => factory.GetInstance<TService>(serviceName);
        }
    }
}