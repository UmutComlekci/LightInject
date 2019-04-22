namespace LightInject
{
    /// <summary>
    /// Represents a class that builds a <see cref="ConstructionInfo"/> instance based on the implementing <see cref="Type"/>.
    /// </summary>
    public interface ITypeConstructionInfoBuilder
    {
        /// <summary>
        /// Analyzes the <paramref name="registration"/> and returns a <see cref="ConstructionInfo"/> instance.
        /// </summary>
        /// <param name="registration">The <see cref="Registration"/> that represents the implementing type to analyze.</param>
        /// <returns>A <see cref="ConstructionInfo"/> instance.</returns>
        ConstructionInfo Execute(Registration registration);
    }
}