// <copyright file="TimeInfo.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.GameConnection
{
    using System;
    using RealTime.Config;
    using RealTime.Simulation;

    /// <summary>
    /// The default implementation of the <see cref="ITimeInfo"/> interface.
    /// </summary>
    /// <seealso cref="ITimeInfo" />
    internal sealed class TimeInfo : ITimeInfo
    {
        private readonly RealTimeConfig config;

        /// <summary>Initializes a new instance of the <see cref="TimeInfo" /> class.</summary>
        /// <param name="config">The configuration to run with.</param>
        /// <exception cref="ArgumentNullException">Thrown when the argument is null.</exception>
        public TimeInfo(RealTimeConfig config)
        {
            this.config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>Gets the current game date and time.</summary>
        public DateTime Now => SimulationManager.instance.m_currentGameTime;

        /// <summary>Gets the current daytime hour.</summary>
        public float CurrentHour => (float)Now.TimeOfDay.TotalHours;

        /// <summary>Gets the sunrise hour of the current day.</summary>
        public float SunriseHour => SimulationManager.SUNRISE_HOUR;

        /// <summary>Gets the sunset hour of the current day.</summary>
        public float SunsetHour => SimulationManager.SUNSET_HOUR;

        /// <summary>Gets a value indicating whether the current time represents a night hour.</summary>
        public bool IsNightTime
        {
            get
            {
                float currentHour = CurrentHour;
                return currentHour >= config.GoToSleepUpHour || currentHour < config.WakeupHour;
            }
        }

        /// <summary>Gets the duration of the current or last day.</summary>
        public float DayDuration => SunsetHour - SunriseHour;

        /// <summary>Gets the duration of the current or last night.</summary>
        public float NightDuration => 24f - DayDuration;
    }
}
