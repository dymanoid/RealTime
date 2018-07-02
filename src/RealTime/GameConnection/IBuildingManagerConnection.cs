// <copyright file="IBuildingManagerConnection.cs" company="dymanoid">
//     Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.GameConnection
{
    using System;
    using System.Collections.Generic;

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

        /// <summary>Gets the citizen unit ID for the building with specified ID.</summary>
        /// <param name="buildingId">The building ID to search the citizen unit for.</param>
        /// <returns>The ID of the building's citizen unit, or 0 if none.</returns>
        uint GetCitizenUnit(ushort buildingId);

        /// <summary>
        /// Gets a value indicating whether the building with specified ID has particular flags.
        /// </summary>
        /// <param name="buildingId">The ID of the building to check the flags of.</param>
        /// <param name="flags">The building flags to check.</param>
        /// ///
        /// <returns>
        /// <c>true</c> if the building with the specified ID has the <paramref name="flags"/>
        /// provided; otherwise, <c>false</c>.
        /// </returns>
        bool BuildingHasFlags(ushort buildingId, Building.Flags flags);

        /// <summary>Gets the distance in game units between two buildings with specified IDs.</summary>
        /// <param name="building1">The ID of the first building.</param>
        /// <param name="building2">The ID of the second building.</param>
        /// <returns>
        /// A distance between the buildings with specified IDs, 0 when any of the IDs is 0.
        /// </returns>
        float GetDistanceBetweenBuildings(ushort building1, ushort building2);

        /// <summary>Modifies the building's material buffer.</summary>
        /// <param name="buildingId">The ID of the building to modify.</param>
        /// <param name="reason">The reason for modification.</param>
        /// <param name="delta">The amount to modify the buffer by.</param>
        void ModifyMaterialBuffer(ushort buildingId, TransferManager.TransferReason reason, int delta);

        /// <summary>Finds an active building that matches the specified criteria.</summary>
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

        /// <summary>Gets the ID of an event that takes place in the building with provided ID.</summary>
        /// <param name="buildingId">The building ID to check.</param>
        /// <returns>An ID of an event that takes place in the building, or 0 if none.</returns>
        ushort GetEvent(ushort buildingId);

        /// <summary>
        /// Gets an ID of a random building in the city that belongs to any of the provided <paramref name="services"/>.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when the argument is null.</exception>
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
        /// Decrements the outgoing problem timer for all buildings of the specified service type and
        /// whose IDs are between the specified values.
        /// </summary>
        /// <param name="buildingIdFrom">The left range value of the building IDs to process.</param>
        /// <param name="buildingIdTo">The right range value of the building IDs to process.</param>
        /// <param name="service">The service type to process buildings of.</param>
        void DecrementOutgoingProblemTimer(ushort buildingIdFrom, ushort buildingIdTo, ItemClass.Service service);

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
    }
}