// <copyright file="AwakeSleepSimulation.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.Simulation
{
    using System;
    using System.Reflection;
    using ColossalFramework.IO;
    using RealTime.Config;
    using RealTime.Tools;

    /// <summary>
    /// A simulation class that decouples the cities' 'night time' from the sun position.
    /// CAUTION: this class hacks a <see cref="SimulationManager"/> static field using reflection.
    /// It might cause unexpected game behavior.
    /// </summary>
    /// <seealso cref="ISimulationManager" />
    internal sealed class AwakeSleepSimulation : ISimulationManager
    {
        private readonly ThreadProfiler threadProfiler;
        private readonly RealTimeConfig config;

        private AwakeSleepSimulation(RealTimeConfig config)
        {
            threadProfiler = new ThreadProfiler();
            this.config = config;
        }

        /// <summary>Installs the simulation into the game using the specified configuration.</summary>
        /// <param name="config">The configuration to run with.</param>
        /// <returns><c>true</c> when it succeeds; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the argument is null.</exception>
        public static bool Install(RealTimeConfig config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            FastList<ISimulationManager> managers = GetSimulationManagers();
            if (managers == null)
            {
                Log.Error("The 'Real Time' mod failed to get the simulation managers");
                return false;
            }

            if (managers.m_size == 0)
            {
                Log.Error("The 'Real Time' mod failed to register the awake/sleep simulation - no simulation managers found");
                return false;
            }

            if (managers.m_buffer[0] is AwakeSleepSimulation)
            {
                return true;
            }

            if (managers.m_buffer[managers.m_buffer.Length - 1] != null)
            {
                managers.EnsureCapacity(managers.m_buffer.Length + 4);
            }

            ++managers.m_size;

            for (int i = managers.m_size - 1; i > 0; --i)
            {
                managers.m_buffer[i] = managers.m_buffer[i - 1];
            }

            managers.m_buffer[0] = new AwakeSleepSimulation(config);
            return true;
        }

        /// <summary>Uninstalls the previously installed simulation. Has no effect if no simulation was installed.</summary>
        public static void Uninstall()
        {
            FastList<ISimulationManager> managers = GetSimulationManagers();
            if (managers == null || managers.m_size == 0 || !(managers.m_buffer[0] is AwakeSleepSimulation))
            {
                return;
            }

            for (int i = 0; i < managers.m_size - 1; ++i)
            {
                managers.m_buffer[i] = managers.m_buffer[i + 1];
            }

            managers.m_buffer[managers.m_size - 1] = null;
            --managers.m_size;
        }

#pragma warning disable SA1600 // Elements must be documented; the original interface is not documented
        void ISimulationManager.EarlyUpdateData()
        {
        }

        void ISimulationManager.GetData(FastList<IDataContainer> data)
        {
        }

        string ISimulationManager.GetName()
        {
            return "RealTimeAwakeSleepSimulation";
        }

        ThreadProfiler ISimulationManager.GetSimulationProfiler()
        {
            return threadProfiler;
        }

        void ISimulationManager.LateUpdateData(SimulationManager.UpdateMode mode)
        {
        }

        void ISimulationManager.SimulationStep(int subStep)
        {
            if (subStep == 0 || !SimulationManager.instance.m_enableDayNight)
            {
                return;
            }

            float currentHour = (float)SimulationManager.instance.m_currentGameTime.TimeOfDay.TotalHours;
            SimulationManager.instance.m_isNightTime = currentHour < config.WakeUpHour || currentHour >= config.GoToSleepHour;
        }

        void ISimulationManager.UpdateData(SimulationManager.UpdateMode mode)
        {
        }
#pragma warning restore SA1600 // Elements must be documented

        private static FastList<ISimulationManager> GetSimulationManagers()
        {
            FieldInfo field;
            try
            {
                field = typeof(SimulationManager).GetField("m_managers", BindingFlags.Static | BindingFlags.NonPublic);
            }
            catch (Exception ex)
            {
                Log.Error("The 'Real Time' mod failed to get the simulation managers field: " + ex);
                return null;
            }

            if (field == null)
            {
                Log.Error("The 'Real Time' mod failed to get the simulation managers field");
                return null;
            }

            return field.GetValue(SimulationManager.instance) as FastList<ISimulationManager>;
        }
    }
}
