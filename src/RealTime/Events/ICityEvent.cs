// <copyright file="ICityEvent.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.Events
{
    using System;

    internal interface ICityEvent
    {
        DateTime StartTime { get; }

        DateTime EndTime { get; }

        ushort BuildingId { get; }

        string BuildingName { get; }

        void Configure(ushort buildingId, string buildingName, DateTime startTime);

        bool AcceptsAttendees();

        void AcceptAttendee();
    }
}