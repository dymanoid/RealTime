// <copyright file="IBuildingManagerConnection.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.GameConnection
{
    using System;
    using System.Collections.Generic;

    internal interface IBuildingManagerConnection
    {
        ItemClass.Service GetBuildingService(ushort buildingId);

        ItemClass.SubService GetBuildingSubService(ushort buildingId);

        bool BuildingHasFlags(ushort buildingId, Building.Flags flags);

        float GetDistanceBetweenBuildings(ushort building1, ushort building2);

        void ModifyMaterialBuffer(ushort buildingId, TransferManager.TransferReason reason, int delta);

        ushort FindActiveBuilding(
            ushort searchAreaCenterBuilding,
            float maxDistance,
            ItemClass.Service service,
            ItemClass.SubService subService = ItemClass.SubService.None);

        ushort GetEvent(ushort buildingId);

        /// <summary>
        /// Gets a random building ID for a bulding in the city that belongs
        /// to any of the provided <paramref name="services"/>.
        /// </summary>
        ///
        /// <exception cref="ArgumentNullException">Thrown when the argument is null.</exception>
        ///
        /// <param name="services">A collection of <see cref="ItemClass.Service"/> that specifies
        /// in which services to search the random building in.</param>
        ///
        /// <returns>An ID of a building; or 0 if none found.</returns>
        ///
        /// <remarks>NOTE: this method creates objects on the heap. To avoid memory pressure,
        /// don't call it on every simulation step.</remarks>
        ushort GetRandomBuilding(IEnumerable<ItemClass.Service> services);

        void DecrementOutgoingProblemTimer(ushort buildingIdFrom, ushort buildingIdTo, ItemClass.Service service);

        string GetBuildingClassName(ushort buildingId);

        string GetBuildingName(ushort buildingId);
    }
}