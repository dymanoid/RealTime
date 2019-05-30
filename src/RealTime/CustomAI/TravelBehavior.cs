// <copyright file="TravelBehavior.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.CustomAI
{
    using System;
    using RealTime.GameConnection;
    using SkyTools.Tools;
    using static Constants;

    /// <summary>
    /// A behavior for citizens traveling.
    /// </summary>
    internal sealed class TravelBehavior : ITravelBehavior
    {
        private readonly IBuildingManagerConnection buildingManager;
        private readonly float travelDistancePerCycle;
        private float averageTravelSpeedPerHour;

        /// <summary>Initializes a new instance of the <see cref="TravelBehavior"/> class.</summary>
        /// <param name="buildingManager">
        /// A proxy object that provides a way to call the game-specific methods of the <see cref="BuildingManager"/> class.
        /// </param>
        /// <param name="travelDistancePerCycle">The average distance a citizen can travel during a single simulation cycle.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="buildingManager"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="travelDistancePerCycle"/> is negative or zero.</exception>
        public TravelBehavior(IBuildingManagerConnection buildingManager, float travelDistancePerCycle)
        {
            if (travelDistancePerCycle <= 0)
            {
                throw new ArgumentException("The travel distance per cycle cannot be negative or zero.");
            }

            this.buildingManager = buildingManager ?? throw new ArgumentNullException(nameof(buildingManager));
            this.travelDistancePerCycle = travelDistancePerCycle;
            averageTravelSpeedPerHour = travelDistancePerCycle;
        }

        /// <summary>Sets the duration (in hours) of a full simulation cycle for all citizens.
        /// The game calls the simulation methods for a particular citizen with this period.</summary>
        /// <param name="cyclePeriod">The citizens simulation cycle period, in game hours.</param>
        public void SetSimulationCyclePeriod(float cyclePeriod)
        {
            if (cyclePeriod == 0)
            {
                cyclePeriod = 1f;
            }

            averageTravelSpeedPerHour = travelDistancePerCycle / cyclePeriod;
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
            if (distance == 0)
            {
                return MinTravelTime;
            }

            return FastMath.Clamp(distance / averageTravelSpeedPerHour, MinTravelTime, MaxTravelTime);
        }
    }
}
