// <copyright file="SimulationHandler.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.Simulation
{
    using System;
    using ICities;
    using RealTime.BuildingAI;
    using RealTime.Events;

    /// <summary>
    /// A simulation extension that manages the daytime calculation (sunrise and sunset).
    /// </summary>
    public sealed class SimulationHandler : ThreadingExtensionBase
    {
        private DateTime lastHandledDate;

        /// <summary>
        /// Occurs whent a new day in the game begins.
        /// </summary>
        internal static event EventHandler NewDay;

        internal static RealTimeEventManager EventManager { get; set; }

        internal static DayTimeSimulation DayTimeSimulation { get; set; }

        internal static CommercialAI CommercialAI { get; set; }

        /// <summary>
        /// Called after each game simulation tick. Performs the actual work.
        /// </summary>
        public override void OnAfterSimulationTick()
        {
            EventManager?.ProcessEvents();

            DateTime currentDate = SimulationManager.instance.m_currentGameTime.Date;
            if (currentDate != lastHandledDate)
            {
                lastHandledDate = currentDate;
                DayTimeSimulation?.Process(currentDate);
                OnNewDay(this);
            }
        }

        public override void OnAfterSimulationFrame()
        {
            CommercialAI?.Process(SimulationManager.instance.m_currentFrameIndex);
        }

        private static void OnNewDay(SimulationHandler sender)
        {
            NewDay?.Invoke(sender, EventArgs.Empty);
        }
    }
}
