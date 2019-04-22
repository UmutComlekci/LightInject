using System;

namespace LightInject
{
   /// <summary>
    /// Contains a set of extension method that represents
    /// a compability layer for reflection methods.
    /// </summary>
    internal static class TypeHelper
    {
        /// <summary>
        /// Attempts to create a generic type from the <paramref name="openGenericType"/> using <paramref name="closedGenericArguments"/>.
        /// </summary>
        /// <param name="openGenericType">The open generic type for which to create a generic type.</param>
        /// <param name="closedGenericArguments">The generic arguments used to create the generic type.</param>
        /// <returns>The closed generic type if successful, otherwise null.</returns>
        public static Type TryMakeGenericType(Type openGenericType, Type[] closedGenericArguments)
        {
            try
            {
                return openGenericType.MakeGenericType(closedGenericArguments);
            }
            catch (Exception)
            {
                return null;
            }
        }

#if NET452 || NET46 || NETCOREAPP2_0

        /// <summary>
        /// Gets the method represented by the delegate.
        /// </summary>
        /// <param name="del">The target <see cref="Delegate"/>.</param>
        /// <returns>The method represented by the delegate.</returns>
        public static MethodInfo GetMethodInfo(this Delegate del)
        {
            return del.Method;
        }

        /// <summary>
        /// Gets the custom attributes for this <paramref name="assembly"/>.
        /// </summary>
        /// <param name="assembly">The target <see cref="Assembly"/>.</param>
        /// <param name="attributeType">The type of <see cref="Attribute"/> objects to return.</param>
        /// <returns>The custom attributes for this <paramref name="assembly"/>.</returns>
        public static IEnumerable<Attribute> GetCustomAttributes(this Assembly assembly, Type attributeType)
        {
            return assembly.GetCustomAttributes(attributeType, false).Cast<Attribute>();
        }
#endif

