// <copyright file="IEventManagerConnection.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.GameConnection
{
    using System;
    using System.Collections.Generic;

    internal interface IEventManagerConnection
    {
        EventData.Flags GetEventFlags(ushort eventId);

        IEnumerable<ushort> GetUpcomingEvents(DateTime latestTime);

        bool TryGetEventInfo(ushort eventId, out ushort buildingId, out DateTime startTime, out float duration, out float ticketPrice);
    }
}
