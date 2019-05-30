// <copyright file="VanillaEventInfo.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.GameConnection
{
    using System;

    /// <summary>
    /// A ref-struct consolidating the information about a vanilla game event.
    /// </summary>
    internal readonly ref struct VanillaEventInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VanillaEventInfo"/> struct.
        /// </summary>
        /// <param name="buildingId">The ID of the building where the event takes place.</param>
        /// <param name="startTime">The event start date and time.</param>
        /// <param name="duration">The event duration in hours.</param>
        /// <param name="ticketPrice">The ticket price for the event.</param>
        public VanillaEventInfo(ushort buildingId, DateTime startTime, float duration, float ticketPrice)
        {
            BuildingId = buildingId;
            StartTime = startTime;
            Duration = duration;
            TicketPrice = ticketPrice;
        }

        /// <summary>
        /// Gets the ID of the building where the event takes place.
        /// </summary>
        public ushort BuildingId { get; }

        /// <summary>
        /// Gets the event start date and time.
        /// </summary>
        public DateTime StartTime { get; }

        /// <summary>
        /// Gets the event duration in hours.
        /// </summary>
        public float Duration { get; }

        /// <summary>
        /// Gets the ticket price for the event.
        /// </summary>
        public float TicketPrice { get; }
    }
}
