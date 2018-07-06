// <copyright file="MethodPatcher.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.Patching
{
    using System.Linq;
    using System.Reflection;
    using Harmony;

    /// <summary>
    /// A class that uses Harmony library for redirecting the game's methods.
    /// </summary>
    internal sealed class MethodPatcher
    {
        private const string HarmonyId = "com.cities_skylines.dymanoid.realtime";

        private readonly HarmonyInstance harmony;

        /// <summary>Initializes a new instance of the <see cref="MethodPatcher"/> class.</summary>
        public MethodPatcher()
        {
            harmony = HarmonyInstance.Create(HarmonyId);
        }

        /// <summary>Applies all patches defined in the current assembly.</summary>
        public void Apply()
        {
            Revert();
            harmony.PatchAll(typeof(MethodPatcher).Assembly);
        }

        /// <summary>Reverts all patches, if any.</summary>
        public void Revert()
        {
            foreach (MethodBase method in harmony.GetPatchedMethods().ToList())
            {
                harmony.RemovePatch(method, HarmonyPatchType.All, HarmonyId);
            }
        }
    }
}
