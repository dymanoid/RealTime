// <copyright file="RealTimePrivateBuildingAI.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.CustomAI
{
    using RealTime.Config;
    using RealTime.GameConnection;
    using RealTime.Simulation;
    using static Constants;

    /// <summary>
    /// A class that incorporated the custom logic for the private buildings.
    /// </summary>
    internal sealed class RealTimePrivateBuildingAI
    {
        private const int ConstructionSpeedPaused = 10880;
        private const int ConstructionSpeedMinimum = 1088;
        private const int StartFrameMask = 0xFF;

        private readonly RealTimeConfig config;
        private readonly ITimeInfo timeInfo;
        private readonly IToolManagerConnection toolManager;
        private readonly IBuildingManagerConnection buildingManager;

        private int lastProcessedMinute = -1;
        private bool minuteProcessed;

        private uint lastConfigConstructionSpeedValue;
        private double constructionSpeedValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="RealTimePrivateBuildingAI"/> class.
        /// </summary>
        ///
        /// <param name="config">The configuration to run with.</param>
        /// <param name="timeInfo">The time information source.</param>
        /// <param name="buildingManager">A proxy object that provides a way to call the game-specific methods of the <see cref="global::BuildingManager"/> class.</param>
        /// <param name="toolManager">A proxy object that provides a way to call the game-specific methods of the <see cref="global::ToolManager"/> class.</param>
        ///
        /// <exception cref="System.ArgumentNullException">Thrown when any argument is null.</exception>
        public RealTimePrivateBuildingAI(
            RealTimeConfig config,
            ITimeInfo timeInfo,
            IBuildingManagerConnection buildingManager,
            IToolManagerConnection toolManager)
        {
            this.config = config ?? throw new System.ArgumentNullException(nameof(config));
            this.timeInfo = timeInfo ?? throw new System.ArgumentNullException(nameof(timeInfo));
            this.buildingManager = buildingManager ?? throw new System.ArgumentNullException(nameof(buildingManager));
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

            if (config.ConstructionSpeed != lastConfigConstructionSpeedValue)
            {
                lastConfigConstructionSpeedValue = config.ConstructionSpeed;
                double inverted = 101d - lastConfigConstructionSpeedValue;
                constructionSpeedValue = inverted * inverted * inverted / 1_000_000d;
            }

            // This causes the construction to not advance in the night time
            return timeInfo.IsNightTime && config.StopConstructionAtNight
                ? ConstructionSpeedPaused
                : (int)(ConstructionSpeedMinimum * constructionSpeedValue);
        }

        /// <summary>
        /// Performs the custom processing of the outgoing problem timer.
        /// </summary>
        /// <param name="buildingId">The ID of the building to process.</param>
        /// <param name="oldValue">The old value of the outgoing problem timer.</param>
        /// <param name="newValue">The new value of the outgoing problem timer.</param>
        public void ProcessOutgoingProblems(ushort buildingId, byte oldValue, byte newValue)
        {
            // We have only few customers at night - that's an intended behavior.
            // To avoid commercial buildings from collapsing due to lack of customers,
            // we force the problem timer to pause at night time.
            if (timeInfo.IsNightTime || timeInfo.Now.Minute % ProblemTimersInterval != 0 || minuteProcessed)
            {
                buildingManager.SetOutgoingProblemTimer(buildingId, oldValue);
            }
        }

        /// <summary>
        /// Performs the custom processing of the worker problem timer.
        /// </summary>
        /// <param name="buildingId">The ID of the building to process.</param>
        /// <param name="oldValue">The old value of the worker problem timer.</param>
        /// <param name="newValue">The new value of the worker problem timer.</param>
        public void ProcessWorkerProblems(ushort buildingId, byte oldValue, byte newValue)
        {
            if (timeInfo.IsNightTime || timeInfo.Now.Minute % ProblemTimersInterval != 0 || minuteProcessed)
            {
                buildingManager.SetWorkersProblemTimer(buildingId, oldValue);
            }
        }

        /// <summary>Notifies this simulation object that a new simulation frame is started.
        /// The buildings will be processed again from the beginning of the list.</summary>
        /// <param name="frameIndex">The simulation frame index to process.</param>
        public void ProcessFrame(uint frameIndex)
        {
            if ((frameIndex & StartFrameMask) != 0)
            {
                return;
            }

            int currentMinute = timeInfo.Now.Minute;
            if (lastProcessedMinute != currentMinute)
            {
                lastProcessedMinute = currentMinute;
                minuteProcessed = false;
            }
            else
            {
                minuteProcessed = true;
            }
        }
    }
}
