namespace LightInject
{
   /// <summary>
    /// A class that is capable of providing a service name
    /// to be used when a service is registered during assembly scanning.
    /// </summary>
    public class ServiceNameProvider : IServiceNameProvider
    {
        /// <inheritdoc/>
        public string GetServiceName(Type serviceType, Type implementingType)
        {
            string implementingTypeName = implementingType.FullName;
            string serviceTypeName = serviceType.FullName;
            if (implementingType.GetTypeInfo().IsGenericTypeDefinition)
            {
                var regex = new Regex("((?:[a-z][a-z.]+))", RegexOptions.IgnoreCase);
                implementingTypeName = regex.Match(implementingTypeName).Groups[1].Value;
                serviceTypeName = regex.Match(serviceTypeName).Groups[1].Value;
            }

            if (serviceTypeName.Split('.').Last().Substring(1) == implementingTypeName.Split('.').Last())
            {
                implementingTypeName = string.Empty;
            }

            return implementingTypeName;
        }
    }
}