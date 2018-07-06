// <copyright file="DayTimeSimulation.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.Simulation
{
    using System;
    using RealTime.Config;

    /// <summary>
    /// A class that performs the simulation of real daytime and applies the values
    /// to the <see cref="SimulationManager"/>.
    /// </summary>
    internal sealed class DayTimeSimulation
    {
        private readonly float vanillaSunrise;
        private readonly float vanillaSunset;
        private readonly RealTimeConfig config;

        private DayTimeCalculator calculator;

        /// <summary>Initializes a new instance of the <see cref="DayTimeSimulation"/> class.</summary>
        /// <param name="config">The configuration to run with.</param>
        /// <exception cref="ArgumentNullException">Thrown when the argument is null.</exception>
        public DayTimeSimulation(RealTimeConfig config)
        {
            vanillaSunrise = SimulationManager.SUNRISE_HOUR;
            vanillaSunset = SimulationManager.SUNSET_HOUR;
            this.config = config ?? throw new ArgumentNullException(nameof(config));
        }

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

            float sunriseHour, sunsetHour;
            if (config.IsDynamicDayLengthEnabled)
            {
                calculator.Calculate(date, out sunriseHour, out sunsetHour);
            }
            else
            {
                if (SimulationManager.SUNRISE_HOUR == vanillaSunrise && SimulationManager.SUNSET_HOUR == vanillaSunset)
                {
                    return;
                }

                sunriseHour = vanillaSunrise;
                sunsetHour = vanillaSunset;
            }

            SimulationManager.SUNRISE_HOUR = sunriseHour;
            SimulationManager.SUNSET_HOUR = sunsetHour;

            SimulationManager.RELATIVE_DAY_LENGTH = (int)(100f * (sunsetHour - sunriseHour) / 24f);
            SimulationManager.RELATIVE_NIGHT_LENGTH = 100 - SimulationManager.RELATIVE_DAY_LENGTH;
        }
    }
}
