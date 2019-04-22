namespace LightInject
{
   /// <summary>
    /// Contains information about a registered service.
    /// </summary>
    public class ServiceRegistration : Registration
    {
        /// <summary>
        /// Gets or sets the name of the service.
        /// </summary>
        public string ServiceName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the <see cref="ILifetime"/> instance that controls the lifetime of the service.
        /// </summary>
        public ILifetime Lifetime { get; set; }

        /// <summary>
        /// Gets or sets the value that represents the instance of the service.
        /// </summary>
        public object Value { get; set; }

        /// <summary>
        /// Serves as a hash function for a particular type.
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="T:System.Object"/>.
        /// </returns>
        public override int GetHashCode()
        {
            return ServiceType.GetHashCode() ^ ServiceName.GetHashCode();
        }

        /// <summary>
        /// Determines whether the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// True if the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>; otherwise, false.
        /// </returns>
        /// <param name="obj">The <see cref="T:System.Object"/> to compare with the current <see cref="T:System.Object"/>.</param>
        public override bool Equals(object obj)
        {
            if (!(obj is ServiceRegistration other))
            {
                return false;
            }

            var result = ServiceName == other.ServiceName && ServiceType == other.ServiceType;
            return result;
        }

        /// <summary>
        /// Returns a string representation of the <see cref="ServiceRegistration"/>.
        /// </summary>
        /// <returns>A string representation of the <see cref="ServiceRegistration"/>.</returns>
        public override string ToString()
        {
            var lifeTime = Lifetime?.ToString() ?? "Transient";
            return $"ServiceType: '{ServiceType}', ServiceName: '{ServiceName}', ImplementingType: '{ImplementingType}', Lifetime: '{lifeTime}'";
        }
    }
}