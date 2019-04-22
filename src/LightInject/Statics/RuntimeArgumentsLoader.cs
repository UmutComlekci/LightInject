namespace LightInject
{
    /// <summary>
    /// This class is not for public use and is used internally
    /// to load runtime arguments onto the evaluation stack.
    /// </summary>
    public static class RuntimeArgumentsLoader
    {
        /// <summary>
        /// Loads the runtime arguments onto the evaluation stack.
        /// </summary>
        /// <param name="constants">A object array representing the dynamic method context.</param>
        /// <returns>An array containing the runtime arguments supplied when resolving the service.</returns>
        public static object[] Load(object[] constants)
        {
            if (!(constants[constants.Length - 1] is object[] arguments))
            {
                return new object[] { };
            }

            return arguments;
        }
    }
}