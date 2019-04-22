namespace LightInject
{
   /// <summary>
    /// Represents a class that keeps track of a <see cref="ConstructionInfo"/> instance for each <see cref="Registration"/>.
    /// </summary>
    public interface IConstructionInfoProvider
    {
        /// <summary>
        /// Gets a <see cref="ConstructionInfo"/> instance for the given <paramref name="registration"/>.
        /// </summary>
        /// <param name="registration">The <see cref="Registration"/> for which to get a <see cref="ConstructionInfo"/> instance.</param>
        /// <returns>The <see cref="ConstructionInfo"/> instance that describes how to create an instance of the given <paramref name="registration"/>.</returns>
        ConstructionInfo GetConstructionInfo(Registration registration);
    }
}