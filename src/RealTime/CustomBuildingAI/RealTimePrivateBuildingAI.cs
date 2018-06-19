// <copyright file="RealTimePrivateBuildingAI.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.CustomAI
{
    using RealTime.GameConnection;
    using RealTime.Simulation;

    internal sealed class RealTimePrivateBuildingAI
    {
        private readonly ITimeInfo timeInfo;
        private readonly IToolManagerConnection toolManager;

        public RealTimePrivateBuildingAI(ITimeInfo timeInfo, IToolManagerConnection toolManager)
        {
            this.timeInfo = timeInfo ?? throw new System.ArgumentNullException(nameof(timeInfo));
            this.toolManager = toolManager ?? throw new System.ArgumentNullException(nameof(toolManager));
        }

        public int GetConstructionTime()
        {
            if ((toolManager.GetCurrentMode() & ItemClass.Availability.AssetEditor) != 0)
            {
                return 0;
            }

            // TODO: optionally disable construction on weekends
            // This causes the constuction to not advance in the night time
            // TODO: uncomment to enable the functionality (currently disabled for testing)
            ////return timeInfo.IsNightTime ? 10880 : 1088;
            return 0;
        }
    }
}
