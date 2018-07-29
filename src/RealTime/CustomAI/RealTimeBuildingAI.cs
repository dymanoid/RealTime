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

        private static readonly string[] BannedEntertainmentBuildings = { "parking", "garage", "car park" };
        private readonly TimeSpan lightStateCheckInterval = TimeSpan.FromSeconds(15);

        private readonly RealTimeConfig config;
        private readonly ITimeInfo timeInfo;
        private readonly IBuildingManagerConnection buildingManager;
        private readonly IToolManagerConnection toolManager;
        private readonly WorkBehavior workBehavior;
        private readonly TravelBehavior travelBehavior;

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
        /// <param name="workBehavior">A behavior that provides simulation info for the citizens' work time.</param>
        /// <param name="travelBehavior">A behavior that provides simulation info for the citizens' traveling.</param>
        /// <exception cref="ArgumentNullException">Thrown when any argument is null.</exception>
        public RealTimeBuildingAI(
            RealTimeConfig config,
            ITimeInfo timeInfo,
            IBuildingManagerConnection buildingManager,
            IToolManagerConnection toolManager,
            WorkBehavior workBehavior,
            TravelBehavior travelBehavior)
        {
            this.config = config ?? throw new ArgumentNullException(nameof(config));
            this.timeInfo = timeInfo ?? throw new ArgumentNullException(nameof(timeInfo));
            this.buildingManager = buildingManager ?? throw new ArgumentNullException(nameof(buildingManager));
            this.toolManager = toolManager ?? throw new ArgumentNullException(nameof(toolManager));
            this.workBehavior = workBehavior ?? throw new ArgumentNullException(nameof(workBehavior));
            this.travelBehavior = travelBehavior ?? throw new ArgumentNullException(nameof(travelBehavior));

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
        /// <param name="outgoingProblemTimer">The previous value of the outgoing problem timer.</param>
        public void ProcessBuildingProblems(ushort buildingId, byte outgoingProblemTimer)
        {
            // We have only few customers at night - that's an intended behavior.
            // To avoid commercial buildings from collapsing due to lack of customers,
            // we force the problem timer to pause at night time.
            // In the daytime, the timer is running slower.
            if (timeInfo.IsNightTime || timeInfo.Now.Minute % ProblemTimersInterval != 0 || freezeProblemTimers)
            {
                buildingManager.SetOutgoingProblemTimer(buildingId, outgoingProblemTimer);
            }
        }

        /// <summary>
        /// Performs the custom processing of the worker problem timer.
        /// </summary>
        /// <param name="buildingId">The ID of the building to process.</param>
        /// <param name="oldValue">The old value of the worker problem timer.</param>
        public void ProcessWorkerProblems(ushort buildingId, byte oldValue)
        {
            // We force the problem timer to pause at night time.
            // In the daytime, the timer is running slower.
            if (timeInfo.IsNightTime || timeInfo.Now.Minute % ProblemTimersInterval != 0 || freezeProblemTimers)
            {
                buildingManager.SetWorkersProblemTimer(buildingId, oldValue);
            }
        }

        /// <summary>Initializes the state of the all building lights.</summary>
        public void InitializeLightState()
        {
            for (ushort i = 0; i <= StepMask; i++)
            {
                UpdateLightState(i, false);
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
            return config.SwitchOffLightsAtNight && !lightStates[buildingId];
        }

        /// <summary>
        /// Determines whether the building with the specified ID is an entertainment target.
        /// </summary>
        /// <param name="buildingId">The building ID to check.</param>
        /// <returns>
        ///   <c>true</c> if the building is an entertainment target; otherwise, <c>false</c>.
        /// </returns>
        public bool IsEntertainmentTarget(ushort buildingId)
        {
            if (buildingId == 0)
            {
                return true;
            }

            string className = buildingManager.GetBuildingClassName(buildingId);
            if (string.IsNullOrEmpty(className))
            {
                return true;
            }

            for (int i = 0; i < BannedEntertainmentBuildings.Length; ++i)
            {
                if (className.IndexOf(BannedEntertainmentBuildings[i], 0, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Determines whether the building with the specified <paramref name="buildingId"/> is noise restricted
        /// (has NIMBY policy that is active on current time).
        /// </summary>
        /// <param name="buildingId">The building ID to check.</param>
        /// <param name="currentBuildingId">The ID of a building where the citizen starts their journey.
        /// Specify 0 if there is no journey in schedule.</param>
        /// <returns>
        ///   <c>true</c> if the building with the specified <paramref name="buildingId"/> has NIMBY policy
        ///   that is active on current time; otherwise, <c>false</c>.
        /// </returns>
        public bool IsNoiseRestricted(ushort buildingId, ushort currentBuildingId = 0)
        {
            if (buildingManager.GetBuildingSubService(buildingId) != ItemClass.SubService.CommercialLeisure)
            {
                return false;
            }

            float currentHour = timeInfo.CurrentHour;
            if (currentHour >= config.GoToSleepHour || currentHour <= config.WakeUpHour)
            {
                return buildingManager.IsBuildingNoiseRestricted(buildingId);
            }

            if (currentBuildingId == 0)
            {
                return false;
            }

            float travelTime = travelBehavior.GetEstimatedTravelTime(currentBuildingId, buildingId);
            if (travelTime == 0)
            {
                return false;
            }

            float arriveHour = (float)timeInfo.Now.AddHours(travelTime).TimeOfDay.TotalHours;
            if (arriveHour >= config.GoToSleepHour || arriveHour <= config.WakeUpHour)
            {
                return buildingManager.IsBuildingNoiseRestricted(buildingId);
            }

            return false;
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

            UpdateLightState(step, true);
        }

        private void UpdateLightState(ushort step, bool updateBuilding)
        {
            ushort first = (ushort)(step * BuildingStepSize);
            ushort last = (ushort)(((step + 1) * BuildingStepSize) - 1);

            for (ushort i = first; i <= last; ++i)
            {
                buildingManager.GetBuildingService(i, out ItemClass.Service service, out ItemClass.SubService subService);
                bool lightsOn = !ShouldSwitchBuildingLightsOff(i, service, subService);
                if (lightsOn == lightStates[i])
                {
                    continue;
                }

                lightStates[i] = lightsOn;
                if (updateBuilding)
                {
                    buildingManager.UpdateBuildingColors(i);
                }
            }
        }

        private bool ShouldSwitchBuildingLightsOff(ushort buildingId, ItemClass.Service service, ItemClass.SubService subService)
        {
            switch (service)
            {
                case ItemClass.Service.None:
                    return false;

                case ItemClass.Service.Residential:
                    float currentHour = timeInfo.CurrentHour;
                    return currentHour < Math.Min(config.WakeUpHour, EarliestWakeUp) || currentHour >= config.GoToSleepHour;

                case ItemClass.Service.Office when buildingManager.GetBuildingLevel(buildingId) != ItemClass.Level.Level1:
                    return false;

                case ItemClass.Service.Commercial
                    when subService == ItemClass.SubService.CommercialHigh && buildingManager.GetBuildingLevel(buildingId) != ItemClass.Level.Level1:
                    return false;

                case ItemClass.Service.Monument:
                    return false;

                default:
                    return !workBehavior.IsBuildingWorking(service, subService);
            }
        }
    }
}
