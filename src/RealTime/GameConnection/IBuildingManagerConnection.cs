// <copyright file="IBuildingManagerConnection.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.GameConnection
{
    using System.Collections.Generic;

    internal interface IBuildingManagerConnection
    {
        ItemClass.Service GetBuildingService(ushort buildingId);

        ItemClass.SubService GetBuildingSubService(ushort buildingId);

        Building.Flags GetBuildingFlags(ushort buildingId);

        float GetDistanceBetweenBuildings(ushort building1, ushort building2);

        void ModifyMaterialBuffer(ushort buildingId, TransferManager.TransferReason reason, int delta);

        ushort FindActiveBuilding(
            ushort searchAreaCenterBuilding,
            float maxDistance,
            ItemClass.Service service,
            ItemClass.SubService subService = ItemClass.SubService.None);

        ushort GetEvent(ushort buildingId);

        ushort GetRandomBuilding(IEnumerable<ItemClass.Service> services);

        string GetBuildingClassName(ushort buildingId);
    }
}