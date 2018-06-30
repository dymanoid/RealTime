// <copyright file="RealTimePrivateBuildingAI.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.CustomAI
{
    using RealTime.Config;
    using RealTime.GameConnection;
    using RealTime.Simulation;

    /// <summary>
    /// A class that incorporated the custom logic for the private buildings.
    /// </summary>
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

        /// <summary>
        /// Initializes a new instance of the <see cref="RealTimePrivateBuildingAI"/> class.
        /// </summary>
        ///
        /// <param name="config">The configuration to run with.</param>
        /// <param name="timeInfo">The time information source.</param>
        /// <param name="toolManager">A proxy object that provides a way to call the game-specific methods of the <see cref="global::ToolManager"/> class.</param>
        ///
        /// <exception cref="System.ArgumentNullException">Thrown when any argument is null.</exception>
        public RealTimePrivateBuildingAI(RealTimeConfig config, ITimeInfo timeInfo, IToolManagerConnection toolManager)
        {
            this.config = config ?? throw new System.ArgumentNullException(nameof(config));
            this.timeInfo = timeInfo ?? throw new System.ArgumentNullException(nameof(timeInfo));
            this.toolManager = toolManager ?? throw new System.ArgumentNullException(nameof(toolManager));
        }

        /// <summary>
        /// Gets the building construction time taking into account the current day time.
        /// </summary>
        ///
        /// <returns>The building construction time in game-specific units (0..10880)</returns>
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

            // This causes the construction to not advance in the night time
            return timeInfo.IsNightTime && config.StopConstructionAtNight
                ? ConstructionSpeedPaused
                : (int)(ConstructionSpeedMinimum * value);
#endif
        }
    }
}
