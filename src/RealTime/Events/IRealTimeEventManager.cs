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
        /// Gets the events that can be attended by the citizens when they start traveling to the event
        /// at the current game time.
        /// </summary>
        IReadOnlyList<ICityEvent> EventsToAttend { get; }

        /// <summary>
        /// Gets the <see cref="ICityEvent"/> instance of an ongoing or upcoming city event that takes place in a building
        /// with specified ID.
        /// </summary>
        /// <param name="buildingId">The ID of a building to search events for.</param>
        /// <returns>An <see cref="ICityEvent"/> instance of the first matching city event, or null if none found.</returns>
        ICityEvent GetCityEvent(ushort buildingId);

        /// <summary>Gets the state of a city event in the specified building.</summary>
        /// <param name="buildingId">The building ID to check events in.</param>
        /// <param name="latestStart">The latest start time of events to consider.</param>
        /// <returns>
        /// The state of an event that meets the specified criteria, or <see cref="CityEventState.None"/> if none found.
        /// </returns>
        CityEventState GetEventState(ushort buildingId, DateTime latestStart);
    }
}