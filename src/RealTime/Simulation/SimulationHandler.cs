﻿// <copyright file="SimulationHandler.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.Simulation
{
    using System;
    using ICities;
    using RealTime.CustomAI;
    using RealTime.Events;

    /// <summary>
    /// A central simulation handler that dispatches the simulation frame processing
    /// to the custom logic class instances.
    /// </summary>
    public sealed class SimulationHandler : ThreadingExtensionBase
    {
        private DateTime lastHandledDate;

        /// <summary>
        /// Occurs when a new day in the game begins.
        /// </summary>
        internal static event EventHandler NewDay;

        /// <summary>
        /// Gets or sets the custom event manager simulation class instance.
        /// </summary>
        internal static RealTimeEventManager EventManager { get; set; }

        /// <summary>
        /// Gets or sets the day time simulation class instance.
        /// </summary>
        internal static DayTimeSimulation DayTimeSimulation { get; set; }

        /// <summary>
        /// Gets or sets the custom building simulation class instance.
        /// </summary>
        internal static RealTimeBuildingAI Buildings { get; set; }

        /// <summary>Gets or sets the time adjustment simulation class instance.</summary>
        internal static TimeAdjustment TimeAdjustment { get; set; }

        /// <summary>Gets or sets the weather information class instance.</summary>
        internal static WeatherInfo WeatherInfo { get; set; }

        /// <summary>Gets or sets the citizen processing class instance.</summary>
        internal static CitizenProcessor<ResidentAI, Citizen> CitizenProcessor { get; set; }

        /// <summary>Gets or sets the statistics processing class instance.</summary>
        internal static Statistics Statistics { get; set; }

        /// <summary>
        /// Called before each game simulation tick. A tick contains multiple frames.
        /// Performs the dispatching for this simulation phase.
        /// </summary>
        public override void OnBeforeSimulationTick()
        {
            WeatherInfo?.Update();
            EventManager?.ProcessEvents();

            bool updateFrameLength = TimeAdjustment?.Update() ?? false;

            if (CitizenProcessor != null)
            {
                if (updateFrameLength)
                {
                    CitizenProcessor.UpdateFrameDuration();
                }

                CitizenProcessor.ProcessTick();
            }

            if (updateFrameLength)
            {
                Buildings?.UpdateFrameDuration();
                Statistics?.RefreshUnits();
            }

            if (DayTimeSimulation == null || CitizenProcessor == null)
            {
                return;
            }

            DateTime currentDate = SimulationManager.instance.m_currentGameTime.Date;
            if (currentDate != lastHandledDate)
            {
                lastHandledDate = currentDate;
                DayTimeSimulation.Process(currentDate);
                CitizenProcessor.StartNewDay();
                OnNewDay(this);
            }
        }

        /// <summary>
        /// Called before each game simulation frame. Performs the dispatching for this simulation phase.
        /// </summary>
        public override void OnBeforeSimulationFrame()
        {
            uint currentFrame = SimulationManager.instance.m_currentFrameIndex;
            Buildings?.ProcessFrame(currentFrame);
            CitizenProcessor?.ProcessFrame(currentFrame);
        }

        private static void OnNewDay(SimulationHandler sender)
        {
            NewDay?.Invoke(sender, EventArgs.Empty);
        }
    }
}
