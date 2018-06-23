// <copyright file="DaylightTimeSimulation.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.Simulation
{
    using System;
    using ICities;

    /// <summary>
    /// A simulation extension that manages the daytime calculation (sunrise and sunset).
    /// </summary>
    public sealed class DaylightTimeSimulation : ThreadingExtensionBase
    {
        private readonly DayTime dayTime;
        private DateTime lastCalculation;

        /// <summary>
        /// Initializes a new instance of the <see cref="DaylightTimeSimulation"/> class.
        /// </summary>
        public DaylightTimeSimulation()
        {
            dayTime = new DayTime();
        }

        /// <summary>
        /// Called after each game simulation tick. Performs the actual work.
        /// </summary>
        public override void OnAfterSimulationTick()
        {
            if (!dayTime.IsReady)
            {
                if (DayNightProperties.instance != null)
                {
                    dayTime.Setup(DayNightProperties.instance.m_Latitude);
                }
                else
                {
                    return;
                }
            }

            DateTime currentDate = SimulationManager.instance.m_currentGameTime.Date;
            if (currentDate != lastCalculation)
            {
                CalculateDaylight(currentDate);
            }
        }

        private void CalculateDaylight(DateTime date)
        {
            if (!dayTime.Calculate(date, out float sunriseHour, out float sunsetHour))
            {
                return;
            }

            SimulationManager.SUNRISE_HOUR = sunriseHour;
            SimulationManager.SUNSET_HOUR = sunsetHour;
            lastCalculation = date.Date;
        }
    }
}
