// <copyright file="RealTimeCore.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.Core
{
    using System;
    using System.Security.Permissions;
    using RealTime.Config;
    using RealTime.CustomAI;
    using RealTime.Events;
    using RealTime.Events.Storage;
    using RealTime.GameConnection;
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
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="config"/> is null.</exception>
        ///
        /// <param name="config">The configuration to run with.</param>
        /// <param name="rootPath">The path to the mod's assembly. Additinal files are stored here too.</param>
        ///
        /// <returns>A <see cref="RealTimeCore"/> instance that can be used to stop the mod.</returns>
        [PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust")]
        public static RealTimeCore Run(RealTimeConfig config, string rootPath)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            if (string.IsNullOrEmpty(rootPath))
            {
                throw new ArgumentException("The root path cannot be null or empty string", nameof(rootPath));
            }

            var timeAdjustment = new TimeAdjustment();
            DateTime gameDate = timeAdjustment.Enable();

            var timeInfo = new TimeInfo();
            var buildingManager = new BuildingManagerConnection();
            var simulationManager = new SimulationManagerConnection();

            var gameConnections = new GameConnections<Citizen>(
                timeInfo,
                new CitizenConnection(),
                new CitizenManagerConnection(),
                buildingManager,
                simulationManager,
                new TransferManagerConnection());

            var eventManager = new RealTimeEventManager(
                config,
                CityEventsLoader.Istance,
                new EventManagerConnection(),
                buildingManager,
                simulationManager,
                timeInfo);

            SetupCustomAI(timeInfo, config, gameConnections, eventManager);

            RealTimeEventSimulation.EventManager = eventManager;
            CityEventsLoader.Istance.ReloadEvents(rootPath);

            var customTimeBar = new CustomTimeBar();
            customTimeBar.Enable(gameDate);
            customTimeBar.CityEventClick += CustomTimeBarCityEventClick;

            var result = new RealTimeCore(timeAdjustment, customTimeBar, eventManager);
            eventManager.EventsChanged += result.CityEventsChanged;
            DaylightTimeSimulation.NewDay += result.CityEventsChanged;

            try
            {
                int redirectedCount = Redirector.PerformRedirections();
                Log.Info($"Successfully redirected {redirectedCount} methods.");
            }
            catch (Exception ex)
            {
                Log.Error("Failed to perform method redirections: " + ex.Message);
            }

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
            DaylightTimeSimulation.NewDay -= CityEventsChanged;

            CityEventsLoader.Istance.Clear();

            ResidentAIHook.RealTimeAI = null;
            TouristAIHook.RealTimeAI = null;
            PrivateBuildingAIHook.RealTimeAI = null;
            RealTimeEventSimulation.EventManager = null;

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
    }
}
