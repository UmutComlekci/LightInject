namespace LightInject
{
    #if NET452 || NET46 || NETSTANDARD1_6 || NETSTANDARD2_0 || NETCOREAPP2_0

    /// <summary>
    /// Represents a class that is responsible loading a set of assemblies based on the given search pattern.
    /// </summary>
    public interface IAssemblyLoader
    {
        /// <summary>
        /// Loads a set of assemblies based on the given <paramref name="searchPattern"/>.
        /// </summary>
        /// <param name="searchPattern">The search pattern to use.</param>
        /// <returns>A list of assemblies based on the given <paramref name="searchPattern"/>.</returns>
        IEnumerable<Assembly> Load(string searchPattern);
    }
#endif
}