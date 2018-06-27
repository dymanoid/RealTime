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

            // La nuit, nous n'avons que peu de clients - c'est un comportement intentionné.
            // Pour éviter que le bâtiment ne s'effondre en raison du manque de clients,
            // nous forçons le problème en faisant une pause pendant la nuit du temporisateur.
            if (timeInfo.IsNightTime)
            {
                BuildingMgr.DecrementOutgoingProblemTimer((ushort)buildingFrom, (ushort)buildingTo, ItemClass.Service.Commercial);
            }
        }
    }
}
