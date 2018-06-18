// <copyright file="IEventManagerConnection.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.GameConnection
{
    internal interface IEventManagerConnection
    {
        EventData.Flags GetEventFlags(ushort eventId);
    }
}
