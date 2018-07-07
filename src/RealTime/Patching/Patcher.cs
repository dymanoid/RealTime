// <copyright file="Patcher.cs" company="dymanoid">Copyright (c) dymanoid. All rights reserved.</copyright>

namespace RealTime.Patching
{
    using System;
    using System.Reflection;
    using Harmony;

    /// <summary>An <see cref="IPatcher"/> implementation based on the Harmony library.</summary>
    /// <seealso cref="IPatcher"/>
    internal sealed class Patcher : IPatcher
    {
        private readonly HarmonyInstance harmony;

        /// <summary>Initializes a new instance of the <see cref="Patcher"/> class.</summary>
        /// <param name="harmony">The harmony instance to use for patching.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="harmony"/> is null.</exception>
        public Patcher(HarmonyInstance harmony)
        {
            this.harmony = harmony ?? throw new ArgumentNullException(nameof(harmony));
        }

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
        public void ApplyPatch(MethodInfo method, MethodInfo prefixCall, MethodInfo postfixCall)
        {
            if (method == null)
            {
                throw new ArgumentNullException(nameof(method));
            }

            if (prefixCall == null && postfixCall == null)
            {
                throw new ArgumentException($"Both {nameof(prefixCall)} and {nameof(postfixCall)} cannot be null at the same time.");
            }

            harmony.Patch(method, new HarmonyMethod(prefixCall), new HarmonyMethod(postfixCall));
        }

        /// <summary>Reverts a patch from the specified <paramref name="method"/>.</summary>
        /// <param name="method">The method to patch.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="method"/> is null.</exception>
        public void RevertPatch(MethodInfo method)
        {
            if (method == null)
            {
                throw new ArgumentNullException(nameof(method));
            }

            harmony.RemovePatch(method, HarmonyPatchType.All, harmony.Id);
        }
    }
}