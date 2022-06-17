// <copyright file="FastDelegateFactory.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace SkyTools.Patching
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// A static class containing helper methods for creating delegate instances. These can be used
    /// for speeding up the method calls based on Reflection. The initial delegate creation will need
    /// some overhead, but the delegate calls will be then nearly as fast as direct method calls.
    /// </summary>
    public static class FastDelegateFactory
    {
        /// <summary>
        /// Creates a delegate instance of the specified <typeparamref name="TDelegate"/> type that represents a method
        /// of the <paramref name="type"/>. If the target method is a <paramref name="type"/>'s instance
        /// method, the first parameter of the <typeparamref name="TDelegate"/> signature must be a reference to a
        /// <paramref name="type"/> instance.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="type"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="name"/> is null or an empty string.</exception>
        /// <exception cref="AmbiguousMatchException">Thrown when more than one method is found with the specified
        /// <exception cref="MethodAccessException">Thrown when the caller does not have the permissions necessary to access the method. </exception>
        /// <paramref name="name"/> and matching the specified <typeparamref name="TDelegate"/> signature.</exception>
        /// <exception cref="MissingMethodException">Thrown when no method with the specified <paramref name="name"/> is found
        /// that matches the specified <typeparamref name="TDelegate"/> signature.</exception>
        /// <typeparam name="TDelegate">A delegate type representing the method signature.</typeparam>
        /// <param name="type">A class that holds the method to create a delegate for.</param>
        /// <param name="name">The method name to create a delegate instance for.</param>
        /// <param name="instanceMethod">True if the <typeparamref name="TDelegate"/> type represents an instance method.
        /// That means, the first parameter of the <typeparamref name="TDelegate"/> signature will be processed
        /// as a reference to a <paramref name="type"/> instance (<c>this</c> reference). Default is <c>true</c>.</param>
        /// <returns>An instance of the <typeparamref name="TDelegate"/> delegate that can be called directly.</returns>
        public static TDelegate Create<TDelegate>(Type type, string name, bool instanceMethod)
            where TDelegate : Delegate
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("The method name cannot be null or empty string.");
            }

            var methodInfo = GetMethodInfo<TDelegate>(type, name, instanceMethod);
            return (TDelegate)Delegate.CreateDelegate(typeof(TDelegate), methodInfo);
        }

        private static MethodInfo GetMethodInfo<TDelegate>(Type type, string name, bool instanceMethod)
        {
            IEnumerable<ParameterInfo> parameters = typeof(TDelegate).GetMethod("Invoke").GetParameters();
            if (instanceMethod)
            {
                parameters = parameters.Skip(1);
            }

            var methodInfo = type.GetMethod(
                name,
                BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                parameters.Select(p => p.ParameterType).ToArray(),
                new ParameterModifier[0]);

            if (methodInfo == null)
            {
                throw new MissingMethodException($"The method '{type.Name}.{name}' matching the specified signature doesn't exist: {typeof(TDelegate)}");
            }

            return methodInfo;
        }
    }
}
