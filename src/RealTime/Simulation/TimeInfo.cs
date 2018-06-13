// <copyright file="TimeInfo.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.Simulation
{
    using System;

    internal sealed class TimeInfo : ITimeInfo
    {
        public DateTime Now => SimulationManager.instance.m_currentGameTime;

        public float CurrentHour => SimulationManager.instance.m_currentDayTimeHour;

        public float SunriseHour => SimulationManager.SUNRISE_HOUR;

        public float SunsetHour => SimulationManager.SUNSET_HOUR;
    }
}
