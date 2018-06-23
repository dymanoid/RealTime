// <copyright file="CameraHelper.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.GameConnection
{
    using UnityEngine;

    internal static class CameraHelper
    {
        public static void NavigateToBuilding(ushort buildingId)
        {
            if (buildingId == 0)
            {
                return;
            }

            InstanceID instance = default;
            instance.Building = buildingId;

            Vector3 buildingPosition = BuildingManager.instance.m_buildings.m_buffer[buildingId].m_position;
            ToolsModifierControl.cameraController.SetTarget(instance, buildingPosition, true);
        }
    }
}
