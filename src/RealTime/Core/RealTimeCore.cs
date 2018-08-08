// <copyright file="RealTimeCore.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.Core
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using RealTime.Config;
    using RealTime.CustomAI;
    using RealTime.Events;
    using RealTime.Events.Storage;
    using RealTime.GameConnection;
    using RealTime.GameConnection.Patches;
    using RealTime.Simulation;
    using RealTime.UI;
    using SkyTools.Configuration;
    using SkyTools.GameTools;
    using SkyTools.Localization;
    using SkyTools.Patching;
    using SkyTools.Storage;
    using SkyTools.Tools;

    /// <summary>
    /// The core component of the Real Time mod. Activates and deactivates
    /// the different parts of the mod's logic.
    /// </summary>
    internal sealed class RealTimeCore
    {
        private const string HarmonyId = "com.cities_skylines.dymanoid.realtime";

        private readonly List<IStorageData> storageData = new List<IStorageData>();
        private readonly TimeAdjustment timeAdjustment;
        private readonly CustomTimeBar timeBar;
        private readonly RealTimeEventManager eventManager;
        private readonly MethodPatcher patcher;
        private readonly VanillaEvents vanillaEvents;

        private bool isEnabled;

        private RealTimeCore(
            TimeAdjustment timeAdjustment,
            CustomTimeBar timeBar,
            RealTimeEventManager eventManager,
            MethodPatcher patcher,
            VanillaEvents vanillaEvents)
        {
            this.timeAdjustment = timeAdjustment;
            this.timeBar = timeBar;
            this.eventManager = eventManager;
            this.patcher = patcher;
            this.vanillaEvents = vanillaEvents;
            isEnabled = true;
        }

        /// <summary>Gets a value indicating whether the mod is running in a restricted mode due to method patch failures.</summary>
        public bool IsRestrictedMode { get; private set; }

        /// <summary>
        /// Runs the mod by activating its parts.
        /// </summary>
        ///
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="configProvider"/> or <paramref name="localizationProvider"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="rootPath"/> is null or an empty string.</exception>
        ///
        /// <param name="configProvider">The configuration provider that provides the mod's configuration.</param>
        /// <param name="rootPath">The path to the mod's assembly. Additional files are stored here too.</param>
        /// <param name="localizationProvider">The <see cref="ILocalizationProvider"/> to use for text translation.</param>
        /// <param name="setDefaultTime"><c>true</c> to initialize the game time to a default value (real world date and city wake up hour);
        /// <c>false</c> to leave the game time unchanged.</param>
        ///
        /// <returns>A <see cref="RealTimeCore"/> instance that can be used to stop the mod.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "This is the entry point and needs to instantiate all parts")]
        public static RealTimeCore Run(
            ConfigurationProvider<RealTimeConfig> configProvider,
            string rootPath,
            ILocalizationProvider localizationProvider,
            bool setDefaultTime)
        {
            if (configProvider == null)
            {
                throw new ArgumentNullException(nameof(configProvider));
            }

            if (string.IsNullOrEmpty(rootPath))
            {
                throw new ArgumentException("The root path cannot be null or empty string", nameof(rootPath));
            }

            if (localizationProvider == null)
            {
                throw new ArgumentNullException(nameof(localizationProvider));
            }

            List<IPatch> patches = GetMethodPatches();
            var patcher = new MethodPatcher(HarmonyId, patches);

            HashSet<IPatch> appliedPatches = patcher.Apply();
            if (!CheckRequiredMethodPatches(appliedPatches))
            {
                Log.Error("The 'Real Time' mod failed to perform method redirections for required methods");
                patcher.Revert();
                return null;
            }

            if (StorageBase.CurrentLevelStorage != null)
            {
                LoadStorageData(new[] { configProvider }, StorageBase.CurrentLevelStorage);
            }

            var timeInfo = new TimeInfo(configProvider.Configuration);
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
                configProvider.Configuration,
                CityEventsLoader.Instance,
                new EventManagerConnection(),
                buildingManager,
                randomizer,
                timeInfo);

            if (!SetupCustomAI(timeInfo, configProvider.Configuration, gameConnections, eventManager))
            {
                Log.Error("The 'Real Time' mod failed to setup the customized AI and will now be deactivated.");
                patcher.Revert();
                return null;
            }

            var timeAdjustment = new TimeAdjustment(configProvider.Configuration);
            DateTime gameDate = timeAdjustment.Enable(setDefaultTime);
            SimulationHandler.CitizenProcessor.UpdateFrameDuration();

            CityEventsLoader.Instance.ReloadEvents(rootPath);

            var customTimeBar = new CustomTimeBar();
            customTimeBar.Enable(gameDate);
            customTimeBar.CityEventClick += CustomTimeBarCityEventClick;

            var vanillaEvents = VanillaEvents.Customize();

            var result = new RealTimeCore(timeAdjustment, customTimeBar, eventManager, patcher, vanillaEvents);
            eventManager.EventsChanged += result.CityEventsChanged;

            var statistics = new Statistics(timeInfo, localizationProvider);
            if (statistics.Initialize())
            {
                statistics.RefreshUnits();
            }
            else
            {
                statistics = null;
            }

            SimulationHandler.NewDay += result.CityEventsChanged;

            SimulationHandler.TimeAdjustment = timeAdjustment;
            SimulationHandler.DayTimeSimulation = new DayTimeSimulation(configProvider.Configuration);
            SimulationHandler.EventManager = eventManager;
            SimulationHandler.WeatherInfo = weatherInfo;
            SimulationHandler.Buildings = BuildingAIPatches.RealTimeAI;
            SimulationHandler.Buildings.UpdateFrameDuration();

            if (appliedPatches.Contains(BuildingAIPatches.PrivateShowConsumption))
            {
                SimulationHandler.Buildings.InitializeLightState();
            }

            SimulationHandler.Statistics = statistics;

            if (appliedPatches.Contains(WorldInfoPanelPatches.UpdateBindings))
            {
                WorldInfoPanelPatches.CitizenInfoPanel = CustomCitizenInfoPanel.Enable(ResidentAIPatch.RealTimeAI, localizationProvider);
                WorldInfoPanelPatches.VehicleInfoPanel = CustomVehicleInfoPanel.Enable(ResidentAIPatch.RealTimeAI, localizationProvider);
            }

            AwakeSleepSimulation.Install(configProvider.Configuration);

            result.storageData.Add(eventManager);
            result.storageData.Add(ResidentAIPatch.RealTimeAI.GetStorageService());
            if (StorageBase.CurrentLevelStorage != null)
            {
                StorageBase.CurrentLevelStorage.GameSaving += result.GameSaving;
                LoadStorageData(result.storageData, StorageBase.CurrentLevelStorage);
            }

            result.storageData.Add(configProvider);
            result.Translate(localizationProvider);
            result.IsRestrictedMode = appliedPatches.Count != patches.Count;

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

            Log.Info($"The 'Real Time' mod reverts method patches.");
            patcher.Revert();

            vanillaEvents.Revert();

            timeAdjustment.Disable();
            timeBar.CityEventClick -= CustomTimeBarCityEventClick;
            timeBar.Disable();
            eventManager.EventsChanged -= CityEventsChanged;
            SimulationHandler.NewDay -= CityEventsChanged;

            CityEventsLoader.Instance.Clear();

            AwakeSleepSimulation.Uninstall();

            StorageBase.CurrentLevelStorage.GameSaving -= GameSaving;

            ResidentAIPatch.RealTimeAI = null;
            TouristAIPatch.RealTimeAI = null;
            BuildingAIPatches.RealTimeAI = null;
            TransferManagerPatch.RealTimeAI = null;
            SimulationHandler.EventManager = null;
            SimulationHandler.DayTimeSimulation = null;
            SimulationHandler.TimeAdjustment = null;
            SimulationHandler.WeatherInfo = null;
            SimulationHandler.Buildings = null;
            SimulationHandler.CitizenProcessor = null;
            SimulationHandler.Statistics?.Close();
            SimulationHandler.Statistics = null;

            WorldInfoPanelPatches.CitizenInfoPanel?.Disable();
            WorldInfoPanelPatches.CitizenInfoPanel = null;

            WorldInfoPanelPatches.VehicleInfoPanel?.Disable();
            WorldInfoPanelPatches.VehicleInfoPanel = null;

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

        private static List<IPatch> GetMethodPatches()
        {
            var patches = new List<IPatch>
            {
                BuildingAIPatches.GetConstructionTime,
                BuildingAIPatches.HandleWorkers,
                BuildingAIPatches.CommercialSimulation,
                BuildingAIPatches.PrivateShowConsumption,
                BuildingAIPatches.PlayerShowConsumption,
                ResidentAIPatch.Location,
                ResidentAIPatch.ArriveAtTarget,
                TouristAIPatch.Location,
                TransferManagerPatch.AddOutgoingOffer,
                WorldInfoPanelPatches.UpdateBindings,
                UIGraphPatches.MinDataPoints,
                UIGraphPatches.VisibleEndTime,
                UIGraphPatches.BuildLabels,
                WeatherManagerPatch.SimulationStepImpl
            };

            if (Compatibility.IsModActive(Compatibility.CitizenLifecycleRebalanceId))
            {
                Log.Info("The 'Real Time' mod will not change the citizens aging because the 'Citizen Lifecycle Rebalance' mod is active.");
            }
            else
            {
                patches.Add(ResidentAIPatch.UpdateAge);
                patches.Add(ResidentAIPatch.CanMakeBabies);
            }

            return patches;
        }

        private static bool CheckRequiredMethodPatches(HashSet<IPatch> appliedPatches)
        {
            IPatch[] requiredPatches =
            {
                BuildingAIPatches.HandleWorkers,
                BuildingAIPatches.CommercialSimulation,
                ResidentAIPatch.Location,
                ResidentAIPatch.ArriveAtTarget,
                TouristAIPatch.Location,
                TransferManagerPatch.AddOutgoingOffer
            };

            return requiredPatches.All(appliedPatches.Contains);
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
            var travelBehavior = new TravelBehavior(gameConnections.BuildingManager);
            var workBehavior = new WorkBehavior(config, gameConnections.Random, gameConnections.BuildingManager, timeInfo, travelBehavior);

            var realTimePrivateBuildingAI = new RealTimeBuildingAI(
                config,
                timeInfo,
                gameConnections.BuildingManager,
                new ToolManagerConnection(),
                workBehavior,
                travelBehavior);

            BuildingAIPatches.RealTimeAI = realTimePrivateBuildingAI;
            TransferManagerPatch.RealTimeAI = realTimePrivateBuildingAI;

            var realTimeResidentAI = new RealTimeResidentAI<ResidentAI, Citizen>(
                config,
                gameConnections,
                residentAIConnection,
                eventManager,
                realTimePrivateBuildingAI,
                workBehavior,
                spareTimeBehavior,
                travelBehavior);

            ResidentAIPatch.RealTimeAI = realTimeResidentAI;
            SimulationHandler.CitizenProcessor = new CitizenProcessor<ResidentAI, Citizen>(realTimeResidentAI, spareTimeBehavior, timeInfo);

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
            return true;
        }

        private static void CustomTimeBarCityEventClick(object sender, CustomTimeBarClickEventArgs e)
        {
            CameraHelper.NavigateToBuilding(e.CityEventBuildingId, true);
        }

        private static void LoadStorageData(IEnumerable<IStorageData> storageData, StorageBase storage)
        {
            foreach (IStorageData item in storageData)
            {
                storage.Deserialize(item);
                Log.Debug(LogCategory.Generic, "The 'Real Time' mod loaded its data from container " + item.StorageDataId);
            }
        }

        private void CityEventsChanged(object sender, EventArgs e)
        {
            timeBar.UpdateEventsDisplay(eventManager.CityEvents);
        }

        private void GameSaving(object sender, EventArgs e)
        {
            var storage = (StorageBase)sender;
            foreach (IStorageData item in storageData)
            {
                storage.Serialize(item);
                Log.Debug(LogCategory.Generic, "The 'Real Time' mod stored its data in the current game for container " + item.StorageDataId);
            }
        }
    }
}
