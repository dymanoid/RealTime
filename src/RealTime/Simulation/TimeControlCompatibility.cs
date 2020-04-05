// <copyright file="TimeControlCompatibility.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.Simulation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using ColossalFramework.Plugins;
    using SkyTools.Patching;
    using SkyTools.Tools;

    /// <summary>
    /// A special class that handles compatibility with other time-changing mods by patching their methods.
    /// </summary>
    internal static class TimeControlCompatibility
    {
        private const string SimulationStepMethodName = "SimulationStep";
        private const string TimeOfDayPropertySetter = "set_TimeOfDay";
        private const string TimeWarpManagerType = "TimeWarpMod.SunManager";
        private const string UltimateEyecandyManagerType = "UltimateEyecandy.DayNightCycleManager";
        private const ulong TimeWarpWorkshopId = 814698320ul;
        private const ulong UltimateEyecandyWorkshopId = 672248733ul;

        /// <summary>Gets a collection of method patches that need to be applied to ensure this mod's compatibility
        /// with other time-changing mods.</summary>
        /// <returns>A collection of <see cref="IPatch"/> objects.</returns>
        public static IEnumerable<IPatch> GetCompatibilityPatches()
        {
            var timeWarpType = GetManagerType(TimeWarpWorkshopId, TimeWarpManagerType);
            var ultimateEyecandyType = GetManagerType(UltimateEyecandyWorkshopId, UltimateEyecandyManagerType);

            return GetPatches(timeWarpType).Concat(GetPatches(ultimateEyecandyType));
        }

        private static Type GetManagerType(ulong modId, string typeName)
        {
            var mod = PluginManager.instance.GetPluginsInfo()
                .FirstOrDefault(pi => pi.publishedFileID.AsUInt64 == modId);

            if (mod?.isEnabled != true)
            {
                return null;
            }

            var assembly = mod.GetAssemblies()?.FirstOrDefault();
            if (assembly == null)
            {
                Log.Warning($"'Real Time' compatibility check: the mod {modId} has no assemblies.");
                return null;
            }

            try
            {
                return assembly.GetType(typeName);
            }
            catch (Exception ex)
            {
                Log.Warning($"'Real Time' compatibility check: the mod {modId} doesn't contain the '{typeName}' type: {ex}");
                return null;
            }
        }

        private static IEnumerable<IPatch> GetPatches(Type managerType)
        {
            if (managerType != null)
            {
                yield return new SimulationStepPatch(managerType);
                yield return new SetTimeOfDayPatch(managerType);
            }
        }

        private sealed class SimulationStepPatch : PatchBase
        {
            private readonly Type managerType;

            public SimulationStepPatch(Type managerType)
            {
                this.managerType = managerType;
            }

            protected override MethodInfo GetMethod()
            {
                try
                {
                    return managerType?.GetMethod(SimulationStepMethodName, BindingFlags.Instance | BindingFlags.Public);
                }
                catch (Exception ex)
                {
                    Log.Warning($"'Real Time' compatibility check: the '{managerType.Name}' type doesn't contain the '{SimulationStepMethodName}' method: {ex}");
                    return null;
                }
            }

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Redundancy", "RCS1213", Justification = "Harmony patch")]
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming Rules", "SA1313", Justification = "Harmony patch")]
            private static bool Prefix(ref uint ___dayOffsetFrames)
            {
                bool result = SimulationManager.instance.SimulationPaused;
                if (!result)
                {
                    ___dayOffsetFrames = SimulationManager.instance.m_dayTimeOffsetFrames;
                }

                return result;
            }
        }

        private sealed class SetTimeOfDayPatch : PatchBase
        {
            private readonly Type managerType;

            public SetTimeOfDayPatch(Type managerType)
            {
                this.managerType = managerType;
            }

            protected override MethodInfo GetMethod()
            {
                try
                {
                    return managerType?.GetMethod(TimeOfDayPropertySetter, BindingFlags.Instance | BindingFlags.Public);
                }
                catch (Exception ex)
                {
                    Log.Warning($"'Real Time' compatibility check: the '{managerType.Name}' type doesn't contain the '{TimeOfDayPropertySetter}' method: {ex}");
                    return null;
                }
            }

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Redundancy", "RCS1213", Justification = "Harmony patch")]
            private static void Prefix(float value)
            {
                if (Math.Abs(value - SimulationManager.instance.m_currentDayTimeHour) >= 0.03f)
                {
                    SimulationManager.instance.SimulationPaused = true;
                }
            }
        }
    }
}
