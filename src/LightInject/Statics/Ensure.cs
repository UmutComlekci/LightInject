namespace LightInject
{
   /// <summary>
    /// Contains a set of helper method related to validating
    /// user input.
    /// </summary>
    public static class Ensure
    {
        /// <summary>
        /// Ensures that the given <paramref name="value"/> is not null.
        /// </summary>
        /// <typeparam name="T">The type of value to be validated.</typeparam>
        /// <param name="value">The value to be validated.</param>
        /// <param name="paramName">The name of the parameter from which the <paramref name="value"/> comes from.</param>
        public static void IsNotNull<T>(T value, string paramName)
        {
            if (value == null)
            {
                throw new ArgumentNullException(paramName);
            }
        }
    }
}