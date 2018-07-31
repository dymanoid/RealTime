// <copyright file="CitizenSchedule.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.CustomAI
{
    using System;
    using static Constants;

    /// <summary>A container struct that holds information about the detailed resident citizen state.
    /// Note that this struct is intentionally made mutable to increase performance.</summary>
    internal struct CitizenSchedule
    {
        /// <summary>The size of the buffer in bytes to store the data.</summary>
        public const int DataRecordSize = 6;

        /// <summary>The citizen's current state.</summary>
        public ResidentState CurrentState;

        /// <summary>The citizen's schedule hint.</summary>
        public ScheduleHint Hint;

        /// <summary>The ID of the building where an event takes place, if the citizen schedules to attend one.</summary>
        public ushort EventBuilding;

        /// <summary>The citizen's work status.</summary>
        public WorkStatus WorkStatus;

        /// <summary>The ID of the citizen's work building. If it doesn't equal the game's value, the work shift data needs to be updated.</summary>
        public ushort WorkBuilding;

        /// <summary>The time when citizen started their last ride to the work building.</summary>
        public DateTime DepartureToWorkTime;

        private const float TravelTimeMultiplier = ushort.MaxValue / MaxTravelTime;

        /// <summary>Gets the citizen's next scheduled state.</summary>
        public ResidentState ScheduledState { get; private set; }

        /// <summary>Gets the citizen's previous scheduled state.</summary>
        public ResidentState LastScheduledState { get; private set; }

        /// <summary>Gets the time when the citizen will perform the next state change.</summary>
        public DateTime ScheduledStateTime { get; private set; }

        /// <summary>
        /// Gets the travel time (in hours) from citizen's home to the work building. The maximum value is
        /// determined by the <see cref="MaxTravelTime"/> constant.
        /// </summary>
        public float TravelTimeToWork { get; private set; }

        /// <summary>Gets the citizen's work shift.</summary>
        public WorkShift WorkShift { get; private set; }

        /// <summary>Gets the daytime hour when the citizen's work shift starts.</summary>
        public float WorkShiftStartHour { get; private set; }

        /// <summary>Gets the daytime hour when the citizen's work shift ends.</summary>
        public float WorkShiftEndHour { get; private set; }

        /// <summary>Gets a value indicating whether this citizen works on weekends.</summary>
        public bool WorksOnWeekends { get; private set; }

        /// <summary>Updates the travel time that the citizen needs to read the work building or school/university.</summary>
        /// <param name="arrivalTime">
        /// The arrival time at the work building or school/university. Must be great than <see cref="DepartureToWorkTime"/>.
        /// </param>
        public void UpdateTravelTimeToWork(DateTime arrivalTime)
        {
            if (arrivalTime < DepartureToWorkTime || DepartureToWorkTime == default)
            {
                return;
            }

            float onTheWayHours = (float)(arrivalTime - DepartureToWorkTime).TotalHours;
            if (onTheWayHours > MaxTravelTime)
            {
                onTheWayHours = MaxTravelTime;
            }

            TravelTimeToWork = TravelTimeToWork == 0
                ? onTheWayHours
                : (TravelTimeToWork + onTheWayHours) / 2;
        }

        /// <summary>Updates the work shift data for this citizen's schedule.</summary>
        /// <param name="workShift">The citizen's work shift.</param>
        /// <param name="startHour">The work shift start hour.</param>
        /// <param name="endHour">The work shift end hour.</param>
        /// <param name="worksOnWeekends">if <c>true</c>, the citizen works on weekends.</param>
        public void UpdateWorkShift(WorkShift workShift, float startHour, float endHour, bool worksOnWeekends)
        {
            WorkShift = workShift;
            WorkShiftStartHour = startHour;
            WorkShiftEndHour = endHour;
            WorksOnWeekends = worksOnWeekends;
        }

        /// <summary>Schedules next actions for the citizen with a specified action time.</summary>
        /// <param name="nextState">The next scheduled citizen's state.</param>
        /// <param name="nextStateTime">The time when the scheduled state must change.</param>
        public void Schedule(ResidentState nextState, DateTime nextStateTime)
        {
            LastScheduledState = ScheduledState;
            ScheduledState = nextState;
            ScheduledStateTime = nextStateTime;
        }

        /// <summary>Schedules next actions for the citizen with no action time (ASAP).</summary>
        /// <param name="nextState">The next scheduled citizen's state.</param>
        public void Schedule(ResidentState nextState)
        {
            // Note: not calling the overload to avoid additional method call - this method will be called frequently
            LastScheduledState = ScheduledState;
            ScheduledState = nextState;
            ScheduledStateTime = default;
        }

        /// <summary>Writes this instance to the specified target buffer.</summary>
        /// <param name="target">The target buffer. Must have length of <see cref="DataRecordSize"/> elements.</param>
        /// <param name="referenceTime">The reference time (in ticks) to use for time serialization.</param>
        public void Write(byte[] target, long referenceTime)
        {
            target[0] = (byte)(((int)WorkShift & 0xF) + ((int)WorkStatus << 4));
            target[1] = (byte)ScheduledState;

            ushort minutes = ScheduledStateTime == default
                ? (ushort)0
                : (ushort)((ScheduledStateTime.Ticks - referenceTime) / TimeSpan.TicksPerMinute);

            target[2] = (byte)(minutes & 0xFF);
            target[3] = (byte)(minutes >> 8);

            ushort travelTime = (ushort)(TravelTimeToWork * TravelTimeMultiplier);
            target[4] = (byte)(travelTime & 0xFF);
            target[5] = (byte)(travelTime >> 8);
        }

        /// <summary>Reads this instance from the specified source buffer.</summary>
        /// <param name="source">The source buffer. Must have length of <see cref="DataRecordSize"/> elements.</param>
        /// <param name="referenceTime">The reference time (in ticks) to use for time deserialization.</param>
        public void Read(byte[] source, long referenceTime)
        {
            WorkShift = (WorkShift)(source[0] & 0xF);
            WorkStatus = (WorkStatus)(source[0] >> 4);
            ScheduledState = (ResidentState)source[1];

            int minutes = source[2] + (source[3] << 8);
            ScheduledStateTime = minutes == 0
                ? default
                : new DateTime((minutes * TimeSpan.TicksPerMinute) + referenceTime);

            int travelTime = source[4] + (source[5] << 8);
            TravelTimeToWork = travelTime / TravelTimeMultiplier;
        }
    }
}