// <copyright file="IPatch.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace SkyTools.Patching
{
    /// <summary>
    /// An interface for classes that apply and revert method patches using an external <see cref="IPatcher"/>.
    /// </summary>
    public interface IPatch
    {
        /// <summary>Applies the method patch using the specified <paramref name="patcher"/>.</summary>
        /// <param name="patcher">The patcher object that can perform the patching.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when the argument is null.</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "patcher", Justification = "Reviewed")]
        void ApplyPatch(IPatcher patcher);

        /// <summary>Reverts the method patch using the specified <paramref name="patcher"/>
        /// Has no effect if no patches have been applied previously.</summary>
        /// <param name="patcher">The patcher object that can perform the patching.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when the argument is null.</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "patcher", Justification = "Reviewed")]
        void RevertPatch(IPatcher patcher);
    }
}
