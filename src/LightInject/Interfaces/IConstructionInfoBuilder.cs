namespace LightInject
{
    /// <summary>
    /// Represents a class that is capable of building a <see cref="ConstructorInfo"/> instance
    /// based on a <see cref="Registration"/>.
    /// </summary>
    public interface IConstructionInfoBuilder
    {
        /// <summary>
        /// Returns a <see cref="ConstructionInfo"/> instance based on the given <see cref="Registration"/>.
        /// </summary>
        /// <param name="registration">The <see cref="Registration"/> for which to return a <see cref="ConstructionInfo"/> instance.</param>
        /// <returns>A <see cref="ConstructionInfo"/> instance that describes how to create a service instance.</returns>
        ConstructionInfo Execute(Registration registration);
    }
}