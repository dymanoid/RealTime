// <copyright file="RealTimePrivateBuildingAI.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.CustomAI
{
    using RealTime.Config;
    using RealTime.GameConnection;
    using RealTime.Simulation;

    internal sealed class RealTimePrivateBuildingAI
    {
        private const int ConstructionSpeedPaused = 10880;
        private const int ConstructionSpeedMinimum = 1088;

        private readonly RealTimeConfig config;
        private readonly ITimeInfo timeInfo;
        private readonly IToolManagerConnection toolManager;

#if !DEBUG
        private uint lastConfigValue;
        private double value;
#endif

        public RealTimePrivateBuildingAI(RealTimeConfig config, ITimeInfo timeInfo, IToolManagerConnection toolManager)
        {
            this.config = config ?? throw new System.ArgumentNullException(nameof(config));
            this.timeInfo = timeInfo ?? throw new System.ArgumentNullException(nameof(timeInfo));
            this.toolManager = toolManager ?? throw new System.ArgumentNullException(nameof(toolManager));
        }

        public int GetConstructionTime()
        {
            if ((toolManager.GetCurrentMode() & ItemClass.Availability.AssetEditor) != 0)
            {
                return 0;
            }

#if DEBUG
            return 0;
#else
            if (config.ConstructionSpeed != lastConfigValue)
            {
                lastConfigValue = config.ConstructionSpeed;
                double inverted = 101d - lastConfigValue;
                value = inverted * inverted * inverted / 1_000_000d;
            }

            // This causes the constuction to not advance in the night time
            return timeInfo.IsNightTime && config.StopConstructionAtNight
                ? ConstructionSpeedPaused
                : (int)(ConstructionSpeedMinimum * value);
#endif
        }
    }
}
