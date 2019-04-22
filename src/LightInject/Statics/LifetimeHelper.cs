namespace LightInject
{
     internal static class LifetimeHelper
    {
        static LifetimeHelper()
        {
            GetInstanceMethod = typeof(ILifetime).GetTypeInfo().GetDeclaredMethod("GetInstance");
            GetCurrentScopeMethod = typeof(IScopeManager).GetTypeInfo().GetDeclaredProperty("CurrentScope").GetMethod;
        }

        public static MethodInfo GetInstanceMethod { get; private set; }

        public static MethodInfo GetCurrentScopeMethod { get; private set; }
    }
}