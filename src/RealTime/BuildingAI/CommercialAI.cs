// <copyright file="CommercialAI.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.BuildingAI
{
    using RealTime.GameConnection;
    using RealTime.Simulation;

    internal sealed class CommercialAI
    {
        private readonly ITimeInfo timeInfo;

        public CommercialAI(ITimeInfo timeInfo, IBuildingManagerConnection buildingManager)
        {
            this.timeInfo = timeInfo ?? throw new System.ArgumentNullException(nameof(timeInfo));
            BuildingMgr = buildingManager ?? throw new System.ArgumentNullException(nameof(buildingManager));
        }

        public IBuildingManagerConnection BuildingMgr { get; }

        public void Process(uint frameId)
        {
            frameId &= 0xFF;

            uint buildingFrom = frameId * 192u;
            uint buildingTo = ((frameId + 1u) * 192u) - 1u;

            // We have only few customers at night - that's an inteded behavior.
            // To avoid commercial building from collapsing due to lack of customers,
            // we force the problem timer to pause at night time.
            if (timeInfo.IsNightTime)
            {
                BuildingMgr.DecrementOutgoingProblemTimer((ushort)buildingFrom, (ushort)buildingTo, ItemClass.Service.Commercial);
            }
        }
    }
}
