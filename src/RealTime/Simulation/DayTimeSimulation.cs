// <copyright file="DayTimeSimulation.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.Simulation
{
    using System;

    /// <summary>
    /// A class that performs the simulation of real daytime and applies the values
    /// to the <see cref="SimulationManager"/>.
    /// </summary>
    internal sealed class DayTimeSimulation
    {
        private DayTimeCalculator calculator;

        /// <summary>
        /// Processes the simulation for the specified date.
        /// </summary>
        ///
        /// <param name="date">The date to perform the simulation for.</param>
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

            SimulationManager.RELATIVE_DAY_LENGTH = (int)((sunsetHour - sunriseHour) * 100 / 24);
            SimulationManager.RELATIVE_NIGHT_LENGTH = 100 - SimulationManager.RELATIVE_DAY_LENGTH;
        }
    }
}
