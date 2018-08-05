// <copyright file="EventManagerConnection.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.GameConnection
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// The default implementation of the <see cref="IEventManagerConnection"/> interface.
    /// </summary>
    /// <seealso cref="IEventManagerConnection" />
    internal sealed class EventManagerConnection : IEventManagerConnection
    {
        /// <summary>Gets the flags of an event with specified ID.</summary>
        /// <param name="eventId">The ID of the event to get flags of.</param>
        /// <returns>
        /// The event flags or <see cref="EventData.Flags.None" /> if none found.
        /// </returns>
        public EventData.Flags GetEventFlags(ushort eventId)
        {
            return eventId == 0 || eventId >= EventManager.instance.m_events.m_size
                ? EventData.Flags.None
                : EventManager.instance.m_events.m_buffer[eventId].m_flags;
        }

        /// <summary>
        /// Gets a collection of the IDs of upcoming city events in the specified time interval.
        /// </summary>
        /// <param name="earliestTime">The start time of the interval to get events from.</param>
        /// <param name="latestTime">The end time of the interval to get events from.</param>
        /// <returns>A collection of the city event IDs.</returns>
        public IEnumerable<ushort> GetUpcomingEvents(DateTime earliestTime, DateTime latestTime)
        {
            FastList<EventData> events = EventManager.instance.m_events;
            for (ushort i = 1; i < events.m_size; ++i)
            {
                if ((events.m_buffer[i].m_flags & (EventData.Flags.Preparing | EventData.Flags.Ready | EventData.Flags.Active)) == 0)
                {
                    continue;
                }

                if ((events.m_buffer[i].m_flags
                    & (EventData.Flags.Cancelled | EventData.Flags.Completed | EventData.Flags.Deleted | EventData.Flags.Expired)) != 0)
                {
                    continue;
                }

                if (events.m_buffer[i].StartTime >= earliestTime && events.m_buffer[i].StartTime < latestTime)
                {
                    yield return i;
                }
            }
        }

        /// <summary>
        /// Gets various information about a city event with specified ID.
        /// </summary>
        /// <param name="eventId">The ID of the city event to get information for.</param>
        /// <param name="buildingId">The ID of a building where the city event takes place.</param>
        /// <param name="startTime">The start time of the city event.</param>
        /// <param name="duration">The duration in hours of the city event.</param>
        /// <param name="ticketPrice">The city event's ticket price.</param>
        /// <returns>
        ///   <c>true</c> if the information was retrieved; otherwise, <c>false</c>.
        /// </returns>
        public bool TryGetEventInfo(ushort eventId, out ushort buildingId, out DateTime startTime, out float duration, out float ticketPrice)
        {
            buildingId = default;
            duration = default;
            startTime = default;
            ticketPrice = default;
            if (eventId == 0 || eventId >= EventManager.instance.m_events.m_size)
            {
                return false;
            }

            ref EventData eventData = ref EventManager.instance.m_events.m_buffer[eventId];
            buildingId = eventData.m_building;
            startTime = eventData.StartTime;
            duration = eventData.Info.m_eventAI.m_eventDuration;
            ticketPrice = eventData.m_ticketPrice / 100f;
            return true;
        }

        /// <summary>Sets the start time of the event to the specified value.</summary>
        /// <param name="eventId">The ID of the event to change.</param>
        /// <param name="startTime">The new event start time.</param>
        public void SetStartTime(ushort eventId, DateTime startTime)
        {
            if (eventId == 0 || eventId >= EventManager.instance.m_events.m_size)
            {
                return;
            }

            ref EventData eventData = ref EventManager.instance.m_events.m_buffer[eventId];
            uint oldStartTime = eventData.m_startFrame;
            eventData.m_startFrame = SimulationManager.instance.TimeToFrame(startTime);
            eventData.m_expireFrame = eventData.m_expireFrame + (eventData.m_startFrame - oldStartTime);
        }
    }
}
