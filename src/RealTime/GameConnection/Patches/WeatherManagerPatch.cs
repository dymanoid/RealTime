// <copyright file="WeatherManagerPatch.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.GameConnection.Patches
{
    using System;
    using System.Reflection;
    using SkyTools.Patching;

    /// <summary>
    /// A static class that provides the patch objects for the weather AI .
    /// </summary>
    internal static class WeatherManagerPatch
    {
        /// <summary>Gets the simulation step implementation.</summary>
        public static IPatch SimulationStepImpl { get; } = new WeatherManager_SimulationStepImpl();

        /// <summary>Gets the patch object for the simulation method.</summary>
        private sealed class WeatherManager_SimulationStepImpl : PatchBase
        {
            protected override MethodInfo GetMethod()
            {
                return typeof(WeatherManager).GetMethod(
                    "SimulationStepImpl",
                    BindingFlags.Instance | BindingFlags.NonPublic,
                    null,
                    new[] { typeof(int) },
                    new ParameterModifier[0]);
            }

#pragma warning disable SA1313 // Parameter names must begin with lower-case letter
            private static void Prefix(ref float ___m_temperatureSpeed, float ___m_targetTemperature, float ___m_currentTemperature)
            {
                // The maximum temperature change speed is now 1/20 of the original
                float delta = Math.Abs((___m_targetTemperature - ___m_currentTemperature) * 0.000_05f);
                delta = Math.Min(Math.Abs(___m_temperatureSpeed) + 0.000_01f, delta);
                ___m_temperatureSpeed = delta - 0.000_099f;
            }
#pragma warning restore SA1313 // Parameter names must begin with lower-case letter
        }
    }
}
