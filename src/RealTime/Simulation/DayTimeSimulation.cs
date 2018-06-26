// <copyright file="DayTimeSimulation.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.Simulation
{
    using System;

    internal sealed class DayTimeSimulation
    {
        private DayTimeCalculator calculator;

        public void Process(DateTime date)
        {
            if (calculator == null && DayNightProperties.instance != null)
            {
                calculator = new DayTimeCalculator(DayNightProperties.instance.m_Latitude);
            }

            if (calculator == null)
            {
                return;
            }

            calculator.Calculate(date, out float sunriseHour, out float sunsetHour);
            SimulationManager.SUNRISE_HOUR = sunriseHour;
            SimulationManager.SUNSET_HOUR = sunsetHour;
        }
    }
}
