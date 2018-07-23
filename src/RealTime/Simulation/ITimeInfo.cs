// <copyright file="ITimeInfo.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.Simulation
{
    using System;

    /// <summary>
    /// An interface for the current game time and date information.
    /// </summary>
    internal interface ITimeInfo
    {
        /// <summary>
        /// Gets the current game date and time.
        /// </summary>
        DateTime Now { get; }

        /// <summary>
        /// Gets the current daytime hour.
        /// </summary>
        float CurrentHour { get; }

        /// <summary>
        /// Gets the sunrise hour of the current day.
        /// </summary>
        float SunriseHour { get; }

        /// <summary>
        /// Gets the sunset hour of the current day.
        /// </summary>
        float SunsetHour { get; }

        /// <summary>
        /// Gets a value indicating whether the current time represents a night hour.
        /// </summary>
        bool IsNightTime { get; }

        /// <summary>
        /// Gets the duration of the current or last day.
        /// </summary>
        float DayDuration { get; }

        /// <summary>
        /// Gets the duration of the current or last night.
        /// </summary>
        float NightDuration { get; }

        /// <summary>Gets the number of hours that fit into one simulation frame.</summary>
        float HoursPerFrame { get; }
    }
}
