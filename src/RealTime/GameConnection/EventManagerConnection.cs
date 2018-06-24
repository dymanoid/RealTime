// <copyright file="EventManagerConnection.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.GameConnection
{
    using System;
    using System.Collections.Generic;

    internal sealed class EventManagerConnection : IEventManagerConnection
    {
        public EventData.Flags GetEventFlags(ushort eventId)
        {
            return eventId == 0
                ? EventData.Flags.None
                : EventManager.instance.m_events.m_buffer[eventId].m_flags;
        }

        public IEnumerable<ushort> GetUpcomingEvents(DateTime earliestTime, DateTime latestTime)
        {
            FastList<EventData> events = EventManager.instance.m_events;
            for (ushort i = 0; i < events.m_size && i < EventManager.MAX_EVENT_COUNT; ++i)
            {
                EventData eventData = events.m_buffer[i];
                if ((eventData.m_flags & (EventData.Flags.Preparing | EventData.Flags.Ready | EventData.Flags.Active)) == 0)
                {
                    continue;
                }

                if (eventData.StartTime >= earliestTime && eventData.StartTime < latestTime)
                {
                    yield return i;
                }
            }
        }

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

            EventData eventData = EventManager.instance.m_events.m_buffer[eventId];
            buildingId = eventData.m_building;
            startTime = eventData.StartTime;
            duration = eventData.Info.m_eventAI.m_eventDuration;
            ticketPrice = eventData.m_ticketPrice / 100f;
            return true;
        }
    }
}
