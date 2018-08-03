// <copyright file="TravelBehavior.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.CustomAI
{
    using RealTime.GameConnection;
    using SkyTools.Tools;
    using static Constants;

    /// <summary>
    /// A behavior for citizens traveling.
    /// </summary>
    internal sealed class TravelBehavior : ITravelBehavior
    {
        private readonly IBuildingManagerConnection buildingManager;

        /// <summary>Initializes a new instance of the <see cref="TravelBehavior"/> class.</summary>
        /// <param name="buildingManager">
        /// A proxy object that provides a way to call the game-specific methods of the <see cref="BuildingManager"/> class.
        /// </param>
        /// <exception cref="System.ArgumentNullException">Thrown when the argument is null.</exception>
        public TravelBehavior(IBuildingManagerConnection buildingManager)
        {
            this.buildingManager = buildingManager ?? throw new System.ArgumentNullException(nameof(buildingManager));
        }

        /// <summary>Gets an estimated travel time (in hours) between two specified buildings.</summary>
        /// <param name="building1">The ID of the first building.</param>
        /// <param name="building2">The ID of the second building.</param>
        /// <returns>An estimated travel time in hours.</returns>
        public float GetEstimatedTravelTime(ushort building1, ushort building2)
        {
            if (building1 == 0 || building2 == 0 || building1 == building2)
            {
                return 0;
            }

            float distance = buildingManager.GetDistanceBetweenBuildings(building1, building2);
            return FastMath.Clamp(distance / OnTheWayDistancePerHour, MinTravelTime, MaxTravelTime);
        }
    }
}
