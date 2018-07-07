// <copyright file="IPatcher.cs" company="dymanoid">Copyright (c) dymanoid. All rights reserved.</copyright>

namespace RealTime.Patching
{
    using System.Reflection;

    /// <summary>An interface for method patching processor.</summary>
    internal interface IPatcher
    {
        /// <summary>
        /// Applies a patch to the specified <paramref name="method"/>. At least one called method must be specified.
        /// </summary>
        /// <param name="method">The method to patch.</param>
        /// <param name="prefixCall">The prefix method to call before the <paramref name="method"/>. Can be null.</param>
        /// <param name="postfixCall">The postfix method to call after the <paramref name="method"/>. Can be null.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="method"/> is null.</exception>
        /// <exception cref="ArgumentException">
        /// Thrown when both <paramref name="prefixCall"/> and <paramref name="postfixCall"/> are null.
        /// </exception>
        void ApplyPatch(MethodInfo method, MethodInfo prefixCall, MethodInfo postfixCall);

        /// <summary>Reverts a patch from the specified <paramref name="method"/>.</summary>
        /// <param name="method">The method to patch.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="method"/> is null.</exception>
        void RevertPatch(MethodInfo method);
    }
}