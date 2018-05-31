// <copyright file="RealTimeCore.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.Core
{
    using System;
    using RealTime.Simulation;
    using RealTime.UI;

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

        public static RealTimeCore Enable()
        {
            var timeAdjustment = new TimeAdjustment();
            DateTime gameDate = timeAdjustment.Enable();

            var customTimeBar = new CustomTimeBar();
            customTimeBar.Enable(gameDate);

            var core = new RealTimeCore(timeAdjustment, customTimeBar);
            core.isEnabled = true;
            return core;
        }

        public void Disable()
        {
            if (!isEnabled)
            {
                return;
            }

            timeAdjustment.Disable();
            timeBar.Disable();
            isEnabled = false;
        }
    }
}
