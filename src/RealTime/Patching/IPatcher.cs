// <copyright file="IPatcher.cs" company="dymanoid">Copyright (c) dymanoid. All rights reserved.</copyright>

namespace SkyTools.Patching
{
    using System.Reflection;

    /// <summary>An interface for method patching processor.</summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Patcher", Justification = "Reviewed")]
    public interface IPatcher
    {
        /// <summary>
        /// Applies a patch to the specified <paramref name="method"/>. At least one called method must be specified.
        /// </summary>
        /// <param name="method">The method to patch.</param>
        /// <param name="prefixCall">The prefix method to call before the <paramref name="method"/>. Can be null.</param>
        /// <param name="postfixCall">The postfix method to call after the <paramref name="method"/>. Can be null.</param>
        /// <param name="transformCall">The transform method called to process the IL code of the <paramref name="method"/>. Can be null.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="method"/> is null.</exception>
        /// <exception cref="System.ArgumentException">
        /// Thrown when all three called methods are null (<paramref name="prefixCall"/>, <paramref name="postfixCall"/>,
        /// <paramref name="transformCall"/>).
        /// </exception>
        void ApplyPatch(MethodInfo method, MethodInfo prefixCall, MethodInfo postfixCall, MethodInfo transformCall);

        /// <summary>Reverts a patch from the specified <paramref name="method"/>.</summary>
        /// <param name="method">The method to patch.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="method"/> is null.</exception>
        void RevertPatch(MethodInfo method);
    }
}
