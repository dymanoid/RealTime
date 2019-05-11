// <copyright file="SimulationHandler.cs" company="dymanoid">
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
        private bool wasPaused;

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
            if (SimulationManager.instance.SimulationPaused || SimulationManager.instance.ForcedSimulationPaused)
            {
                wasPaused = true;
                return;
            }

            WeatherInfo?.Update();

            bool updateFrameLength = TimeAdjustment?.Update(wasPaused) ?? false;
            wasPaused = false;

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
                VanillaEvents.ProcessUpdatedTimeSpeed(TimeAdjustment.GetOriginalTime);
            }

            EventManager?.ProcessEvents();

            if (DayTimeSimulation == null || CitizenProcessor == null)
            {
                return;
            }

            DateTime currentDateTime = SimulationManager.instance.m_currentGameTime;
            if (currentDateTime.Hour != lastHandledDate.Hour || lastHandledDate == default)
            {
                int triggerHour = lastHandledDate == default
                    ? 0
                    : currentDateTime.Hour;

                lastHandledDate = currentDateTime;
                CitizenProcessor.TriggerHour(triggerHour);
                if (triggerHour == 0)
                {
                    DayTimeSimulation.Process(currentDateTime);
                    OnNewDay(this);
                }
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

        /// <summary>Called by the simulation manager when an update is required.</summary>
        /// <param name="realTimeDelta">The real time delta time.</param>
        /// <param name="simulationTimeDelta">The simulation delta time.</param>
        public override void OnUpdate(float realTimeDelta, float simulationTimeDelta) => TimeAdjustment?.UpdateSunPosition();

        private static void OnNewDay(SimulationHandler sender) => NewDay?.Invoke(sender, EventArgs.Empty);
    }
}
