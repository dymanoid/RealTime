// <copyright file="ITimeInfo.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.Simulation
{
    using System;

    internal interface ITimeInfo
    {
        DateTime Now { get; }

        float CurrentHour { get; }

        float SunriseHour { get; }

        float SunsetHour { get; }

        bool IsNightTime { get; }

        float DayDuration { get; }

        float NightDuration { get; }
    }
}
