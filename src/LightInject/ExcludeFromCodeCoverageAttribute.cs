namespace LightInject
{
#if NETSTANDARD1_1 || NETSTANDARD1_3 || NETSTANDARD1_6 || NETSTANDARD2_0
    /// <summary>
    /// An attribute shim since we don't have this attribute in netstandard.
    /// </summary>
    public class ExcludeFromCodeCoverageAttribute : Attribute
    {
    }
#endif
}