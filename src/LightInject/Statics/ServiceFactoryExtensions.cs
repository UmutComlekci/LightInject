namespace LightInject
{
    using System;
    using System.Collections.Generic;

   /// <summary>
    /// Extends the <see cref="IServiceFactory"/> interface.
    /// </summary>
    public static class ServiceFactoryExtensions
    {
        /// <summary>
        /// Gets an instance of the given <typeparamref name="TService"/> type.
        /// </summary>
        /// <typeparam name="TService">The type of the requested service.</typeparam>
        /// <param name="factory">The target <see cref="IServiceFactory"/>.</param>
        /// <returns>The requested service instance.</returns>
        public static TService GetInstance<TService>(this IServiceFactory factory)
        {
            return (TService)factory.GetInstance(typeof(TService));
        }

        /// <summary>
        /// Gets a named instance of the given <typeparamref name="TService"/>.
        /// </summary>
        /// <typeparam name="TService">The type of the requested service.</typeparam>
        /// <param name="factory">The target <see cref="IServiceFactory"/>.</param>
        /// <param name="serviceName">The name of the requested service.</param>
        /// <returns>The requested service instance.</returns>
        public static TService GetInstance<TService>(this IServiceFactory factory, string serviceName)
        {
            return (TService)factory.GetInstance(typeof(TService), serviceName);
        }

        /// <summary>
        /// Gets an instance of the given <typeparamref name="TService"/>.
        /// </summary>
        /// <typeparam name="T">The type of the argument.</typeparam>
        /// <typeparam name="TService">The type of the requested service.</typeparam>
        /// <param name="factory">The target <see cref="IServiceFactory"/>.</param>
        /// <param name="value">The argument value.</param>
        /// <returns>The requested service instance.</returns>
        public static TService GetInstance<T, TService>(this IServiceFactory factory, T value)
        {
            return (TService)factory.GetInstance(typeof(TService), new object[] { value });
        }

        /// <summary>
        /// Gets an instance of the given <typeparamref name="TService"/>.
        /// </summary>
        /// <typeparam name="T">The type of the parameter.</typeparam>
        /// <typeparam name="TService">The type of the requested service.</typeparam>
        /// <param name="factory">The target <see cref="IServiceFactory"/>.</param>
        /// <param name="value">The argument value.</param>
        /// <param name="serviceName">The name of the requested service.</param>
        /// <returns>The requested service instance.</returns>
        public static TService GetInstance<T, TService>(this IServiceFactory factory, T value, string serviceName)
        {
            return (TService)factory.GetInstance(typeof(TService), serviceName, new object[] { value });
        }

        /// <summary>
        /// Gets an instance of the given <typeparamref name="TService"/>.
        /// </summary>
        /// <typeparam name="T1">The type of the first parameter.</typeparam>
        /// <typeparam name="T2">The type of the second parameter.</typeparam>
        /// <typeparam name="TService">The type of the requested service.</typeparam>
        /// <param name="factory">The target <see cref="IServiceFactory"/>.</param>
        /// <param name="arg1">The first argument value.</param>
        /// <param name="arg2">The second argument value.</param>
        /// <returns>The requested service instance.</returns>
        public static TService GetInstance<T1, T2, TService>(this IServiceFactory factory, T1 arg1, T2 arg2)
        {
            return (TService)factory.GetInstance(typeof(TService), new object[] { arg1, arg2 });
        }

        /// <summary>
        /// Gets an instance of the given <typeparamref name="TService"/>.
        /// </summary>
        /// <typeparam name="T1">The type of the first parameter.</typeparam>
        /// <typeparam name="T2">The type of the second parameter.</typeparam>
        /// <typeparam name="TService">The type of the requested service.</typeparam>
        /// <param name="factory">The target <see cref="IServiceFactory"/>.</param>
        /// <param name="arg1">The first argument value.</param>
        /// <param name="arg2">The second argument value.</param>
        /// <param name="serviceName">The name of the requested service.</param>
        /// <returns>The requested service instance.</returns>
        public static TService GetInstance<T1, T2, TService>(this IServiceFactory factory, T1 arg1, T2 arg2, string serviceName)
        {
            return (TService)factory.GetInstance(typeof(TService), serviceName, new object[] { arg1, arg2 });
        }

        /// <summary>
        /// Gets an instance of the given <typeparamref name="TService"/>.
        /// </summary>
        /// <typeparam name="T1">The type of the first parameter.</typeparam>
        /// <typeparam name="T2">The type of the second parameter.</typeparam>
        /// <typeparam name="T3">The type of the third parameter.</typeparam>
        /// <typeparam name="TService">The type of the requested service.</typeparam>
        /// <param name="factory">The target <see cref="IServiceFactory"/>.</param>
        /// <param name="arg1">The first argument value.</param>
        /// <param name="arg2">The second argument value.</param>
        /// <param name="arg3">The third argument value.</param>
        /// <returns>The requested service instance.</returns>
        public static TService GetInstance<T1, T2, T3, TService>(this IServiceFactory factory, T1 arg1, T2 arg2, T3 arg3)
        {
            return (TService)factory.GetInstance(typeof(TService), new object[] { arg1, arg2, arg3 });
        }

        /// <summary>
        /// Gets an instance of the given <typeparamref name="TService"/>.
        /// </summary>
        /// <typeparam name="T1">The type of the first parameter.</typeparam>
        /// <typeparam name="T2">The type of the second parameter.</typeparam>
        /// <typeparam name="T3">The type of the third parameter.</typeparam>
        /// <typeparam name="TService">The type of the requested service.</typeparam>
        /// <param name="factory">The target <see cref="IServiceFactory"/>.</param>
        /// <param name="arg1">The first argument value.</param>
        /// <param name="arg2">The second argument value.</param>
        /// <param name="arg3">The third argument value.</param>
        /// <param name="serviceName">The name of the requested service.</param>
        /// <returns>The requested service instance.</returns>
        public static TService GetInstance<T1, T2, T3, TService>(this IServiceFactory factory, T1 arg1, T2 arg2, T3 arg3, string serviceName)
        {
            return (TService)factory.GetInstance(typeof(TService), serviceName, new object[] { arg1, arg2, arg3 });
        }

        /// <summary>
        /// Gets an instance of the given <typeparamref name="TService"/>.
        /// </summary>
        /// <typeparam name="T1">The type of the first parameter.</typeparam>
        /// <typeparam name="T2">The type of the second parameter.</typeparam>
        /// <typeparam name="T3">The type of the third parameter.</typeparam>
        /// <typeparam name="T4">The type of the fourth parameter.</typeparam>
        /// <typeparam name="TService">The type of the requested service.</typeparam>
        /// <param name="factory">The target <see cref="IServiceFactory"/>.</param>
        /// <param name="arg1">The first argument value.</param>
        /// <param name="arg2">The second argument value.</param>
        /// <param name="arg3">The third argument value.</param>
        /// <param name="arg4">The fourth argument value.</param>
        /// <returns>The requested service instance.</returns>
        public static TService GetInstance<T1, T2, T3, T4, TService>(this IServiceFactory factory, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            return (TService)factory.GetInstance(typeof(TService), new object[] { arg1, arg2, arg3, arg4 });
        }

        /// <summary>
        /// Gets an instance of the given <typeparamref name="TService"/>.
        /// </summary>
        /// <typeparam name="T1">The type of the first parameter.</typeparam>
        /// <typeparam name="T2">The type of the second parameter.</typeparam>
        /// <typeparam name="T3">The type of the third parameter.</typeparam>
        /// <typeparam name="T4">The type of the fourth parameter.</typeparam>
        /// <typeparam name="TService">The type of the requested service.</typeparam>
        /// <param name="factory">The target <see cref="IServiceFactory"/>.</param>
        /// <param name="arg1">The first argument value.</param>
        /// <param name="arg2">The second argument value.</param>
        /// <param name="arg3">The third argument value.</param>
        /// <param name="arg4">The fourth argument value.</param>
        /// <param name="serviceName">The name of the requested service.</param>
        /// <returns>The requested service instance.</returns>
        public static TService GetInstance<T1, T2, T3, T4, TService>(this IServiceFactory factory, T1 arg1, T2 arg2, T3 arg3, T4 arg4, string serviceName)
        {
            return (TService)factory.GetInstance(typeof(TService), serviceName, new object[] { arg1, arg2, arg3, arg4 });
        }

        /// <summary>
        /// Tries to get an instance of the given <typeparamref name="TService"/> type.
        /// </summary>
        /// <typeparam name="TService">The type of the requested service.</typeparam>
        /// <param name="factory">The target <see cref="IServiceFactory"/>.</param>
        /// <returns>The requested service instance if available, otherwise default(T).</returns>
        public static TService TryGetInstance<TService>(this IServiceFactory factory)
        {
            return (TService)factory.TryGetInstance(typeof(TService));
        }

        /// <summary>
        /// Tries to get an instance of the given <typeparamref name="TService"/> type.
        /// </summary>
        /// <typeparam name="TService">The type of the requested service.</typeparam>
        /// <param name="factory">The target <see cref="IServiceFactory"/>.</param>
        /// <param name="serviceName">The name of the requested service.</param>
        /// <returns>The requested service instance if available, otherwise default(T).</returns>
        public static TService TryGetInstance<TService>(this IServiceFactory factory, string serviceName)
        {
            return (TService)factory.TryGetInstance(typeof(TService), serviceName);
        }

        /// <summary>
        /// Gets all instances of type <typeparamref name="TService"/>.
        /// </summary>
        /// <typeparam name="TService">The type of services to resolve.</typeparam>
        /// <param name="factory">The target <see cref="IServiceFactory"/>.</param>
        /// <returns>A list that contains all implementations of the <typeparamref name="TService"/> type.</returns>
        public static IEnumerable<TService> GetAllInstances<TService>(this IServiceFactory factory)
        {
            return factory.GetInstance<IEnumerable<TService>>();
        }

        /// <summary>
        /// Creates an instance of a concrete class.
        /// </summary>
        /// <typeparam name="TService">The type of class for which to create an instance.</typeparam>
        /// <param name="factory">The target <see cref="IServiceFactory"/>.</param>
        /// <returns>An instance of <typeparamref name="TService"/>.</returns>
        /// <remarks>The concrete type will be registered if not already registered with the container.</remarks>
        public static TService Create<TService>(this IServiceFactory factory)
            where TService : class
        {
            return (TService)factory.Create(typeof(TService));
        }
    }
}