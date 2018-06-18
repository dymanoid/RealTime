// <copyright file="RealTimeCore.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.Core
{
    using System;
    using System.Security.Permissions;
    using RealTime.Config;
    using RealTime.CustomResidentAI;
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

        private bool isEnabled;

        private RealTimeCore(TimeAdjustment timeAdjustment, CustomTimeBar timeBar)
        {
            this.timeAdjustment = timeAdjustment;
            this.timeBar = timeBar;
            isEnabled = true;
        }

        public static void TestRedirections()
        {
            int count = Redirector.PerformRedirections();
            System.Diagnostics.Trace.WriteLine("Redirection count = " + count);
        }

        /// <summary>
        /// Runs the mod by activating its parts.
        /// </summary>
        ///
        /// <returns>A <see cref="RealTimeCore"/> instance that can be used to stop the mod.</returns>
        [PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust")]
        public static RealTimeCore Run()
        {
            var timeAdjustment = new TimeAdjustment();
            DateTime gameDate = timeAdjustment.Enable();

            var customTimeBar = new CustomTimeBar();
            customTimeBar.Enable(gameDate);

            var gameConnections = new GameConnections<ResidentAI>(
                ResidentAIHook.GetResidentAIConnection(),
                new CitizenManagerConnection(),
                new BuildingManagerConnection(),
                new EventManagerConnection());

            var realTimeResidentAI = new RealTimeResidentAI<ResidentAI>(
                new Configuration(),
                gameConnections,
                new TimeInfo(),
                ref SimulationManager.instance.m_randomizer);

            ResidentAIHook.RealTimeAI = realTimeResidentAI;

            try
            {
                int redirectedCount = Redirector.PerformRedirections();
                Log.Info($"Successfully redirected {redirectedCount} methods.");
            }
            catch (Exception ex)
            {
                Log.Error("Failed to perform method redirections: " + ex.Message);
            }

            return new RealTimeCore(timeAdjustment, customTimeBar);
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
            timeBar.Disable();
            ResidentAIHook.RealTimeAI = null;

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
    }
}
