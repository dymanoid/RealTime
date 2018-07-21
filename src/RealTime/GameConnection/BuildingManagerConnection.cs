// <copyright file="BuildingManagerConnection.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.GameConnection
{
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    /// <summary>
    /// A default implementation of the <see cref="IBuildingManagerConnection"/> interface.
    /// </summary>
    /// <seealso cref="IBuildingManagerConnection" />
    internal sealed class BuildingManagerConnection : IBuildingManagerConnection
    {
        /// <summary>Gets the service type of the building with specified ID.</summary>
        /// <param name="buildingId">The ID of the building to get the service type of.</param>
        /// <returns>
        /// The service type of the building with the specified ID, or
        /// <see cref="ItemClass.Service.None" /> if <paramref name="buildingId" /> is 0.
        /// </returns>
        public ItemClass.Service GetBuildingService(ushort buildingId)
        {
            return buildingId == 0
                ? ItemClass.Service.None
                : BuildingManager.instance.m_buildings.m_buffer[buildingId].Info.m_class.m_service;
        }

        /// <summary>Gets the sub-service type of the building with specified ID.</summary>
        /// <param name="buildingId">The ID of the building to get the sub-service type of.</param>
        /// <returns>
        /// The sub-service type of the building with the specified ID, or
        /// <see cref="ItemClass.SubService.None" /> if <paramref name="buildingId" /> is 0.
        /// </returns>
        public ItemClass.SubService GetBuildingSubService(ushort buildingId)
        {
            return buildingId == 0
                ? ItemClass.SubService.None
                : BuildingManager.instance.m_buildings.m_buffer[buildingId].Info.m_class.m_subService;
        }

        /// <summary>Gets the citizen unit ID for the building with specified ID.</summary>
        /// <param name="buildingId">The building ID to search the citizen unit for.</param>
        /// <returns>The ID of the building's citizen unit, or 0 if none.</returns>
        public uint GetCitizenUnit(ushort buildingId)
        {
            return buildingId == 0
                ? 0
                : BuildingManager.instance.m_buildings.m_buffer[buildingId].m_citizenUnits;
        }

        /// <summary>
        /// Gets a value indicating whether the building with specified ID has particular flags.
        /// The single <see cref="Building.Flags.None"/> value can also be checked for.
        /// </summary>
        /// <param name="buildingId">The ID of the building to check the flags of.</param>
        /// <param name="flags">The building flags to check.</param>
        /// <param name="includeZero"><c>true</c> if a building without any flags can also be considered.</param>
        /// <returns>
        /// <c>true</c> if the building with the specified ID has the specified <paramref name="flags"/>;
        /// otherwise, <c>false</c>.
        /// </returns>
        public bool BuildingHasFlags(ushort buildingId, Building.Flags flags, bool includeZero = false)
        {
            if (buildingId == 0)
            {
                return false;
            }

            Building.Flags buildingFlags = BuildingManager.instance.m_buildings.m_buffer[buildingId].m_flags;
            return (buildingFlags & flags) != 0 || (includeZero && buildingFlags == Building.Flags.None);
        }

        /// <summary>
        /// Gets the distance in game units between two buildings with specified IDs.
        /// </summary>
        /// <param name="building1">The ID of the first building.</param>
        /// <param name="building2">The ID of the second building.</param>
        /// <returns>
        /// A distance between the buildings with specified IDs, 0 when any of the IDs is 0.
        /// </returns>
        public float GetDistanceBetweenBuildings(ushort building1, ushort building2)
        {
            if (building1 == 0 || building2 == 0)
            {
                return float.MaxValue;
            }

            Building[] buildings = BuildingManager.instance.m_buildings.m_buffer;
            return Vector3.Distance(buildings[building1].m_position, buildings[building2].m_position);
        }

        /// <summary>Modifies the building's material buffer.</summary>
        /// <param name="buildingId">The ID of the building to modify.</param>
        /// <param name="reason">The reason for modification.</param>
        /// <param name="delta">The amount to modify the buffer by.</param>
        public void ModifyMaterialBuffer(ushort buildingId, TransferManager.TransferReason reason, int delta)
        {
            if (buildingId == 0 || delta == 0)
            {
                return;
            }

            ref Building building = ref BuildingManager.instance.m_buildings.m_buffer[buildingId];
            building.Info.m_buildingAI.ModifyMaterialBuffer(buildingId, ref building, reason, ref delta);
        }

        /// <summary>Finds an active building that matches the specified criteria.</summary>
        /// <param name="searchAreaCenterBuilding">The building ID that represents the search area center point.</param>
        /// <param name="maxDistance">The maximum distance for search, the search area radius.</param>
        /// <param name="service">The building service type to find.</param>
        /// <param name="subService">The building sub-service type to find.</param>
        /// <returns>An ID of the first found building, or 0 if none found.</returns>
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

        /// <summary>Gets the ID of an event that takes place in the building with specified ID.</summary>
        /// <param name="buildingId">The building ID to check.</param>
        /// <returns>An ID of an event that takes place in the building, or 0 if none.</returns>
        public ushort GetEvent(ushort buildingId)
        {
            return buildingId == 0
                ? (ushort)0
                : BuildingManager.instance.m_buildings.m_buffer[buildingId].m_eventIndex;
        }

        /// <summary>
        /// Gets an ID of a random building in the city that belongs to any of the specified <paramref name="services" />.
        /// </summary>
        /// <param name="services">A collection of <see cref="ItemClass.Service" /> that specifies in which services to
        /// search the random building in.</param>
        /// <returns>An ID of a building; or 0 if none found.</returns>
        /// <remarks>
        /// NOTE: this method creates objects on the heap. To avoid memory pressure, don't call it on
        /// every simulation step.
        /// </remarks>
        public ushort GetRandomBuilding(IEnumerable<ItemClass.Service> services)
        {
            // No memory pressure here because this method will not be called on each simulation step
            var buildings = new List<FastList<ushort>>();

            int totalCount = 0;
            foreach (FastList<ushort> serviceBuildings in services
                .Select(s => BuildingManager.instance.GetServiceBuildings(s))
                .Where(b => b != null))
            {
                totalCount += serviceBuildings.m_size;
                buildings.Add(serviceBuildings);
            }

            if (totalCount == 0)
            {
                return 0;
            }

            int buildingNumber = SimulationManager.instance.m_randomizer.Int32((uint)totalCount);
            totalCount = 0;
            foreach (FastList<ushort> serviceBuildings in buildings)
            {
                if (buildingNumber < totalCount + serviceBuildings.m_size)
                {
                    return serviceBuildings[buildingNumber - totalCount];
                }

                totalCount += serviceBuildings.m_size;
            }

            return 0;
        }

        /// <summary>
        /// Sets the outgoing problem timer value for the building with specified ID.
        /// </summary>
        /// <param name="buildingId">The ID of building to set the problem timer for.</param>
        /// <param name="value">The outgoing problem timer value to set.</param>
        public void SetOutgoingProblemTimer(ushort buildingId, byte value)
        {
            if (buildingId != 0)
            {
                BuildingManager.instance.m_buildings.m_buffer[buildingId].m_outgoingProblemTimer = value;
            }
        }

        /// <summary>
        /// Sets the workers problem timer value for the building with specified ID.
        /// </summary>
        /// <param name="buildingId">The ID of building to set the problem timer for.</param>
        /// <param name="value">The workers problem timer value to set.</param>
        public void SetWorkersProblemTimer(ushort buildingId, byte value)
        {
            if (buildingId != 0)
            {
                BuildingManager.instance.m_buildings.m_buffer[buildingId].m_workerProblemTimer = value;
            }
        }

        /// <summary>Gets the class name of the building with specified ID.</summary>
        /// <param name="buildingId">The building ID to get the class name of.</param>
        /// <returns>
        /// A string representation of the building class, or null if none found.
        /// </returns>
        public string GetBuildingClassName(ushort buildingId)
        {
            return buildingId == 0
                ? string.Empty
                : BuildingManager.instance.m_buildings.m_buffer[buildingId].Info.name;
        }

        /// <summary>Gets the localized name of a building with specified ID.</summary>
        /// <param name="buildingId">The building ID to get the name of.</param>
        /// <returns>A localized building name string, or null if none found.</returns>
        public string GetBuildingName(ushort buildingId)
        {
            return buildingId == 0
                ? string.Empty
                : BuildingManager.instance.GetBuildingName(buildingId, InstanceID.Empty);
        }

        /// <summary>
        /// Determines whether the building with specified ID is located in a noise restricted district.
        /// </summary>
        /// <param name="buildingId">The building ID to check.</param>
        /// <returns>
        ///   <c>true</c> if the building with specified ID is located in a noise restricted district;
        ///   otherwise, <c>false</c>.
        /// </returns>
        public bool IsBuildingNoiseRestricted(ushort buildingId)
        {
            if (buildingId == 0)
            {
                return false;
            }

            Vector3 location = BuildingManager.instance.m_buildings.m_buffer[buildingId].m_position;
            byte district = DistrictManager.instance.GetDistrict(location);
            DistrictPolicies.CityPlanning policies = DistrictManager.instance.m_districts.m_buffer[district].m_cityPlanningPolicies;
            return (policies & DistrictPolicies.CityPlanning.NoLoudNoises) != 0;
        }
    }
}
