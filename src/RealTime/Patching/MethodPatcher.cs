// <copyright file="MethodPatcher.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace SkyTools.Patching
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using HarmonyLib;
    using SkyTools.Tools;

    /// <summary>
    /// A class that uses Harmony library for redirecting the game's methods.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Patcher", Justification = "Reviewed")]
    public sealed class MethodPatcher
    {
        private readonly Patcher patcher;
        private readonly string id;
        private readonly IEnumerable<IPatch> patches;

        /// <summary>Initializes a new instance of the <see cref="MethodPatcher"/> class.</summary>
        /// <param name="id">The unique ID to use in Harmony.</param>
        /// <param name="patches">The patches to process by this object.</param>
        /// <exception cref="ArgumentException">Thrown when no patches specified.</exception>
        public MethodPatcher(string id, IEnumerable<IPatch> patches)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentException("The ID cannot be null or an empty string", nameof(id));
            }

            if (patches?.Any() != true)
            {
                throw new ArgumentException("At least one patch is required");
            }

            this.id = id;
            this.patches = patches;
            var harmony = new Harmony(id);
            patcher = new Patcher(harmony);
        }

        /// <summary>Applies all patches this object knows about.</summary>
        /// <returns>A collection of successfully applied patches.</returns>
        public HashSet<IPatch> Apply()
        {
            Revert();

            var result = new HashSet<IPatch>();
            foreach (var patch in patches)
            {
                try
                {
                    patch.ApplyPatch(patcher);
                    result.Add(patch);
                }
                catch (Exception ex)
                {
                    Log.Info($"Harmony (ID '{id}') method patch failed for {patch}: {ex}");
                }
            }

            Log.Info($"{result.Count} Harmony method patches with ID '{id}' successfully applied.");
            return result;
        }

        /// <summary>Reverts all patches, if any applied.</summary>
        public void Revert()
        {
            foreach (var patch in patches)
            {
                try
                {
                    patch.RevertPatch(patcher);
                }
                catch (Exception ex)
                {
                    Log.Error($"Failed reverting a Harmony method patch {patch} with ID '{id}'. Error message: {ex}");
                }
            }
        }

        private sealed class Patcher : IPatcher
        {
            private readonly Harmony harmony;

            public Patcher(Harmony harmony)
            {
                this.harmony = harmony;
            }

            public void ApplyPatch(MethodInfo method, MethodInfo prefixCall, MethodInfo postfixCall, MethodInfo transformCall)
            {
                if (method == null)
                {
                    throw new ArgumentNullException(nameof(method));
                }

                if (prefixCall == null && postfixCall == null && transformCall == null)
                {
                    throw new ArgumentException($"All call methods ({nameof(prefixCall)}, {nameof(postfixCall)}, {nameof(transformCall)}) cannot be null at the same time.");
                }

                var prefix = prefixCall == null ? null : new HarmonyMethod(prefixCall);
                var postfix = postfixCall == null ? null : new HarmonyMethod(postfixCall);
                var transpiler = transformCall == null ? null : new HarmonyMethod(transformCall);
                harmony.Patch(method, prefix, postfix, transpiler);
            }

            public void RevertPatch(MethodInfo method)
            {
                if (method == null)
                {
                    throw new ArgumentNullException(nameof(method));
                }

                harmony.Unpatch(method, HarmonyPatchType.All, harmony.Id);
            }
        }
    }
}
