// <copyright file="EventManagerConnection.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.GameConnection
{
    internal sealed class EventManagerConnection : IEventManagerConnection
    {
        public EventData.Flags GetEventFlags(ushort eventId)
        {
            return eventId == 0
                ? EventData.Flags.None
                : EventManager.instance.m_events.m_buffer[eventId].m_flags;
        }
    }
}
