// <copyright file="BuildingAIHooks.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.GameConnection
{
    using Harmony;
    using RealTime.CustomAI;

    internal static class BuildingAIHooks
    {
        /// <summary>
        /// Gets or sets the custom commercial building simulation class instance.
        /// </summary>
        internal static RealTimeBuildingAI Buildings { get; set; }

        [HarmonyPatch(typeof(CommercialBuildingAI), nameof(SimulationStepActive), null)]
        private static class SimulationStepActive
        {
#pragma warning disable SA1313 // Parameter names must begin with lower-case letter
            public static bool Prefix(ref Building buildingData, ref byte __state)
#pragma warning restore SA1313 // Parameter names must begin with lower-case letter
            {
                __state = buildingData.m_outgoingProblemTimer;
                return true;
            }

#pragma warning disable SA1313 // Parameter names must begin with lower-case letter
            public static void Postfix(ushort buildingID, ref Building buildingData, byte __state)
#pragma warning restore SA1313 // Parameter names must begin with lower-case letter
            {
                if (__state != buildingData.m_outgoingProblemTimer)
                {
                    Buildings?.ProcessOutgoingProblems(buildingID, __state, buildingData.m_outgoingProblemTimer);
                }
            }
        }

        [HarmonyPatch(typeof(PrivateBuildingAI), nameof(HandleWorkers), null)]
        private static class HandleWorkers
        {
#pragma warning disable SA1313 // Parameter names must begin with lower-case letter
            public static bool Prefix(ref Building buildingData, ref byte __state)
#pragma warning restore SA1313 // Parameter names must begin with lower-case letter
            {
                __state = buildingData.m_workerProblemTimer;
                return true;
            }

#pragma warning disable SA1313 // Parameter names must begin with lower-case letter
            public static void Postfix(ushort buildingID, ref Building buildingData, byte __state)
#pragma warning restore SA1313 // Parameter names must begin with lower-case letter
            {
                if (__state != buildingData.m_workerProblemTimer)
                {
                    Buildings?.ProcessWorkerProblems(buildingID, __state, buildingData.m_workerProblemTimer);
                }
            }
        }
    }
}
