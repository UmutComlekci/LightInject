namespace LightInject
{
    /// <summary>
    /// A delegate that represents the dynamic method compiled to resolved service instances.
    /// </summary>
    /// <param name="args">The arguments used by the dynamic method that this delegate represents.</param>
    /// <returns>A service instance.</returns>
    internal delegate object GetInstanceDelegate(object[] args);
}