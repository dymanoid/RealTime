// <copyright file="RealTimeEvent.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.Events
{
    using System;
    using RealTime.Events.Storage;

    internal sealed class RealTimeEvent : RealTimeEventBase
    {
        private readonly Event eventInfo;
        private int attendeesCount;

        public RealTimeEvent(Event @event)
        {
            eventInfo = @event ?? throw new ArgumentNullException(nameof(@event));
        }

        public override void Attend()
        {
            attendeesCount++;
        }

        public override bool CanAttend()
        {
            return attendeesCount < eventInfo.Capacity;
        }

        protected override float GetDuration()
        {
            return (float)eventInfo.Length;
        }
    }
}
