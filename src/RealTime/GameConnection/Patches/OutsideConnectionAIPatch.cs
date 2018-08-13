// <copyright file="OutsideConnectionAIPatch.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.GameConnection.Patches
{
    using System;
    using System.Reflection;
    using RealTime.CustomAI;
    using SkyTools.Patching;

    /// <summary>
    /// A static class that provides the patch objects for the outside connections AI.
    /// </summary>
    internal static class OutsideConnectionAIPatch
    {
        /// <summary>Gets or sets the spare time behavior simulation.</summary>
        public static ISpareTimeBehavior SpareTimeBehavior { get; set; }

        /// <summary>Gets the patch object for the method that determines the dummy traffic density.</summary>
        public static IPatch DummyTrafficProbability { get; } = new OutsideConnectionAI_DummyTrafficProbability();

        private sealed class OutsideConnectionAI_DummyTrafficProbability : PatchBase
        {
            protected override MethodInfo GetMethod()
            {
                return typeof(OutsideConnectionAI).GetMethod(
                    "DummyTrafficProbability",
                    BindingFlags.Static | BindingFlags.NonPublic,
                    null,
                    new Type[0],
                    new ParameterModifier[0]);
            }

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Redundancy", "RCS1213", Justification = "Harmony patch")]
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming Rules", "SA1313", Justification = "Harmony patch")]
            private static void Postfix(ref int __result)
            {
                if (SpareTimeBehavior != null)
                {
                    // Using the relaxing chance of an adult as base value - seems to be reasonable.
                    int chance = (int)SpareTimeBehavior.GetRelaxingChance(Citizen.AgeGroup.Adult);
                    __result = __result * chance * chance / 10_000;
                }
            }
        }
    }
}
