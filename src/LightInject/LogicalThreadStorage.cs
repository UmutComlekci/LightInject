namespace LightInject
{
    using System.Threading;

#if NET452
    using System;
    using System.Runtime.Remoting.Messaging;
#endif

#if NETSTANDARD1_3 || NETSTANDARD1_6 || NETSTANDARD2_0 || NET46 || NETCOREAPP2_0
    /// <summary>
    /// Provides storage per logical thread of execution.
    /// </summary>
    /// <typeparam name="T">The type of the value contained in this <see cref="LogicalThreadStorage{T}"/>.</typeparam>
    public class LogicalThreadStorage<T>
    {
        private readonly AsyncLocal<T> asyncLocal = new AsyncLocal<T>();

        private readonly object lockObject = new object();

        /// <summary>
        /// Gets or sets the value for the current logical thread of execution.
        /// </summary>
        /// <value>
        /// The value for the current logical thread of execution.
        /// </value>
        public T Value
        {
            get { return asyncLocal.Value; }
            set { asyncLocal.Value = value; }
        }
    }
#endif

#if NET452

    /// <summary>
    /// Provides storage per logical thread of execution.
    /// </summary>
    /// <typeparam name="T">The type of the value contained in this <see cref="LogicalThreadStorage{T}"/>.</typeparam>
    public class LogicalThreadStorage<T>
    {
        private readonly string key = Guid.NewGuid().ToString();

        /// <summary>
        /// Gets or sets the value for the current logical thread of execution.
        /// </summary>
        /// <value>
        /// The value for the current logical thread of execution.
        /// </value>
        public T Value
        {
            get
            {
                var logicalThreadValue = (LogicalThreadValue)CallContext.LogicalGetData(key);
                return logicalThreadValue != null ? logicalThreadValue.Value : default(T);
            }

            set
            {
                LogicalThreadValue logicalThreadValue = null;
                if (value != null)
                {
                    logicalThreadValue = new LogicalThreadValue { Value = value };
                }

                CallContext.LogicalSetData(key, logicalThreadValue);
            }
        }

        [Serializable]
        private class LogicalThreadValue : MarshalByRefObject
        {
            [NonSerialized]
            private T value;

            public T Value
            {
                get
                {
                    return value;
                }

                set
                {
                    this.value = value;
                }
            }
        }
    }
#endif
}