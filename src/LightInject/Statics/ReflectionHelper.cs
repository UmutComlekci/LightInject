namespace LightInject
{
    internal static class ReflectionHelper
    {
        private static readonly Lazy<ThreadSafeDictionary<Type, MethodInfo>> GetInstanceWithParametersMethods;

        static ReflectionHelper()
        {
            GetInstanceWithParametersMethods = CreateLazyGetInstanceWithParametersMethods();
        }

        public static MethodInfo GetGetInstanceWithParametersMethod(Type serviceType)
        {
            return GetInstanceWithParametersMethods.Value.GetOrAdd(serviceType, CreateGetInstanceWithParametersMethod);
        }

        public static Delegate CreateGetNamedInstanceWithParametersDelegate(IServiceFactory factory, Type delegateType, string serviceName)
        {
            Type[] genericTypeArguments = delegateType.GetTypeInfo().GenericTypeArguments;
            var openGenericMethod =
                typeof(ReflectionHelper).GetTypeInfo().DeclaredMethods
                    .Single(
                        m =>
                            m.GetGenericArguments().Length == genericTypeArguments.Length
                            && m.Name == "CreateGenericGetNamedParameterizedInstanceDelegate");
            var closedGenericMethod = openGenericMethod.MakeGenericMethod(genericTypeArguments);
            return (Delegate)closedGenericMethod.Invoke(null, new object[] { factory, serviceName });
        }

        private static Lazy<ThreadSafeDictionary<Type, MethodInfo>> CreateLazyGetInstanceWithParametersMethods()
        {
            return new Lazy<ThreadSafeDictionary<Type, MethodInfo>>(
                () => new ThreadSafeDictionary<Type, MethodInfo>());
        }

        private static MethodInfo CreateGetInstanceWithParametersMethod(Type serviceType)
        {
            Type[] genericTypeArguments = serviceType.GetTypeInfo().GenericTypeArguments;
            MethodInfo openGenericMethod =
                typeof(ServiceFactoryExtensions).GetTypeInfo().DeclaredMethods.Single(m => m.Name == "GetInstance"
                                                                                           && m.GetGenericArguments().Length == genericTypeArguments.Length && m.GetParameters().All(p => p.Name != "serviceName"));

            MethodInfo closedGenericMethod = openGenericMethod.MakeGenericMethod(genericTypeArguments);

            return closedGenericMethod;
        }

        // ReSharper disable UnusedMember.Local
        private static Func<TArg, TService> CreateGenericGetNamedParameterizedInstanceDelegate<TArg, TService>(IServiceFactory factory, string serviceName)

        // ReSharper restore UnusedMember.Local
        {
            return arg => factory.GetInstance<TArg, TService>(arg, serviceName);
        }

        // ReSharper disable UnusedMember.Local
        private static Func<TArg1, TArg2, TService> CreateGenericGetNamedParameterizedInstanceDelegate<TArg1, TArg2, TService>(IServiceFactory factory, string serviceName)

        // ReSharper restore UnusedMember.Local
        {
            return (arg1, arg2) => factory.GetInstance<TArg1, TArg2, TService>(arg1, arg2, serviceName);
        }

        // ReSharper disable UnusedMember.Local
        private static Func<TArg1, TArg2, TArg3, TService> CreateGenericGetNamedParameterizedInstanceDelegate<TArg1, TArg2, TArg3, TService>(IServiceFactory factory, string serviceName)

        // ReSharper restore UnusedMember.Local
        {
            return (arg1, arg2, arg3) => factory.GetInstance<TArg1, TArg2, TArg3, TService>(arg1, arg2, arg3, serviceName);
        }

        // ReSharper disable UnusedMember.Local
        private static Func<TArg1, TArg2, TArg3, TArg4, TService> CreateGenericGetNamedParameterizedInstanceDelegate<TArg1, TArg2, TArg3, TArg4, TService>(IServiceFactory factory, string serviceName)

        // ReSharper restore UnusedMember.Local
        {
            return (arg1, arg2, arg3, arg4) => factory.GetInstance<TArg1, TArg2, TArg3, TArg4, TService>(arg1, arg2, arg3, arg4, serviceName);
        }
    }
}