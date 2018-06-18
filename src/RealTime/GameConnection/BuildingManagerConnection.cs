// <copyright file="BuildingManagerConnection.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.GameConnection
{
    using RealTime.Tools;
    using UnityEngine;

    internal sealed class BuildingManagerConnection : IBuildingManagerConnection
    {
        public ItemClass.Service GetBuildingService(ushort buildingId)
        {
            return buildingId == 0
                ? ItemClass.Service.None
                : BuildingManager.instance.m_buildings.m_buffer[buildingId].Info.m_class.m_service;
        }

        public ItemClass.SubService GetBuildingSubService(ushort buildingId)
        {
            return buildingId == 0
                ? ItemClass.SubService.None
                : BuildingManager.instance.m_buildings.m_buffer[buildingId].Info.m_class.m_subService;
        }

        public Building.Flags GetBuildingFlags(ushort buildingId)
        {
            return buildingId == 0
                ? Building.Flags.None
                : BuildingManager.instance.m_buildings.m_buffer[buildingId].m_flags;
        }

        public float GetDistanceBetweenBuildings(ushort building1, ushort building2)
        {
            if (building1 == 0 || building2 == 0)
            {
                return float.MaxValue;
            }

            Building[] buildings = BuildingManager.instance.m_buildings.m_buffer;
            return Vector3.Distance(buildings[building1].m_position, buildings[building2].m_position);
        }

        public void ModifyMaterialBuffer(ushort buildingId, TransferManager.TransferReason reason, int delta)
        {
            if (buildingId == 0 || delta == 0)
            {
                return;
            }

            ref Building building = ref BuildingManager.instance.m_buildings.m_buffer[buildingId];
            building.Info.m_buildingAI.ModifyMaterialBuffer(buildingId, ref building, reason, ref delta);
        }

        public ushort FindActiveBuilding(
            ushort searchAreaCenterBuilding,
            float maxDistance,
            ItemClass.Service service,
            ItemClass.SubService subService = ItemClass.SubService.None)
        {
            if (searchAreaCenterBuilding == 0)
            {
                return 0;
            }

            Vector3 currentPosition = BuildingManager.instance.m_buildings.m_buffer[searchAreaCenterBuilding].m_position;

            Building.Flags restrictedFlags = Building.Flags.Deleted | Building.Flags.Evacuating | Building.Flags.Flooded | Building.Flags.Collapsed
                | Building.Flags.BurnedDown | Building.Flags.RoadAccessFailed;

            return BuildingManager.instance.FindBuilding(
                currentPosition,
                maxDistance,
                service,
                subService,
                Building.Flags.Created | Building.Flags.Completed | Building.Flags.Active,
                restrictedFlags);
        }

        public ushort GetEvent(ushort buildingId)
        {
            return buildingId == 0
                ? (ushort)0
                : BuildingManager.instance.m_buildings.m_buffer[buildingId].m_eventIndex;
        }
    }
}
