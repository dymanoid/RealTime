// <copyright file="BuildingAIPatches.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.GameConnection.Patches
{
    using System;
    using System.Reflection;
    using RealTime.CustomAI;
    using RealTime.Patching;

    /// <summary>
    /// A static class that provides the patch objects for the building AI game methods.
    /// </summary>
    internal static class BuildingAIPatches
    {
        /// <summary>Gets or sets the custom AI object for buildings.</summary>
        public static RealTimePrivateBuildingAI RealTimeAI { get; set; }

        /// <summary>Gets the patch for the commercial building AI class.</summary>
        public static IPatch CommercialSimulation { get; } = new CommercialBuildingA_SimulationStepActive();

        /// <summary>Gets the first patch for the private building AI class.</summary>
        public static IPatch PrivateHandleWorkers { get; } = new PrivateBuildingAI_HandleWorkers();

        /// <summary>Gets the second patch for the private building AI class.</summary>
        public static IPatch PrivateConstructionTime { get; } = new PrivateBuildingAI_GetConstructionTime();

        private sealed class CommercialBuildingA_SimulationStepActive : PatchBase
        {
            protected override MethodInfo GetMethod()
            {
                return typeof(CommercialBuildingAI).GetMethod(
                    "SimulationStepActive",
                    BindingFlags.Instance | BindingFlags.NonPublic,
                    null,
                    new[] { typeof(ushort), typeof(Building).MakeByRefType(), typeof(Building.Frame).MakeByRefType() },
                    new ParameterModifier[0]);
            }

#pragma warning disable SA1313 // Parameter names must begin with lower-case letter
            private static bool Prefix(ref Building buildingData, ref byte __state)
            {
                __state = buildingData.m_outgoingProblemTimer;
                return true;
            }

            private static void Postfix(ushort buildingID, ref Building buildingData, byte __state)
            {
                if (__state != buildingData.m_outgoingProblemTimer)
                {
                    RealTimeAI?.ProcessOutgoingProblems(buildingID, __state, buildingData.m_outgoingProblemTimer);
                }
            }
#pragma warning restore SA1313 // Parameter names must begin with lower-case letter
        }

        private sealed class PrivateBuildingAI_HandleWorkers : PatchBase
        {
            protected override MethodInfo GetMethod()
            {
                Type refInt = typeof(int).MakeByRefType();

                return typeof(PrivateBuildingAI).GetMethod(
                    "HandleWorkers",
                    BindingFlags.Instance | BindingFlags.NonPublic,
                    null,
                    new[] { typeof(ushort), typeof(Building).MakeByRefType(), typeof(Citizen.BehaviourData).MakeByRefType(), refInt, refInt, refInt },
                    new ParameterModifier[0]);
            }

#pragma warning disable SA1313 // Parameter names must begin with lower-case letter
            private static bool Prefix(ref Building buildingData, ref byte __state)
            {
                __state = buildingData.m_workerProblemTimer;
                return true;
            }

            private static void Postfix(ushort buildingID, ref Building buildingData, byte __state)
            {
                if (__state != buildingData.m_workerProblemTimer)
                {
                    RealTimeAI?.ProcessWorkerProblems(buildingID, __state, buildingData.m_workerProblemTimer);
                }
            }
#pragma warning restore SA1313 // Parameter names must begin with lower-case letter
        }

        private sealed class PrivateBuildingAI_GetConstructionTime : PatchBase
        {
            protected override MethodInfo GetMethod()
            {
                return typeof(PrivateBuildingAI).GetMethod(
                    "GetConstructionTime",
                    BindingFlags.Instance | BindingFlags.NonPublic,
                    null,
                    new Type[0],
                    new ParameterModifier[0]);
            }

#pragma warning disable SA1313 // Parameter names must begin with lower-case letter
            private static bool Prefix(ref int __result)
            {
                __result = RealTimeAI?.GetConstructionTime() ?? 0;
                return false;
            }
#pragma warning restore SA1313 // Parameter names must begin with lower-case letter
        }
    }
}
