// <copyright file="PatchBase.cs" company="dymanoid">Copyright (c) dymanoid. All rights reserved.</copyright>

namespace RealTime.Patching
{
    using System;
    using System.Reflection;

    /// <summary>
    /// A base class for all method patch classes. The derived classes must define at least one static method with names
    /// 'Prefix' and/or 'Postfix' and provide the method to patch by overriding the <see cref="GetMethod"/> abstract method.
    /// </summary>
    /// <seealso cref="IPatch"/>
    internal abstract class PatchBase : IPatch
    {
        private MethodInfo method;

        /// <summary>Applies the method patch using the specified <paramref name="patcher"/>.</summary>
        /// <param name="patcher">The patcher object that can perform the patching.</param>
        /// <exception cref="ArgumentNullException">Thrown when the argument is null.</exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when both the 'Prefix' and 'Postfix' methods are missing
        /// in the derived type.
        /// </exception>
        void IPatch.ApplyPatch(IPatcher patcher)
        {
            if (patcher == null)
            {
                throw new ArgumentNullException(nameof(patcher));
            }

            if (method == null)
            {
                method = GetMethod();
            }
            else
            {
                return;
            }

            if (method == null)
            {
                return;
            }

            MethodInfo prefix = GetType().GetMethod("Prefix", BindingFlags.Static | BindingFlags.NonPublic);
            MethodInfo postfix = GetType().GetMethod("Postfix", BindingFlags.Static | BindingFlags.NonPublic);

            if (prefix == null && postfix == null)
            {
                throw new InvalidOperationException("At least one of the 'Prefix' and 'Postfix' methods must be defined");
            }

            patcher.ApplyPatch(method, prefix, postfix);
        }

        /// <summary>
        /// Reverts the method patch using the specified <paramref name="patcher"/> Has no effect if no patches have been
        /// applied previously.
        /// </summary>
        /// <param name="patcher">The patcher object that can perform the patching.</param>
        /// <exception cref="ArgumentNullException">Thrown when the argument is null.</exception>
        void IPatch.RevertPatch(IPatcher patcher)
        {
            if (patcher == null)
            {
                throw new ArgumentNullException(nameof(patcher));
            }

            MethodInfo patchedMethod = method ?? GetMethod();
            if (patchedMethod != null)
            {
                patcher.RevertPatch(patchedMethod);
                method = null;
            }
        }

        /// <summary>
        /// When overridden in derived classes, gets the <see cref="MethodInfo"/> instance of the method to patch.
        /// Can return null if no patching is possible due to some reason.
        /// </summary>
        /// <returns>A <see cref="MethodInfo"/> instance of the method to patch, or null.</returns>
        protected abstract MethodInfo GetMethod();
    }
}