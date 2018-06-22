// <copyright file="VanillaEvent.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.Events
{
    internal sealed class VanillaEvent : RealTimeEventBase
    {
        private readonly float duration;

        public VanillaEvent(float duration)
        {
            this.duration = duration;
        }

        public ushort EventId { get; set; }

        protected override float GetDuration()
        {
            return duration;
        }
    }
}
