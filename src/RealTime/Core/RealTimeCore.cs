// <copyright file="RealTimeCore.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.Core
{
    using System;
    using System.Collections.Generic;
    using RealTime.Config;
    using RealTime.CustomAI;
    using RealTime.Events;
    using RealTime.Events.Storage;
    using RealTime.GameConnection;
    using RealTime.GameConnection.Patches;
    using RealTime.Localization;
    using RealTime.Patching;
    using RealTime.Simulation;
    using RealTime.Tools;
    using RealTime.UI;

    /// <summary>
    /// The core component of the Real Time mod. Activates and deactivates
    /// the different parts of the mod's logic.
    /// </summary>
    internal sealed class RealTimeCore
    {
        private readonly List<IStorageData> storageData = new List<IStorageData>();
        private readonly TimeAdjustment timeAdjustment;
        private readonly CustomTimeBar timeBar;
        private readonly RealTimeEventManager eventManager;
        private readonly MethodPatcher patcher;

        private bool isEnabled;

        private RealTimeCore(TimeAdjustment timeAdjustment, CustomTimeBar timeBar, RealTimeEventManager eventManager, MethodPatcher patcher)
        {
            this.timeAdjustment = timeAdjustment;
            this.timeBar = timeBar;
            this.eventManager = eventManager;
            this.patcher = patcher;
            isEnabled = true;
        }

        /// <summary>
        /// Runs the mod by activating its parts.
        /// </summary>
        ///
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="config"/> or <paramref name="localizationProvider"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="rootPath"/> is null or an empty string.</exception>
        ///
        /// <param name="config">The configuration to run with.</param>
        /// <param name="rootPath">The path to the mod's assembly. Additional files are stored here too.</param>
        /// <param name="localizationProvider">The <see cref="ILocalizationProvider"/> to use for text translation.</param>
        ///
        /// <returns>A <see cref="RealTimeCore"/> instance that can be used to stop the mod.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "This is the entry point and needs to instantiate all parts")]
        public static RealTimeCore Run(RealTimeConfig config, string rootPath, ILocalizationProvider localizationProvider)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            if (string.IsNullOrEmpty(rootPath))
            {
                throw new ArgumentException("The root path cannot be null or empty string", nameof(rootPath));
            }

            if (localizationProvider == null)
            {
                throw new ArgumentNullException(nameof(localizationProvider));
            }

            var patcher = new MethodPatcher(
                BuildingAIPatches.PrivateConstructionTime,
                BuildingAIPatches.PrivateHandleWorkers,
                BuildingAIPatches.CommercialSimulation,
                ResidentAIPatch.Location,
                ResidentAIPatch.ArriveAtDestination,
                TouristAIPatch.Location,
                UIGraphPatches.MinDataPoints,
                UIGraphPatches.VisibleEndTime,
                UIGraphPatches.BuildLabels,
                WeatherManagerPatch.SimulationStepImpl);

            try
            {
                patcher.Apply();
                Log.Info("The 'Real Time' successfully performed the methods redirections");
            }
            catch (Exception ex)
            {
                Log.Error("The 'Real Time' mod failed to perform method redirections: " + ex);
                patcher.Revert();
                return null;
            }

            var timeInfo = new TimeInfo(config);
            var buildingManager = new BuildingManagerConnection();
            var randomizer = new GameRandomizer();

            var weatherInfo = new WeatherInfo(new WeatherManagerConnection());

            var gameConnections = new GameConnections<Citizen>(
                timeInfo,
                new CitizenConnection(),
                new CitizenManagerConnection(),
                buildingManager,
                randomizer,
                new TransferManagerConnection(),
                weatherInfo);

            var eventManager = new RealTimeEventManager(
                config,
                CityEventsLoader.Instance,
                new EventManagerConnection(),
                buildingManager,
                randomizer,
                timeInfo);

            if (!SetupCustomAI(timeInfo, config, gameConnections, eventManager))
            {
                Log.Error("The 'Real Time' mod failed to setup the customized AI and will now be deactivated.");
                patcher.Revert();
                return null;
            }

            var timeAdjustment = new TimeAdjustment(config);
            DateTime gameDate = timeAdjustment.Enable();
            SimulationHandler.CitizenProcessor.SetFrameDuration(timeAdjustment.HoursPerFrame);

            CityEventsLoader.Instance.ReloadEvents(rootPath);

            var customTimeBar = new CustomTimeBar();
            customTimeBar.Enable(gameDate);
            customTimeBar.CityEventClick += CustomTimeBarCityEventClick;

            var result = new RealTimeCore(timeAdjustment, customTimeBar, eventManager, patcher);
            eventManager.EventsChanged += result.CityEventsChanged;
            SimulationHandler.NewDay += result.CityEventsChanged;

            SimulationHandler.TimeAdjustment = timeAdjustment;
            SimulationHandler.DayTimeSimulation = new DayTimeSimulation(config);
            SimulationHandler.EventManager = eventManager;
            SimulationHandler.WeatherInfo = weatherInfo;
            SimulationHandler.Buildings = BuildingAIPatches.RealTimeAI;

            AwakeSleepSimulation.Install(config);

            RealTimeStorage.CurrentLevelStorage.GameSaving += result.GameSaving;
            result.storageData.Add(eventManager);
            result.storageData.Add(ResidentAIPatch.RealTimeAI.GetStorageService());
            result.LoadStorageData();

            result.Translate(localizationProvider);

            return result;
        }

        /// <summary>
        /// Stops the mod by deactivating all its parts.
        /// </summary>
        public void Stop()
        {
            if (!isEnabled)
            {
                return;
            }

            patcher.Revert();

            timeAdjustment.Disable();
            timeBar.CityEventClick -= CustomTimeBarCityEventClick;
            timeBar.Disable();
            eventManager.EventsChanged -= CityEventsChanged;
            SimulationHandler.NewDay -= CityEventsChanged;

            CityEventsLoader.Instance.Clear();

            AwakeSleepSimulation.Uninstall();

            RealTimeStorage.CurrentLevelStorage.GameSaving -= GameSaving;

            ResidentAIPatch.RealTimeAI = null;
            TouristAIPatch.RealTimeAI = null;
            BuildingAIPatches.RealTimeAI = null;
            SimulationHandler.EventManager = null;
            SimulationHandler.DayTimeSimulation = null;
            SimulationHandler.TimeAdjustment = null;
            SimulationHandler.WeatherInfo = null;
            SimulationHandler.Buildings = null;
            SimulationHandler.CitizenProcessor = null;

            isEnabled = false;
        }

        /// <summary>
        /// Translates all the mod's component to a different language obtained from
        /// the specified <paramref name="localizationProvider"/>.
        /// </summary>
        ///
        /// <exception cref="ArgumentNullException">Thrown when the argument is null.</exception>
        ///
        /// <param name="localizationProvider">An instance of the <see cref="ILocalizationProvider"/> to use for translation.</param>
        public void Translate(ILocalizationProvider localizationProvider)
        {
            if (localizationProvider == null)
            {
                throw new ArgumentNullException(nameof(localizationProvider));
            }

            timeBar.Translate(localizationProvider.CurrentCulture);
            UIGraphPatches.Translate(localizationProvider.CurrentCulture);
        }

        private static bool SetupCustomAI(
            TimeInfo timeInfo,
            RealTimeConfig config,
            GameConnections<Citizen> gameConnections,
            RealTimeEventManager eventManager)
        {
            ResidentAIConnection<ResidentAI, Citizen> residentAIConnection = ResidentAIPatch.GetResidentAIConnection();
            if (residentAIConnection == null)
            {
                return false;
            }

            var spareTimeBehavior = new SpareTimeBehavior(config, timeInfo);

            var realTimeResidentAI = new RealTimeResidentAI<ResidentAI, Citizen>(
                config,
                gameConnections,
                residentAIConnection,
                eventManager,
                spareTimeBehavior);

            ResidentAIPatch.RealTimeAI = realTimeResidentAI;
            SimulationHandler.CitizenProcessor = new CitizenProcessor(realTimeResidentAI, spareTimeBehavior);

            TouristAIConnection<TouristAI, Citizen> touristAIConnection = TouristAIPatch.GetTouristAIConnection();
            if (touristAIConnection == null)
            {
                return false;
            }

            var realTimeTouristAI = new RealTimeTouristAI<TouristAI, Citizen>(
                config,
                gameConnections,
                touristAIConnection,
                eventManager,
                spareTimeBehavior);

            TouristAIPatch.RealTimeAI = realTimeTouristAI;

            var realTimePrivateBuildingAI = new RealTimePrivateBuildingAI(
                config,
                timeInfo,
                gameConnections.BuildingManager,
                new ToolManagerConnection());

            BuildingAIPatches.RealTimeAI = realTimePrivateBuildingAI;
            return true;
        }

        private static void CustomTimeBarCityEventClick(object sender, CustomTimeBarClickEventArgs e)
        {
            CameraHelper.NavigateToBuilding(e.CityEventBuildingId);
        }

        private void CityEventsChanged(object sender, EventArgs e)
        {
            timeBar.UpdateEventsDisplay(eventManager.CityEvents);
        }

        private void LoadStorageData()
        {
            foreach (IStorageData item in storageData)
            {
                RealTimeStorage.CurrentLevelStorage.Deserialize(item);
            }
        }

        private void GameSaving(object sender, EventArgs e)
        {
            var storage = (RealTimeStorage)sender;
            foreach (IStorageData item in storageData)
            {
                storage.Serialize(item);
            }
        }
    }
}
