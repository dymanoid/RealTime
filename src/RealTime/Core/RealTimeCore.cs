// <copyright file="RealTimeCore.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.Core
{
    using System;
    using System.Collections.Generic;
    using System.Security.Permissions;
    using RealTime.Config;
    using RealTime.CustomAI;
    using RealTime.Events;
    using RealTime.Events.Storage;
    using RealTime.GameConnection;
    using RealTime.Localization;
    using RealTime.Simulation;
    using RealTime.Tools;
    using RealTime.UI;
    using Redirection;

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

        private bool isEnabled;

        private RealTimeCore(TimeAdjustment timeAdjustment, CustomTimeBar timeBar, RealTimeEventManager eventManager)
        {
            this.timeAdjustment = timeAdjustment;
            this.timeBar = timeBar;
            this.eventManager = eventManager;
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
        /// <param name="localizationProvider">The <see cref="LocalizationProvider"/> to use for text translation.</param>
        ///
        /// <returns>A <see cref="RealTimeCore"/> instance that can be used to stop the mod.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "This is the entry point and needs to instantiate all parts")]
        [PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust")]
        public static RealTimeCore Run(RealTimeConfig config, string rootPath, LocalizationProvider localizationProvider)
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

            try
            {
                int redirectedCount = Redirector.PerformRedirections();
                Log.Info($"Successfully redirected {redirectedCount} methods.");
            }
            catch (Exception ex)
            {
                Log.Error("Failed to perform method redirections: " + ex.Message);
                return null;
            }

            var timeAdjustment = new TimeAdjustment(config);
            DateTime gameDate = timeAdjustment.Enable();
            SimulationHandler.TimeAdjustment = timeAdjustment;

            var timeInfo = new TimeInfo();
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

            SetupCustomAI(timeInfo, config, gameConnections, eventManager);

            CityEventsLoader.Instance.ReloadEvents(rootPath);

            var customTimeBar = new CustomTimeBar();
            customTimeBar.Enable(gameDate);
            customTimeBar.CityEventClick += CustomTimeBarCityEventClick;

            var result = new RealTimeCore(timeAdjustment, customTimeBar, eventManager);
            eventManager.EventsChanged += result.CityEventsChanged;
            SimulationHandler.NewDay += result.CityEventsChanged;

            SimulationHandler.DayTimeSimulation = new DayTimeSimulation(config);
            SimulationHandler.EventManager = eventManager;
            SimulationHandler.CommercialAI = new RealTimeCommercialBuildingAI(timeInfo, buildingManager);
            SimulationHandler.WeatherInfo = weatherInfo;

            RealTimeStorage.CurrentLevelStorage.GameSaving += result.GameSaving;
            result.storageData.Add(eventManager);
            result.LoadStorageData();

            result.Translate(localizationProvider);

            return result;
        }

        /// <summary>
        /// Stops the mod by deactivating all its parts.
        /// </summary>
        [PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust")]
        public void Stop()
        {
            if (!isEnabled)
            {
                return;
            }

            timeAdjustment.Disable();
            timeBar.CityEventClick -= CustomTimeBarCityEventClick;
            timeBar.Disable();
            eventManager.EventsChanged -= CityEventsChanged;
            SimulationHandler.NewDay -= CityEventsChanged;

            CityEventsLoader.Instance.Clear();

            RealTimeStorage.CurrentLevelStorage.GameSaving -= GameSaving;

            ResidentAIHook.RealTimeAI = null;
            TouristAIHook.RealTimeAI = null;
            PrivateBuildingAIHook.RealTimeAI = null;
            SimulationHandler.EventManager = null;
            SimulationHandler.DayTimeSimulation = null;
            SimulationHandler.CommercialAI = null;
            SimulationHandler.TimeAdjustment = null;
            SimulationHandler.WeatherInfo = null;

            try
            {
                Redirector.RevertRedirections();
                Log.Info($"Successfully reverted all method redirections.");
            }
            catch (Exception ex)
            {
                Log.Error("Failed to revert method redirections: " + ex.Message);
            }

            isEnabled = false;
        }

        /// <summary>
        /// Translates all the mod's component to a different language obtained from
        /// the provided <paramref name="localizationProvider"/>.
        /// </summary>
        ///
        /// <exception cref="ArgumentNullException">Thrown when the argument is null.</exception>
        ///
        /// <param name="localizationProvider">An instance of the <see cref="LocalizationProvider"/> to use for translation.</param>
        public void Translate(LocalizationProvider localizationProvider)
        {
            if (localizationProvider == null)
            {
                throw new ArgumentNullException(nameof(localizationProvider));
            }

            timeBar.Translate(localizationProvider.CurrentCulture);
        }

        private static void SetupCustomAI(
            TimeInfo timeInfo,
            RealTimeConfig config,
            GameConnections<Citizen> gameConnections,
            RealTimeEventManager eventManager)
        {
            var realTimeResidentAI = new RealTimeResidentAI<ResidentAI, Citizen>(
                config,
                gameConnections,
                ResidentAIHook.GetResidentAIConnection(),
                eventManager);

            ResidentAIHook.RealTimeAI = realTimeResidentAI;

            var realTimeTouristAI = new RealTimeTouristAI<TouristAI, Citizen>(
                config,
                gameConnections,
                TouristAIHook.GetTouristAIConnection(),
                eventManager);

            TouristAIHook.RealTimeAI = realTimeTouristAI;

            var realTimePrivateBuildingAI = new RealTimePrivateBuildingAI(
                config,
                timeInfo,
                new ToolManagerConnection());

            PrivateBuildingAIHook.RealTimeAI = realTimePrivateBuildingAI;
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
