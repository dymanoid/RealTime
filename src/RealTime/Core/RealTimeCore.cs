// <copyright file="RealTimeCore.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.Core
{
    using System;
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
        }

        /// <summary>
        /// Runs the mod by activating its parts.
        /// </summary>
        ///
        /// <returns>A <see cref="RealTimeCore"/> instance that can be used to stop the mod.</returns>
        public static RealTimeCore Run()
        {
            var timeAdjustment = new TimeAdjustment();
            DateTime gameDate = timeAdjustment.Enable();

            var customTimeBar = new CustomTimeBar();
            customTimeBar.Enable(gameDate);

            try
            {
                Redirector.PerformRedirections();
            }
            catch (Exception ex)
            {
                Log.Error("Failed to perform method redirections: " + ex.Message);
            }

            var core = new RealTimeCore(timeAdjustment, customTimeBar)
            {
                isEnabled = true
            };

            return core;
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

            timeAdjustment.Disable();
            timeBar.Disable();

            try
            {
                Redirector.RevertRedirections();
            }
            catch (Exception ex)
            {
                Log.Error("Failed to revert method redirections: " + ex.Message);
            }

            isEnabled = false;
        }
    }
}
