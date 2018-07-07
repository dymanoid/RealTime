// <copyright file="RealTimeBuildingAI.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.CustomAI
{
    using RealTime.GameConnection;
    using RealTime.Simulation;
    using static Constants;

    /// <summary>
    /// A class that incorporates the customized logic for the buildings.
    /// </summary>
    internal sealed class RealTimeBuildingAI
    {
        private readonly ITimeInfo timeInfo;
        private readonly IBuildingManagerConnection buildingMgr;
        private int lastProcessedMinute = -1;
        private bool minuteProcessed;

        /// <summary>
        /// Initializes a new instance of the <see cref="RealTimeBuildingAI"/> class.
        /// </summary>
        ///
        /// <exception cref="System.ArgumentNullException">Thrown when any argument is null.</exception>
        ///
        /// <param name="timeInfo">A time information source.</param>
        /// <param name="buildingManager">A proxy object that provides a way to call the game-specific methods of the <see cref="global::BuildingManager"/> class.</param>
        public RealTimeBuildingAI(ITimeInfo timeInfo, IBuildingManagerConnection buildingManager)
        {
            this.timeInfo = timeInfo ?? throw new System.ArgumentNullException(nameof(timeInfo));
            buildingMgr = buildingManager ?? throw new System.ArgumentNullException(nameof(buildingManager));
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
                buildingMgr.SetOutgoingProblemTimer(buildingId, oldValue);
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
                buildingMgr.SetWorkersProblemTimer(buildingId, oldValue);
            }
        }

        /// <summary>Notifies this simulation object that a new simulation frame is started.
        /// The buildings will be processed again from the beginning of the list.</summary>
        public void StartBuildingProcessingFrame()
        {
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
