// <copyright file="IRealTimeEventManager.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.Events
{
    using System;

    /// <summary>An interface for the customized city events manager.</summary>
    internal interface IRealTimeEventManager
    {
        /// <summary>
        /// Gets the <see cref="ICityEvent"/> instance of an ongoing or upcoming city event that takes place in a building
        /// with specified ID.
        /// </summary>
        /// <param name="buildingId">The ID of a building to search events for.</param>
        /// <returns>An <see cref="ICityEvent"/> instance of the first matching city event, or null if none found.</returns>
        ICityEvent GetCityEvent(ushort buildingId);

        /// <summary>
        /// Gets the <see cref="ICityEvent"/> instance of an upcoming city event whose start time is between the specified values.
        /// </summary>
        /// <param name="earliestStartTime">The earliest city event start time to consider.</param>
        /// <param name="latestStartTime">The latest city event start time to consider.</param>
        /// <returns>An <see cref="ICityEvent"/> instance of the first matching city event, or null if none found.</returns>
        ICityEvent GetUpcomingCityEvent(DateTime earliestStartTime, DateTime latestStartTime);

        /// <summary>Gets the state of a city event in the specified building.</summary>
        /// <param name="buildingId">The building ID to check events in.</param>
        /// <param name="latestStart">The latest start time of events to consider.</param>
        /// <returns>
        /// The state of an event that meets the specified criteria, or <see cref="CityEventState.None"/> if none found.
        /// </returns>
        CityEventState GetEventState(ushort buildingId, DateTime latestStart);
    }
}