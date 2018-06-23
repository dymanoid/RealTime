// <copyright file="CityEventBase.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.Events
{
    using System;

    internal abstract class CityEventBase : ICityEvent
    {
        public DateTime StartTime { get; private set; }

        public DateTime EndTime => StartTime.AddHours(GetDuration());

        public ushort BuildingId { get; private set; }

        public string BuildingName { get; private set; }

        public virtual void AcceptAttendee()
        {
        }

        public virtual bool AcceptsAttendees()
        {
            return true;
        }

        public void Configure(ushort buildingId, string buildingName, DateTime startTime)
        {
            BuildingId = buildingId;
            BuildingName = buildingName ?? string.Empty;
            StartTime = startTime;
        }

        protected abstract float GetDuration();
    }
}
