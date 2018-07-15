// <copyright file="ResidentStateDescriptor.cs" company="dymanoid">Copyright (c) dymanoid. All rights reserved.</copyright>

namespace RealTime.CustomAI
{
    using System;
    using RealTime.Tools;
    using static Constants;

    /// <summary>A container struct that holds information about the detailed resident citizen state.</summary>
    internal struct ResidentStateDescriptor
    {
        /// <summary>The citizen's last known state.</summary>
        public ResidentState State;

        /// <summary>The citizen's current work status.</summary>
        public WorkStatus WorkStatus;

        /// <summary>The time when citizen started their last movement in the city.</summary>
        public DateTime DepartureTime;

        /// <summary>
        /// Gets or sets the travel time (in hours) from citizen's home to the work building. The maximum value is
        /// determined by the <see cref="MaxHoursOnTheWay"/> constant.
        /// </summary>
        public float TravelTimeToWork;

        /// <summary>Updates the travel time that the citizen needs to read the work building or school/university.</summary>
        /// <param name="arrivalTime">
        /// The arrival time at the work building or school/university. Must be great than <see cref="DepartureTime"/>.
        /// </param>
        public void UpdateTravelTimeToWork(DateTime arrivalTime)
        {
            if (arrivalTime < DepartureTime)
            {
                return;
            }

            float onTheWayHours = (float)(arrivalTime - DepartureTime).TotalHours;
            if (onTheWayHours > MaxHoursOnTheWay)
            {
                onTheWayHours = MaxHoursOnTheWay;
            }

            TravelTimeToWork = TravelTimeToWork == 0
                ? onTheWayHours
                : (TravelTimeToWork + onTheWayHours) / 2;
        }
    }
}