// <copyright file="IRealTimeEvent.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.Events
{
    using System;

    internal interface IRealTimeEvent
    {
        DateTime StartTime { get; }

        DateTime EndTime { get; }

        ushort BuildingId { get; }

        string BuildingName { get; }

        void Configure(ushort buildingId, string buildingName, DateTime startTime);

        bool CanAttend();

        void Attend();
    }
}