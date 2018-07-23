// <copyright file="IEventManagerConnection.cs" company="dymanoid">
//     Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.GameConnection
{
    using System;
    using System.Collections.Generic;

    /// <summary>An interface for the game specific logic related to the event management.</summary>
    internal interface IEventManagerConnection
    {
        /// <summary>Gets the flags of an event with specified ID.</summary>
        /// <param name="eventId">The ID of the event to get flags of.</param>
        /// <returns>The event flags or <see cref="EventData.Flags.None"/> if none found.</returns>
        EventData.Flags GetEventFlags(ushort eventId);

        /// <summary>
        /// Gets a collection of the IDs of upcoming city events in the specified time interval.
        /// </summary>
        /// <param name="earliestTime">The start time of the interval to get events from.</param>
        /// <param name="latestTime">The end time of the interval to get events from.</param>
        /// <returns>A collection of the city event IDs.</returns>
        IEnumerable<ushort> GetUpcomingEvents(DateTime earliestTime, DateTime latestTime);

        /// <summary>Gets various information about a city event with specified ID.</summary>
        /// <param name="eventId">The ID of the city event to get information for.</param>
        /// <param name="buildingId">The ID of a building where the city event takes place.</param>
        /// <param name="startTime">The start time of the city event.</param>
        /// <param name="duration">The duration in hours of the city event.</param>
        /// <param name="ticketPrice">The city event's ticket price.</param>
        /// <returns><c>true</c> if the information was retrieved; otherwise, <c>false</c>.</returns>
        bool TryGetEventInfo(ushort eventId, out ushort buildingId, out DateTime startTime, out float duration, out float ticketPrice);
    }
}