        /// <summary>
        /// Gets a value indicating whether the <see cref="Type"/> is an <see cref="IEnumerable{T}"/> type.
        /// </summary>
        /// <param name="type">The target <see cref="Type"/>.</param>
        /// <returns>true if the <see cref="Type"/> is an <see cref="IEnumerable{T}"/>; otherwise, false.</returns>
        public static bool IsEnumerableOfT(this Type type)
        {
            var typeInfo = type.GetTypeInfo();
            return typeInfo.IsGenericType && typeInfo.GetGenericTypeDefinition() == typeof(IEnumerable<>);
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="Type"/> is an <see cref="IList{T}"/> type.
        /// </summary>
        /// <param name="type">The target <see cref="Type"/>.</param>
        /// <returns>true if the <see cref="Type"/> is an <see cref="IList{T}"/>; otherwise, false.</returns>
        public static bool IsListOfT(this Type type)
        {
            var typeInfo = type.GetTypeInfo();
            return typeInfo.IsGenericType && typeInfo.GetGenericTypeDefinition() == typeof(IList<>);
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="Type"/> is an <see cref="ICollection{T}"/> type.
        /// </summary>
        /// <param name="type">The target <see cref="Type"/>.</param>
        /// <returns>true if the <see cref="Type"/> is an <see cref="ICollection{T}"/>; otherwise, false.</returns>
        public static bool IsCollectionOfT(this Type type)
        {
            var typeInfo = type.GetTypeInfo();
            return typeInfo.IsGenericType && typeInfo.GetGenericTypeDefinition() == typeof(ICollection<>);
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="Type"/> is an <see cref="IReadOnlyCollection{T}"/> type.
        /// </summary>
        /// <param name="type">The target <see cref="Type"/>.</param>
        /// <returns>true if the <see cref="Type"/> is an <see cref="IReadOnlyCollection{T}"/>; otherwise, false.</returns>
        public static bool IsReadOnlyCollectionOfT(this Type type)
        {
            var typeInfo = type.GetTypeInfo();
            return typeInfo.IsGenericType && typeInfo.GetGenericTypeDefinition() == typeof(IReadOnlyCollection<>);
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="Type"/> is an <see cref="IReadOnlyList{T}"/> type.
        /// </summary>
        /// <param name="type">The target <see cref="Type"/>.</param>
        /// <returns>true if the <see cref="Type"/> is an <see cref="IReadOnlyList{T}"/>; otherwise, false.</returns>
        public static bool IsReadOnlyListOfT(this Type type)
        {
            var typeInfo = type.GetTypeInfo();
            return typeInfo.IsGenericType && typeInfo.GetGenericTypeDefinition() == typeof(IReadOnlyList<>);
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="Type"/> is an <see cref="Lazy{T}"/> type.
        /// </summary>
        /// <param name="type">The target <see cref="Type"/>.</param>
        /// <returns>true if the <see cref="Type"/> is an <see cref="Lazy{T}"/>; otherwise, false.</returns>
        public static bool IsLazy(this Type type)
        {
            var typeInfo = type.GetTypeInfo();
            return typeInfo.IsGenericType && typeInfo.GetGenericTypeDefinition() == typeof(Lazy<>);
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="Type"/> is an <see cref="Func{T1}"/> type.
        /// </summary>
        /// <param name="type">The target <see cref="Type"/>.</param>
        /// <returns>true if the <see cref="Type"/> is an <see cref="Func{T1}"/>; otherwise, false.</returns>
        public static bool IsFunc(this Type type)
        {
            var typeInfo = type.GetTypeInfo();
            return typeInfo.IsGenericType && typeInfo.GetGenericTypeDefinition() == typeof(Func<>);
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="Type"/> is an <see cref="Func{T1, TResult}"/>,
        /// <see cref="Func{T1,T2,TResult}"/>, <see cref="Func{T1,T2,T3, TResult}"/> or an <see cref="Func{T1,T2,T3,T4 ,TResult}"/>.
        /// </summary>
        /// <param name="type">The target <see cref="Type"/>.</param>
        /// <returns>true if the <see cref="Type"/> is an <see cref="Func{T1, TResult}"/>, <see cref="Func{T1,T2,TResult}"/>, <see cref="Func{T1,T2,T3, TResult}"/> or an <see cref="Func{T1,T2,T3,T4 ,TResult}"/>; otherwise, false.</returns>
        public static bool IsFuncWithParameters(this Type type)
        {
            var typeInfo = type.GetTypeInfo();
            if (!typeInfo.IsGenericType)
            {
                return false;
            }

            Type genericTypeDefinition = typeInfo.GetGenericTypeDefinition();

            return genericTypeDefinition == typeof(Func<,>) || genericTypeDefinition == typeof(Func<,,>)
                   || genericTypeDefinition == typeof(Func<,,,>) || genericTypeDefinition == typeof(Func<,,,,>);
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="Type"/> is a closed generic type.
        /// </summary>
        /// <param name="type">The target <see cref="Type"/>.</param>
        /// <returns>true if the <see cref="Type"/> is a closed generic type; otherwise, false.</returns>
        public static bool IsClosedGeneric(this Type type)
        {
            var typeInfo = type.GetTypeInfo();
            return typeInfo.IsGenericType && !typeInfo.IsGenericTypeDefinition;
        }

        /// <summary>
        /// Returns the <see cref="Type"/> of the object encompassed or referred to by the current array, pointer or reference type.
        /// </summary>
        /// <param name="type">The target <see cref="Type"/>.</param>
        /// <returns>The <see cref="Type"/> of the object encompassed or referred to by the current array, pointer, or reference type,
        /// or null if the current Type is not an array or a pointer, or is not passed by reference,
        /// or represents a generic type or a type parameter in the definition of a generic type or generic method.</returns>
        public static Type GetElementType(Type type)
        {
            var typeInfo = type.GetTypeInfo();
            var genericTypeArguments = typeInfo.GenericTypeArguments;
            if (typeInfo.IsGenericType && genericTypeArguments.Length == 1)
            {
                return genericTypeArguments[0];
            }

            return type.GetElementType();
        }
    }
}