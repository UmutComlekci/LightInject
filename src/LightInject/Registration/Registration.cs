namespace LightInject
{
    using System;
    using System.Linq.Expressions;

    /// <summary>
    /// Base class for concrete registrations within the service container.
    /// </summary>
    public abstract class Registration
    {
        /// <summary>
        /// Gets or sets the service <see cref="Type"/>.
        /// </summary>
        public Type ServiceType { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Type"/> that implements the <see cref="Registration.ServiceType"/>.
        /// </summary>
        public virtual Type ImplementingType { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="LambdaExpression"/> used to create a service instance.
        /// </summary>
        public Delegate FactoryExpression { get; set; }
    }
}