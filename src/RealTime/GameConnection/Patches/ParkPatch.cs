// <copyright file="ParkPatch.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.GameConnection.Patches
{
    using System.Reflection;
    using RealTime.CustomAI;
    using SkyTools.Patching;

    /// <summary>
    /// A static class that provides the patch objects for the Park Life DLC related methods.
    /// </summary>
    internal static class ParkPatch
    {
        /// <summary>Gets the patch for the district park simulation method.</summary>
        public static IPatch DistrictParkSimulation { get; } = new DistrictPark_SimulationStep();

        /// <summary>Gets or sets the city spare time behavior.</summary>
        public static ISpareTimeBehavior SpareTimeBehavior { get; set; }

        private sealed class DistrictPark_SimulationStep : PatchBase
        {
            protected override MethodInfo GetMethod() =>
                typeof(DistrictPark).GetMethod(
                    "SimulationStep",
                    BindingFlags.Instance | BindingFlags.Public,
                    null,
                    new[] { typeof(byte) },
                    new ParameterModifier[0]);

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Redundancy", "RCS1213", Justification = "Harmony patch")]
            private static void Postfix(byte parkID)
            {
                ref DistrictPark park = ref DistrictManager.instance.m_parks.m_buffer[parkID];

                if (!SpareTimeBehavior.AreFireworksAllowed)
                {
                    park.m_flags &= ~DistrictPark.Flags.SpecialMode;
                    return;
                }

                if (park.m_dayNightCount == 6 || (park.m_parkPolicies & DistrictPolicies.Park.FireworksBoost) != 0)
                {
                    park.m_flags |= DistrictPark.Flags.SpecialMode;
                }
            }
        }
    }
}
