// <copyright file="IPatch.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.Patching
{
    /// <summary>
    /// An interface for classes that apply and revert method patches using an external <see cref="IPatcher"/>.
    /// </summary>
    internal interface IPatch
    {
        /// <summary>Applies the method patch using the specified <paramref name="patcher"/>.</summary>
        /// <param name="patcher">The patcher object that can perform the patching.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when the argument is null.</exception>
        void ApplyPatch(IPatcher patcher);

        /// <summary>Reverts the method patch using the specified <paramref name="patcher"/>
        /// Has no effect if no patches have been applied previously.</summary>
        /// <param name="patcher">The patcher object that can perform the patching.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when the argument is null.</exception>
        void RevertPatch(IPatcher patcher);
    }
}
