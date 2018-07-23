// <copyright file="FastDelegate.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.Patching
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
    internal static class FastDelegate
    {
        /// <summary>
        /// Creates a delegate instance of the specified <typeparamref name="TDelegate"/> type that represents a method
        /// of the <typeparamref name="TType"/> class. If the target method is a <typeparamref name="TType"/>'s instance
        /// method, the first parameter of the <typeparamref name="TDelegate"/> signature must be a reference to a
        /// <typeparamref name="TType"/> instance.
        /// </summary>
        ///
        /// <exception cref="ArgumentException">Thrown when <paramref name="name"/> is null or an empty string.</exception>
        /// <exception cref="AmbiguousMatchException">Thrown when more than one method is found with the specified
        /// <paramref name="name"/> and matching the specified <typeparamref name="TDelegate"/> signature.</exception>
        /// <exception cref="MissingMethodException">Thrown when no method with the specified <paramref name="name"/> is found
        /// that matches the specified <typeparamref name="TDelegate"/> signature.</exception>
        ///
        /// <typeparam name="TType">A class that holds the method to create a delegate for.</typeparam>
        /// <typeparam name="TDelegate">A delegate type representing the method signature.</typeparam>
        ///
        /// <param name="name">The method name to create a delegate instance for.</param>
        /// <param name="instanceMethod">True if the <typeparamref name="TDelegate"/> type represents an instance method.
        /// That means, the first parameter of the <typeparamref name="TDelegate"/> signature will be processed
        /// as a reference to a <typeparamref name="TType"/> instance (<c>this</c> reference). Default is <c>true</c>.</param>
        ///
        /// <returns>An instance of the <typeparamref name="TDelegate"/> delegate that can be called directly.</returns>
        public static TDelegate Create<TType, TDelegate>(string name, bool instanceMethod = true)
            where TDelegate : Delegate
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("The method name cannot be null or empty string.");
            }

            MethodInfo methodInfo = GetMethodInfo<TType, TDelegate>(name, instanceMethod);
            return (TDelegate)Delegate.CreateDelegate(typeof(TDelegate), methodInfo);
        }

        private static MethodInfo GetMethodInfo<TType, TDelegate>(string name, bool instanceMethod)
        {
            IEnumerable<ParameterInfo> parameters = typeof(TDelegate).GetMethod("Invoke").GetParameters();
            if (instanceMethod)
            {
                parameters = parameters.Skip(1);
            }

            MethodInfo methodInfo = typeof(TType).GetMethod(
                name,
                BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                parameters.Select(p => p.ParameterType).ToArray(),
                new ParameterModifier[0]);

            if (methodInfo == null)
            {
                throw new MissingMethodException($"The method '{typeof(TType).Name}.{name}' matching the specified signature doesn't exist: {typeof(TDelegate)}");
            }

            return methodInfo;
        }
    }
}
