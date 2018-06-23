// <copyright file="RealTimeCityEvent.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.Events
{
    using System;
    using RealTime.Events.Storage;

    internal sealed class RealTimeCityEvent : CityEventBase
    {
        private readonly CityEventTemplate eventTemplate;
        private int attendeesCount;

        public RealTimeCityEvent(CityEventTemplate eventTemplate)
        {
            this.eventTemplate = eventTemplate ?? throw new ArgumentNullException(nameof(eventTemplate));
        }

        public RealTimeCityEvent(CityEventTemplate eventTemplate, int attendeesCount)
            : this(eventTemplate)
        {
            this.attendeesCount = attendeesCount;
        }

        public override void AcceptAttendee()
        {
            attendeesCount++;
        }

        public override bool AcceptsAttendees()
        {
            return attendeesCount < eventTemplate.Capacity;
        }

        public RealTimeEventStorage GetStorageData()
        {
            return new RealTimeEventStorage
            {
                EventName = eventTemplate.EventName,
                BuildingClassName = eventTemplate.BuildingClassName,
                StartTime = StartTime.Ticks,
                BuildingId = BuildingId,
                BuildingName = BuildingName,
                AttendeesCount = attendeesCount
            };
        }

        protected override float GetDuration()
        {
            return (float)eventTemplate.Duration;
        }
    }
}
