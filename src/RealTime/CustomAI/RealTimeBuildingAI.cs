// <copyright file="RealTimeBuildingAI.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.CustomAI
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using RealTime.Config;
    using RealTime.GameConnection;
    using RealTime.Simulation;
    using static Constants;

    /// <summary>
    /// A class that incorporates the custom logic for the private buildings.
    /// </summary>
    internal sealed class RealTimeBuildingAI : IRealTimeBuildingAI
    {
        private const int ConstructionSpeedPaused = 10880;
        private const int ConstructionSpeedMinimum = 1088;
        private const int StepMask = 0xFF;
        private const int BuildingStepSize = 192;
        private const int ConstructionRestrictionThreshold1 = 100;
        private const int ConstructionRestrictionThreshold2 = 1_000;
        private const int ConstructionRestrictionThreshold3 = 10_000;
        private const int ConstructionRestrictionStep1 = MaximumBuildingsInConstruction / 10;
        private const int ConstructionRestrictionStep2 = MaximumBuildingsInConstruction / 5;
        private const int ConstructionRestrictionScale2 = ConstructionRestrictionThreshold2 / (ConstructionRestrictionStep2 - ConstructionRestrictionStep1);
        private const int ConstructionRestrictionScale3 = ConstructionRestrictionThreshold3 / (MaximumBuildingsInConstruction - ConstructionRestrictionStep2);

        private static readonly string[] BannedEntertainmentBuildings = { "parking", "garage", "car park" };
        private readonly TimeSpan lightStateCheckInterval = TimeSpan.FromSeconds(15);

        private readonly RealTimeConfig config;
        private readonly ITimeInfo timeInfo;
        private readonly IBuildingManagerConnection buildingManager;
        private readonly IToolManagerConnection toolManager;
        private readonly IWorkBehavior workBehavior;
        private readonly ITravelBehavior travelBehavior;

        private readonly bool[] lightStates;
        private readonly byte[] reachingTroubles;
        private readonly HashSet<ushort>[] buildingsInConstruction;

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
            IWorkBehavior workBehavior,
            ITravelBehavior travelBehavior)
        {
            this.config = config ?? throw new ArgumentNullException(nameof(config));
            this.timeInfo = timeInfo ?? throw new ArgumentNullException(nameof(timeInfo));
            this.buildingManager = buildingManager ?? throw new ArgumentNullException(nameof(buildingManager));
            this.toolManager = toolManager ?? throw new ArgumentNullException(nameof(toolManager));
            this.workBehavior = workBehavior ?? throw new ArgumentNullException(nameof(workBehavior));
            this.travelBehavior = travelBehavior ?? throw new ArgumentNullException(nameof(travelBehavior));

            lightStates = new bool[buildingManager.GetMaxBuildingsCount()];
            for (int i = 0; i < lightStates.Length; ++i)
            {
                lightStates[i] = true;
            }

            reachingTroubles = new byte[lightStates.Length];

            // This is to preallocate the hash sets to a large capacity, .NET 3.5 doesn't provide a proper way.
            var preallocated = Enumerable.Range(0, MaximumBuildingsInConstruction * 2).Select(v => (ushort)v).ToList();
            buildingsInConstruction = new[]
            {
                new HashSet<ushort>(preallocated),
                new HashSet<ushort>(preallocated),
                new HashSet<ushort>(preallocated),
                new HashSet<ushort>(preallocated),
            };

            for (int i = 0; i < buildingsInConstruction.Length; ++i)
            {
                // Calling Clear() doesn't trim the capacity, we're using this trick for preallocating the hash sets
                buildingsInConstruction[i].Clear();
            }
        }

        /// <summary>
        /// Gets the building construction time taking into account the current day time.
        /// </summary>
        ///
        /// <returns>The building construction time in game-specific units (0..10880).</returns>
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
        /// Determines whether a building can be constructed or upgraded in the specified building zone.
        /// </summary>
        /// <param name="buildingZone">The building zone to check.</param>
        /// <param name="buildingId">The building ID. Can be 0 if we're about to construct a new building.</param>
        /// <returns>
        ///   <c>true</c> if a building can be constructed or upgraded; otherwise, <c>false</c>.
        /// </returns>
        public bool CanBuildOrUpgrade(ItemClass.Service buildingZone, ushort buildingId = 0)
        {
            int index;
            switch (buildingZone)
            {
                case ItemClass.Service.Residential:
                    index = 0;
                    break;

                case ItemClass.Service.Commercial:
                    index = 1;
                    break;

                case ItemClass.Service.Industrial:
                    index = 2;
                    break;

                case ItemClass.Service.Office:
                    index = 3;
                    break;

                default:
                    return true;
            }

            var buildings = buildingsInConstruction[index];
            buildings.RemoveWhere(IsBuildingCompletedOrMissing);

            int allowedCount = GetAllowedConstructingUpradingCount(buildingManager.GeBuildingsCount());
            bool result = buildings.Count < allowedCount;
            if (result && buildingId != 0)
            {
                buildings.Add(buildingId);
            }

            return result;
        }

        /// <summary>Registers the building with specified <paramref name="buildingId"/> as being constructed or
        /// upgraded.</summary>
        /// <param name="buildingId">The building ID to register.</param>
        /// <param name="buildingZone">The building zone.</param>
        public void RegisterConstructingBuilding(ushort buildingId, ItemClass.Service buildingZone)
        {
            switch (buildingZone)
            {
                case ItemClass.Service.Residential:
                    buildingsInConstruction[0].Add(buildingId);
                    return;

                case ItemClass.Service.Commercial:
                    buildingsInConstruction[1].Add(buildingId);
                    return;

                case ItemClass.Service.Industrial:
                    buildingsInConstruction[2].Add(buildingId);
                    return;

                case ItemClass.Service.Office:
                    buildingsInConstruction[3].Add(buildingId);
                    return;
            }
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
                UpdateLightState(i, updateBuilding: false);
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
            UpdateReachingTroubles(frameIndex & StepMask);
            UpdateLightState();

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
        public bool ShouldSwitchBuildingLightsOff(ushort buildingId) => config.SwitchOffLightsAtNight && !lightStates[buildingId];

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

            // A building still can post outgoing offers while inactive.
            // This is to prevent those offers from being dispatched.
            if (!buildingManager.BuildingHasFlags(buildingId, Building.Flags.Active))
            {
                return false;
            }

            var buildingService = buildingManager.GetBuildingService(buildingId);
            if (buildingService == ItemClass.Service.VarsitySports)
            {
                // Do not visit varsity sport arenas for entertainment when no active events
                return false;
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
        /// Determines whether the building with the specified ID is a shopping target.
        /// </summary>
        /// <param name="buildingId">The building ID to check.</param>
        /// <returns>
        ///   <c>true</c> if the building is a shopping target; otherwise, <c>false</c>.
        /// </returns>
        public bool IsShoppingTarget(ushort buildingId)
        {
            if (buildingId == 0)
            {
                return true;
            }

            // A building still can post outgoing offers while inactive.
            // This is to prevent those offers from being dispatched.
            if (!buildingManager.BuildingHasFlags(buildingId, Building.Flags.Active))
            {
                return false;
            }

            var buildingService = buildingManager.GetBuildingService(buildingId);
            if (buildingService == ItemClass.Service.VarsitySports)
            {
                return false;
            }
            else if (buildingService == ItemClass.Service.Monument)
            {
                return buildingManager.IsRealUniqueBuilding(buildingId);
            }
            else
            {
                return true;
            }
        }

        /// <summary>Determines whether a building with specified ID is currently active.</summary>
        /// <param name="buildingId">The ID of the building to check.</param>
        /// <returns>
        ///   <c>true</c> if the building with specified ID is currently active; otherwise, <c>false</c>.
        /// </returns>
        public bool IsBuildingActive(ushort buildingId) => buildingManager.BuildingHasFlags(buildingId, Building.Flags.Active);

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

        /// <summary>Registers a trouble reaching the building with the specified ID.</summary>
        /// <param name="buildingId">The ID of the building where the citizen will not arrive as planned.</param>
        public void RegisterReachingTrouble(ushort buildingId)
        {
            ref byte trouble = ref reachingTroubles[buildingId];
            if (trouble < 255)
            {
                trouble = (byte)Math.Min(255, trouble + 10);
                buildingManager.UpdateBuildingColors(buildingId);
            }
        }

        /// <summary>Gets the reaching trouble factor for a building with specified ID.</summary>
        /// <param name="buildingId">The ID of the building to get the reaching trouble factor of.</param>
        /// <returns>A value in range 0 to 1 that describes how many troubles have citizens while trying to reach
        /// the building.</returns>
        public float GetBuildingReachingTroubleFactor(ushort buildingId) => reachingTroubles[buildingId] / 255f;

        private static int GetAllowedConstructingUpradingCount(int currentBuildingCount)
        {
            if (currentBuildingCount < ConstructionRestrictionThreshold1)
            {
                return ConstructionRestrictionStep1;
            }

            if (currentBuildingCount < ConstructionRestrictionThreshold2)
            {
                return ConstructionRestrictionStep1 + currentBuildingCount / ConstructionRestrictionScale2;
            }

            if (currentBuildingCount < ConstructionRestrictionThreshold3)
            {
                return ConstructionRestrictionStep2 + currentBuildingCount / ConstructionRestrictionScale3;
            }

            return MaximumBuildingsInConstruction;
        }

        private bool IsBuildingCompletedOrMissing(ushort buildingId)
            => buildingManager.BuildingHasFlags(buildingId, Building.Flags.Completed | Building.Flags.Deleted, includeZero: true);

        private void UpdateLightState()
        {
            if (lightStateCheckCounter > 0)
            {
                --lightStateCheckCounter;
                return;
            }

            ushort step = lightCheckStep;
            lightCheckStep = (ushort)((step + 1) & StepMask);
            lightStateCheckCounter = lightStateCheckFramesInterval;

            UpdateLightState(step, updateBuilding: true);
        }

        private void UpdateReachingTroubles(uint step)
        {
            ushort first = (ushort)(step * BuildingStepSize);
            ushort last = (ushort)((step + 1) * BuildingStepSize - 1);

            for (ushort i = first; i <= last; ++i)
            {
                ref byte trouble = ref reachingTroubles[i];
                if (trouble > 0)
                {
                    --trouble;
                    buildingManager.UpdateBuildingColors(i);
                }
            }
        }

        private void UpdateLightState(ushort step, bool updateBuilding)
        {
            ushort first = (ushort)(step * BuildingStepSize);
            ushort last = (ushort)((step + 1) * BuildingStepSize - 1);

            for (ushort i = first; i <= last; ++i)
            {
                if (!buildingManager.BuildingHasFlags(i, Building.Flags.Created))
                {
                    continue;
                }

                buildingManager.GetBuildingService(i, out var service, out var subService);
                bool lightsOn = !ShouldSwitchBuildingLightsOff(i, service, subService);
                if (lightsOn == lightStates[i])
                {
                    continue;
                }

                lightStates[i] = lightsOn;
                if (updateBuilding)
                {
                    buildingManager.UpdateBuildingColors(i);
                    if (!lightsOn && service != ItemClass.Service.Residential)
                    {
                        buildingManager.DeactivateVisually(i);
                    }
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
                    if (buildingManager.GetBuildingHeight(buildingId) > config.SwitchOffLightsMaxHeight)
                    {
                        return false;
                    }

                    float currentHour = timeInfo.CurrentHour;
                    return currentHour < Math.Min(config.WakeUpHour, EarliestWakeUp) || currentHour >= config.GoToSleepHour;

                case ItemClass.Service.Commercial when subService == ItemClass.SubService.CommercialLeisure:
                    return IsNoiseRestricted(buildingId);

                case ItemClass.Service.Office:
                case ItemClass.Service.Commercial:
                    if (buildingManager.GetBuildingHeight(buildingId) > config.SwitchOffLightsMaxHeight)
                    {
                        return false;
                    }

                    goto default;

                case ItemClass.Service.Monument:
                case ItemClass.Service.VarsitySports:
                case ItemClass.Service.Museums:
                case ItemClass.Service.ServicePoint:
                    return false;

                case ItemClass.Service.PlayerEducation:
                case ItemClass.Service.PlayerIndustry:
                    if (buildingManager.IsAreaMainBuilding(buildingId))
                    {
                        return false;
                    }
                    else if (buildingManager.IsAreaResidentalBuilding(buildingId))
                    {
                        return false;
                    }
                    else
                    {
                        goto default;
                    }

                case ItemClass.Service.Beautification when subService == ItemClass.SubService.BeautificationParks:
                    byte parkId = buildingManager.GetParkId(buildingId);
                    if (parkId == 0 || (buildingManager.GetParkPolicies(parkId) & DistrictPolicies.Park.NightTours) == 0)
                    {
                        goto default;
                    }

                    return false;

                default:
                    return !workBehavior.IsBuildingWorking(service, subService);
            }
        }
    }
}
