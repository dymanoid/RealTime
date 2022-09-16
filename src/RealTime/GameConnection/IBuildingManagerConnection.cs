// <copyright file="IBuildingManagerConnection.cs" company="dymanoid">
//     Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.GameConnection
{
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>An interface for the game specific logic related to the building management.</summary>
    internal interface IBuildingManagerConnection
    {
        /// <summary>Gets the service type of the building with specified ID.</summary>
        /// <param name="buildingId">The ID of the building to get the service type of.</param>
        /// <returns>
        /// The service type of the building with the specified ID, or
        /// <see cref="ItemClass.Service.None"/> if <paramref name="buildingId"/> is 0.
        /// </returns>
        ItemClass.Service GetBuildingService(ushort buildingId);

        /// <summary>Gets the sub-service type of the building with specified ID.</summary>
        /// <param name="buildingId">The ID of the building to get the sub-service type of.</param>
        /// <returns>
        /// The sub-service type of the building with the specified ID, or
        /// <see cref="ItemClass.SubService.None"/> if <paramref name="buildingId"/> is 0.
        /// </returns>
        ItemClass.SubService GetBuildingSubService(ushort buildingId);

        /// <summary>Gets the service and sub-service types of the building with specified ID.</summary>
        /// <param name="buildingId">The ID of the building to get the service and sub-service types of.</param>
        /// <param name="service">The service type of the building with the specified ID, or
        /// <see cref="ItemClass.Service.None"/> if <paramref name="buildingId"/> is 0.</param>
        /// <param name="subService">The sub-service type of the building with the specified ID, or
        /// <see cref="ItemClass.SubService.None"/> if <paramref name="buildingId"/> is 0.</param>
        void GetBuildingService(ushort buildingId, out ItemClass.Service service, out ItemClass.SubService subService);

        /// <summary>Gets the citizen unit ID for the building with specified ID.</summary>
        /// <param name="buildingId">The building ID to search the citizen unit for.</param>
        /// <returns>The ID of the building's citizen unit, or 0 if none.</returns>
        uint GetCitizenUnit(ushort buildingId);

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
        bool BuildingHasFlags(ushort buildingId, Building.Flags flags, bool includeZero = false);

        /// <summary>Gets the distance in game units between two buildings with specified IDs.</summary>
        /// <param name="building1">The ID of the first building.</param>
        /// <param name="building2">The ID of the second building.</param>
        /// <returns>
        /// The distance between the buildings with specified IDs, 0 when any of the IDs is 0.
        /// </returns>
        float GetDistanceBetweenBuildings(ushort building1, ushort building2);

        /// <summary>Modifies the building's material buffer.</summary>
        /// <param name="buildingId">The ID of the building to modify.</param>
        /// <param name="reason">The reason for modification.</param>
        /// <param name="delta">The amount to modify the buffer by.</param>
        void ModifyMaterialBuffer(ushort buildingId, TransferManager.TransferReason reason, int delta);

        /// <summary>Finds an active building that matches the specified criteria and can accept visitors.</summary>
        /// <param name="searchAreaCenterBuilding">
        /// The building ID that represents the search area center point.
        /// </param>
        /// <param name="maxDistance">The maximum distance for search, the search area radius.</param>
        /// <param name="service">The building service type to find.</param>
        /// <param name="subService">The building sub-service type to find.</param>
        /// <returns>An ID of the first found building, or 0 if none found.</returns>
        ushort FindActiveBuilding(
            ushort searchAreaCenterBuilding,
            float maxDistance,
            ItemClass.Service service,
            ItemClass.SubService subService = ItemClass.SubService.None);

        /// <summary>Finds an active building that matches the specified criteria and can accept visitors.</summary>
        /// <param name="position">The search area center point.</param>
        /// <param name="maxDistance">The maximum distance for search, the search area radius.</param>
        /// <param name="service">The building service type to find.</param>
        /// <param name="subService">The building sub-service type to find.</param>
        /// <returns>An ID of the first found building, or 0 if none found.</returns>
        ushort FindActiveBuilding(
            Vector3 position,
            float maxDistance,
            ItemClass.Service service,
            ItemClass.SubService subService = ItemClass.SubService.None);

        /// <summary>Gets the ID of an event that takes place in the building with specified ID.</summary>
        /// <param name="buildingId">The building ID to check.</param>
        /// <returns>An ID of an event that takes place in the building, or 0 if none.</returns>
        ushort GetEvent(ushort buildingId);

        /// <summary>
        /// Gets an ID of a random building in the city that belongs to any of the specified <paramref name="services"/>.
        /// </summary>
        /// <exception cref="System.ArgumentNullException">Thrown when the argument is null.</exception>
        /// <param name="services">
        /// A collection of <see cref="ItemClass.Service"/> that specifies in which services to
        /// search the random building in.
        /// </param>
        /// <returns>An ID of a building; or 0 if none found.</returns>
        /// <remarks>
        /// NOTE: this method creates objects on the heap. To avoid memory pressure, don't call it on
        /// every simulation step.
        /// </remarks>
        ushort GetRandomBuilding(IEnumerable<ItemClass.Service> services);

        /// <summary>
        /// Sets the outgoing problem timer value for the building with specified ID.
        /// </summary>
        /// <param name="buildingId">The ID of building to set the problem timer for.</param>
        /// <param name="value">The outgoing problem timer value to set.</param>
        void SetOutgoingProblemTimer(ushort buildingId, byte value);

        /// <summary>
        /// Sets the workers problem timer value for the building with specified ID.
        /// </summary>
        /// <param name="buildingId">The ID of building to set the problem timer for.</param>
        /// <param name="value">The workers problem timer value to set.</param>
        void SetWorkersProblemTimer(ushort buildingId, byte value);

        /// <summary>Gets the class name of the building with specified ID.</summary>
        /// <param name="buildingId">The building ID to get the class name of.</param>
        /// <returns>A string representation of the building class, or null if none found.</returns>
        string GetBuildingClassName(ushort buildingId);

        /// <summary>Gets the localized name of a building with specified ID.</summary>
        /// <param name="buildingId">The building ID to get the name of.</param>
        /// <returns>A localized building name string, or null if none found.</returns>
        string GetBuildingName(ushort buildingId);

        /// <summary>
        /// Determines whether the building with specified ID is located in a noise restricted district.
        /// </summary>
        /// <param name="buildingId">The building ID to check.</param>
        /// <returns>
        ///   <c>true</c> if the building with specified ID is located in a noise restricted district;
        ///   otherwise, <c>false</c>.
        /// </returns>
        bool IsBuildingNoiseRestricted(ushort buildingId);

        /// <summary>Gets the maximum possible buildings count.</summary>
        /// <returns>The maximum possible buildings count.</returns>
        int GetMaxBuildingsCount();

        /// <summary>Gets the current buildings count in the city.</summary>
        /// <returns>The current buildings count.</returns>
        int GeBuildingsCount();

        /// <summary>Updates the building colors in the game by re-rendering the building.</summary>
        /// <param name="buildingId">The ID of the building to update.</param>
        void UpdateBuildingColors(ushort buildingId);

        /// <summary>Gets the building's height in game units.</summary>
        /// <param name="buildingId">The ID of the building.</param>
        /// <returns>The height of the building with the specified ID.</returns>
        float GetBuildingHeight(ushort buildingId);

        /// <summary>Visually deactivates the building with specified ID without affecting its production or coverage.</summary>
        /// <param name="buildingId">The building ID.</param>
        void DeactivateVisually(ushort buildingId);

        /// <summary>Gets the ID of the park area where the building with specified ID is located. Returns 0 if the building
        /// is not in a park.</summary>
        /// <param name="buildingId">The ID of the building to get the park ID of.</param>
        /// <returns>An ID of the park where the building is located, or 0.</returns>
        byte GetParkId(ushort buildingId);

        /// <summary>Gets the policies for a park with specified ID. Returns <see cref="DistrictPolicies.Park.None"/>
        /// if the specified park ID is 0 or invalid.</summary>
        /// <param name="parkId">The ID of the park to get policies of.</param>
        /// <returns>The policies of the park.</returns>
        DistrictPolicies.Park GetParkPolicies(byte parkId);

        /// <summary>
        /// Determines whether the area around the building with specified ID is currently being evacuated.
        /// </summary>
        /// <param name="buildingId">The building ID to check.</param>
        /// <returns>
        ///   <c>true</c> if the area around the building with specified ID is currently being evacuated; otherwise, <c>false</c>.
        /// </returns>
        bool IsAreaEvacuating(ushort buildingId);

        /// <summary>
        /// Determines whether the building with specified ID is a real unique building (not a stadium, not a concert area).
        /// </summary>
        /// <param name="buildingId">The building ID to check.</param>
        /// <returns>
        ///   <c>true</c> if the building with the specified ID is a real unique building; otherwise, <c>false</c>.
        /// </returns>
        bool IsRealUniqueBuilding(ushort buildingId);

        /// <summary>
        /// Determines whether the building with specified ID is the main building of an Industrial or a Campus area.
        /// </summary>
        /// <param name="buildingId">The building ID to check.</param>
        /// <returns>
        ///   <c>true</c> if the building with the specified ID is the main building of an Industrial or a Campus area;
        ///   otherwise, <c>false</c>.
        /// </returns>
        bool IsAreaMainBuilding(ushort buildingId);

        /// <summary>
        /// Determines whether the building with specified ID is a residental building of an Industrial or a Campus area.
        /// </summary>
        /// <param name="buildingId">The building ID to check.</param>
        /// <returns>
        ///   <c>true</c> if the building with the specified ID is a residental building of an Industrial or a Campus area;
        ///   otherwise, <c>false</c>.
        /// </returns>
        bool IsAreaResidentalBuilding(ushort buildingId);

        /// <summary>
        /// Determines whether the AI class of the building with specified ID is of the specified type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of the building AI to check for. It must be a <see cref="BuildingAI"/>.</typeparam>
        /// <param name="buildingId">The building ID to check.</param>
        /// <returns>
        ///   <c>true</c> if the AI class of the building with the specified ID is of the type <typeparamref name="T"/>;
        ///   otherwise, <c>false</c>.
        /// </returns>
        bool IsBuildingAIOfType<T>(ushort buildingId)
            where T : BuildingAI;

        /// <summary>
        /// Determines whether the building with specified ID is of the specified service type and of the specified level.
        /// </summary>
        /// <param name="buildingId">The building ID to check.</param>
        /// <param name="buildingService">The service type to check the building for.</param>
        /// <param name="buildingLevel">The building level to check the building for.</param>
        /// <returns>
        ///   <c>true</c> if the building is of the specified service type and of the specified level;
        ///   otherwise, <c>false</c>.
        /// </returns>
        bool IsBuildingServiceLevel(ushort buildingId, ItemClass.Service buildingService, ItemClass.Level buildingLevel);
    }
}
