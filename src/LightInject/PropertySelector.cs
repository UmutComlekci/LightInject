namespace LightInject
{
   /// <summary>
    /// Selects the properties that represents a dependency to the target <see cref="Type"/>.
    /// </summary>
    public class PropertySelector : IPropertySelector
    {
        /// <summary>
        /// Selects properties that represents a dependency from the given <paramref name="type"/>.
        /// </summary>
        /// <param name="type">The <see cref="Type"/> for which to select the properties.</param>
        /// <returns>A list of properties that represents a dependency to the target <paramref name="type"/>.</returns>
        public IEnumerable<PropertyInfo> Execute(Type type)
        {
            return type.GetRuntimeProperties().Where(IsInjectable).ToList();
        }

        /// <summary>
        /// Determines if the <paramref name="propertyInfo"/> represents an injectable property.
        /// </summary>
        /// <param name="propertyInfo">The <see cref="PropertyInfo"/> that describes the target property.</param>
        /// <returns><b>true</b> if the property is injectable, otherwise <b>false</b>.</returns>
        protected virtual bool IsInjectable(PropertyInfo propertyInfo)
        {
            return !IsReadOnly(propertyInfo);
        }

        private static bool IsReadOnly(PropertyInfo propertyInfo)
        {
            return propertyInfo.SetMethod == null || propertyInfo.SetMethod.IsStatic || propertyInfo.SetMethod.IsPrivate || propertyInfo.GetIndexParameters().Length > 0;
        }
    }
}