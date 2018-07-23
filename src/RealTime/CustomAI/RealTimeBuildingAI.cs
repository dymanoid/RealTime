// <copyright file="RealTimeBuildingAI.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.CustomAI
{
    using System;
    using RealTime.Config;
    using RealTime.GameConnection;
    using RealTime.Simulation;
    using static Constants;

    /// <summary>
    /// A class that incorporated the custom logic for the private buildings.
    /// </summary>
    internal sealed class RealTimeBuildingAI
    {
        private const int ConstructionSpeedPaused = 10880;
        private const int ConstructionSpeedMinimum = 1088;
        private const int StepMask = 0xFF;
        private const int BuildingStepSize = 192;

        private readonly TimeSpan lightStateCheckInterval = TimeSpan.FromSeconds(15);

        private readonly RealTimeConfig config;
        private readonly ITimeInfo timeInfo;
        private readonly IToolManagerConnection toolManager;
        private readonly WorkBehavior workBehavior;
        private readonly IBuildingManagerConnection buildingManager;
        private readonly bool[] lightStates;

        private int lastProcessedMinute = -1;
        private bool freezeProblemTimers;

        private uint lastConfigConstructionSpeedValue;
        private double constructionSpeedValue;

        private int lightStateCheckFramesInterval;
        private int lightStateCheckCounter;
        private ushort lightCheckStep;

        /// <summary>
        /// Initializes a new instance of the <see cref="RealTimeBuildingAI"/> class.
        /// </summary>
        ///
        /// <param name="config">The configuration to run with.</param>
        /// <param name="timeInfo">The time information source.</param>
        /// <param name="buildingManager">A proxy object that provides a way to call the game-specific methods of the <see cref="BuildingManager"/> class.</param>
        /// <param name="toolManager">A proxy object that provides a way to call the game-specific methods of the <see cref="ToolManager"/> class.</param>
        /// <param name="workBehavior">A behavior that provides simulation info for the citizens work time.</param>
        /// <exception cref="ArgumentNullException">Thrown when any argument is null.</exception>
        public RealTimeBuildingAI(
            RealTimeConfig config,
            ITimeInfo timeInfo,
            IBuildingManagerConnection buildingManager,
            IToolManagerConnection toolManager,
            WorkBehavior workBehavior)
        {
            this.config = config ?? throw new ArgumentNullException(nameof(config));
            this.timeInfo = timeInfo ?? throw new ArgumentNullException(nameof(timeInfo));
            this.buildingManager = buildingManager ?? throw new ArgumentNullException(nameof(buildingManager));
            this.toolManager = toolManager ?? throw new ArgumentNullException(nameof(toolManager));
            this.workBehavior = workBehavior ?? throw new ArgumentNullException(nameof(workBehavior));

            lightStates = new bool[buildingManager.GetMaxBuildingsCount()];
        }

        /// <summary>
        /// Gets the building construction time taking into account the current day time.
        /// </summary>
        ///
        /// <returns>The building construction time in game-specific units (0..10880)</returns>
        public int GetConstructionTime()
        {
            if ((toolManager.GetCurrentMode() & ItemClass.Availability.AssetEditor) != 0)
            {
                return 0;
            }

            if (config.ConstructionSpeed != lastConfigConstructionSpeedValue)
            {
                lastConfigConstructionSpeedValue = config.ConstructionSpeed;
                double inverted = 101d - lastConfigConstructionSpeedValue;
                constructionSpeedValue = inverted * inverted * inverted / 1_000_000d;
            }

            // This causes the construction to not advance in the night time
            return timeInfo.IsNightTime && config.StopConstructionAtNight
                ? ConstructionSpeedPaused
                : (int)(ConstructionSpeedMinimum * constructionSpeedValue);
        }

        /// <summary>
        /// Performs the custom processing of the outgoing problem timer.
        /// </summary>
        /// <param name="buildingId">The ID of the building to process.</param>
        /// <param name="oldValue">The old value of the outgoing problem timer.</param>
        /// <param name="newValue">The new value of the outgoing problem timer.</param>
        public void ProcessOutgoingProblems(ushort buildingId, byte oldValue, byte newValue)
        {
            // We have only few customers at night - that's an intended behavior.
            // To avoid commercial buildings from collapsing due to lack of customers,
            // we force the problem timer to pause at night time.
            // In the daytime, the timer is running slower.
            if (timeInfo.IsNightTime || timeInfo.Now.Minute % ProblemTimersInterval != 0 || freezeProblemTimers)
            {
                buildingManager.SetOutgoingProblemTimer(buildingId, oldValue);
            }
        }

        /// <summary>
        /// Performs the custom processing of the worker problem timer.
        /// </summary>
        /// <param name="buildingId">The ID of the building to process.</param>
        /// <param name="oldValue">The old value of the worker problem timer.</param>
        /// <param name="newValue">The new value of the worker problem timer.</param>
        public void ProcessWorkerProblems(ushort buildingId, byte oldValue, byte newValue)
        {
            // We force the problem timer to pause at night time.
            // In the daytime, the timer is running slower.
            if (timeInfo.IsNightTime || timeInfo.Now.Minute % ProblemTimersInterval != 0 || freezeProblemTimers)
            {
                buildingManager.SetWorkersProblemTimer(buildingId, oldValue);
            }
        }

        /// <summary>Re-calculates the duration of a simulation frame.</summary>
        public void UpdateFrameDuration()
        {
            lightStateCheckFramesInterval = (int)(lightStateCheckInterval.TotalHours / timeInfo.HoursPerFrame);
            if (lightStateCheckFramesInterval == 0)
            {
                ++lightStateCheckFramesInterval;
            }
        }

        /// <summary>Notifies this simulation object that a new simulation frame is started.
        /// The buildings will be processed again from the beginning of the list.</summary>
        /// <param name="frameIndex">The simulation frame index to process.</param>
        public void ProcessFrame(uint frameIndex)
        {
            UpdateLightState(frameIndex);

            if ((frameIndex & StepMask) != 0)
            {
                return;
            }

            int currentMinute = timeInfo.Now.Minute;
            if (lastProcessedMinute != currentMinute)
            {
                lastProcessedMinute = currentMinute;
                freezeProblemTimers = false;
            }
            else
            {
                freezeProblemTimers = true;
            }
        }

        /// <summary>
        /// Determines whether the lights should be switched off in the specified building.
        /// </summary>
        /// <param name="buildingId">The ID of the building to check.</param>
        /// <returns>
        ///   <c>true</c> if the lights should be switched off in the specified building; otherwise, <c>false</c>.
        /// </returns>
        public bool ShouldSwitchBuildingLightsOff(ushort buildingId)
        {
            return lightStates[buildingId];
        }

        private void UpdateLightState(uint frameIndex)
        {
            if (lightStateCheckCounter > 0)
            {
                --lightStateCheckCounter;
                return;
            }

            ushort step = lightCheckStep;
            lightCheckStep = (ushort)((step + 1) & StepMask);
            lightStateCheckCounter = lightStateCheckFramesInterval;

            ushort first = (ushort)(step * BuildingStepSize);
            ushort last = (ushort)(((step + 1) * BuildingStepSize) - 1);

            for (ushort i = first; i <= last; ++i)
            {
                buildingManager.GetBuildingService(i, out ItemClass.Service service, out ItemClass.SubService subService);
                bool newState = ShouldSwitchBuildingLightsOff(service, subService);
                if (newState != lightStates[i])
                {
                    lightStates[i] = newState;
                    buildingManager.UpdateBuildingColors(i);
                }
            }
        }

        private bool ShouldSwitchBuildingLightsOff(ItemClass.Service service, ItemClass.SubService subService)
        {
            float currentHour = timeInfo.CurrentHour;
            if (currentHour >= config.WakeupHour && currentHour < config.GoToSleepUpHour)
            {
                return false;
            }

            if (service != ItemClass.Service.Residential)
            {
                return !workBehavior.IsBuildingWorking(service, subService);
            }

            return true;
        }
    }
}
