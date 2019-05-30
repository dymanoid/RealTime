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
        private readonly List<ushort> upcomingEvents = new List<ushort>();
        private readonly ReadOnlyList<ushort> readonlyUpcomingEvents;

        /// <summary>
        /// Initializes a new instance of the <see cref="EventManagerConnection"/> class.
        /// </summary>
        public EventManagerConnection()
        {
            readonlyUpcomingEvents = new ReadOnlyList<ushort>(upcomingEvents);
        }

        /// <summary>Gets the flags of an event with specified ID.</summary>
        /// <param name="eventId">The ID of the event to get flags of.</param>
        /// <returns>
        /// The event flags or <see cref="EventData.Flags.None" /> if none found.
        /// </returns>
        public EventData.Flags GetEventFlags(ushort eventId)
        {
            if (eventId == 0 || eventId >= EventManager.instance.m_events.m_size)
            {
                return EventData.Flags.None;
            }

            ref EventData eventData = ref EventManager.instance.m_events.m_buffer[eventId];
            return eventData.Info?.m_type == EventManager.EventType.AcademicYear
                ? EventData.Flags.None
                : eventData.m_flags;
        }

        /// <summary>
        /// Gets a collection of the IDs of upcoming city events in the specified time interval.
        /// </summary>
        /// <param name="earliestTime">The start time of the interval to get events from.</param>
        /// <param name="latestTime">The end time of the interval to get events from.</param>
        /// <returns>A collection of the city event IDs.</returns>
        public IReadOnlyList<ushort> GetUpcomingEvents(DateTime earliestTime, DateTime latestTime)
        {
            upcomingEvents.Clear();
            FastList<EventData> events = EventManager.instance.m_events;
            for (ushort i = 1; i < events.m_size; ++i)
            {
                ref EventData eventData = ref events.m_buffer[i];

                if ((eventData.m_flags & (EventData.Flags.Preparing | EventData.Flags.Ready | EventData.Flags.Active)) == 0)
                {
                    continue;
                }

                if ((eventData.m_flags
                    & (EventData.Flags.Cancelled | EventData.Flags.Completed | EventData.Flags.Deleted | EventData.Flags.Expired)) != 0)
                {
                    continue;
                }

                if (eventData.Info?.m_type == EventManager.EventType.AcademicYear)
                {
                    continue;
                }

                if (eventData.StartTime >= earliestTime && eventData.StartTime < latestTime)
                {
                    upcomingEvents.Add(i);
                }
            }

            return readonlyUpcomingEvents;
        }

        /// <summary>Gets the start time of a city event with specified ID.</summary>
        /// <param name="eventId">The ID of the city event to get start time of.</param>
        /// <param name="startTime">The start time of the event with the specified ID.</param>
        /// <returns><c>true</c> if the start time was retrieved; otherwise, <c>false</c>.</returns>
        public bool TryGetEventStartTime(ushort eventId, out DateTime startTime)
        {
            if (eventId == 0 || eventId >= EventManager.instance.m_events.m_size)
            {
                startTime = default;
                return false;
            }

            ref EventData eventData = ref EventManager.instance.m_events.m_buffer[eventId];
            if (eventData.Info?.m_type == EventManager.EventType.AcademicYear)
            {
                startTime = default;
                return false;
            }

            startTime = eventData.StartTime;
            return true;
        }

        /// <summary>
        /// Gets various information about a city event with specified ID.
        /// </summary>
        /// <param name="eventId">The ID of the city event to get information for.</param>
        /// <param name="eventInfo">A <see cref="VanillaEventInfo"/> ref-struct containing the event information.</param>
        /// <returns>
        ///   <c>true</c> if the information was retrieved; otherwise, <c>false</c>.
        /// </returns>
        public bool TryGetEventInfo(ushort eventId, out VanillaEventInfo eventInfo)
        {
            if (eventId == 0 || eventId >= EventManager.instance.m_events.m_size)
            {
                eventInfo = default;
                return false;
            }

            ref EventData eventData = ref EventManager.instance.m_events.m_buffer[eventId];
            if (eventData.Info?.m_type == EventManager.EventType.AcademicYear)
            {
                eventInfo = default;
                return false;
            }

            eventInfo = new VanillaEventInfo(
                eventData.m_building,
                eventData.StartTime,
                eventData.Info.m_eventAI.m_eventDuration,
                eventData.m_ticketPrice / 100f);
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
            if (eventData.Info?.m_type == EventManager.EventType.AcademicYear)
            {
                return;
            }

            uint duration = eventData.m_expireFrame - eventData.m_startFrame;
            eventData.m_startFrame = SimulationManager.instance.TimeToFrame(startTime);
            eventData.m_expireFrame = eventData.m_startFrame + duration;
        }
    }
}
