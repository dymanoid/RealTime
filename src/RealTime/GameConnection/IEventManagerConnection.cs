// <copyright file="IEventManagerConnection.cs" company="dymanoid">
//     Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.GameConnection
{
    using System;
    using RealTime.Events;

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
        IReadOnlyList<ushort> GetUpcomingEvents(DateTime earliestTime, DateTime latestTime);

        /// <summary>Gets various information about a city event with specified ID.</summary>
        /// <param name="eventId">The ID of the city event to get information for.</param>
        /// <param name="eventInfo">A <see cref="VanillaEventInfo"/> ref-struct containing the event information.</param>
        /// <returns><c>true</c> if the information was retrieved; otherwise, <c>false</c>.</returns>
        bool TryGetEventInfo(ushort eventId, out VanillaEventInfo eventInfo);

        /// <summary>Gets the start time of a city event with specified ID.</summary>
        /// <param name="eventId">The ID of the city event to get start time of.</param>
        /// <param name="startTime">The start time of the event with the specified ID.</param>
        /// <returns><c>true</c> if the start time was retrieved; otherwise, <c>false</c>.</returns>
        bool TryGetEventStartTime(ushort eventId, out DateTime startTime);

        /// <summary>Gets the color of a city event with specified ID.</summary>
        /// <param name="eventId">The ID of the city event to get the color of.</param>
        /// <returns>The color of the event.</returns>
        EventColor GetEventColor(ushort eventId);

        /// <summary>Sets the start time of the event to the specified value.</summary>
        /// <param name="eventId">The ID of the event to change.</param>
        /// <param name="startTime">The new event start time.</param>
        void SetStartTime(ushort eventId, DateTime startTime);
    }
}