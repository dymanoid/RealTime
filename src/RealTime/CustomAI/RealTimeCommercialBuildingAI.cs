// <copyright file="RealTimeCommercialBuildingAI.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.CustomAI
{
    using RealTime.GameConnection;
    using RealTime.Simulation;

    /// <summary>
    /// A class that incorporates the customized logic for the commercial buildings.
    /// </summary>
    internal sealed class RealTimeCommercialBuildingAI
    {
        private readonly ITimeInfo timeInfo;
        private readonly IBuildingManagerConnection buildingMgr;

        /// <summary>
        /// Initializes a new instance of the <see cref="RealTimeCommercialBuildingAI"/> class.
        /// </summary>
        ///
        /// <exception cref="System.ArgumentNullException">Thrown when any argument is null.</exception>
        ///
        /// <param name="timeInfo">A time information source.</param>
        /// <param name="buildingManager">A proxy object that provides a way to call the game-specific methods of the <see cref="global::BuildingManager"/> class.</param>
        public RealTimeCommercialBuildingAI(ITimeInfo timeInfo, IBuildingManagerConnection buildingManager)
        {
            this.timeInfo = timeInfo ?? throw new System.ArgumentNullException(nameof(timeInfo));
            buildingMgr = buildingManager ?? throw new System.ArgumentNullException(nameof(buildingManager));
        }

        /// <summary>
        /// Performs the custom processing using the specified simulation frame ID.
        /// </summary>
        ///
        /// <param name="frameId">The simulation frame number to process.</param>
        public void Process(uint frameId)
        {
            frameId &= 0xFF;

            uint buildingFrom = frameId * 192u;
            uint buildingTo = ((frameId + 1u) * 192u) - 1u;

            // We have only few customers at night - that's an intended behavior.
            // To avoid commercial buildings from collapsing due to lack of customers,
            // we force the problem timer to pause at night time.
            if (timeInfo.IsNightTime)
            {
                buildingMgr.DecrementOutgoingProblemTimer((ushort)buildingFrom, (ushort)buildingTo, ItemClass.Service.Commercial);
            }
        }
    }
}
