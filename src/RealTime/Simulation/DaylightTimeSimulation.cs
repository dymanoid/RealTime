// <copyright file="DaylightTimeSimulation.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.Simulation
{
    using System;
    using ICities;
    using RealTime.Tools;

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

            if (threadingManager.simulationTime.Date != lastCalculation)
            {
                CalculateDaylight(threadingManager.simulationTime);
            }
        }

        private void CalculateDaylight(DateTime date)
        {
            if (!dayTime.Calculate(date, out TimeSpan sunriseHour, out TimeSpan sunsetHour))
            {
                return;
            }

            SimulationManager.SUNRISE_HOUR = (float)sunriseHour.TotalHours;
            SimulationManager.SUNSET_HOUR = (float)sunsetHour.TotalHours;

            Log.Info($"New day: {date.Date:d}, sunrise at {sunriseHour.Hours}:{sunriseHour.Minutes:00}, sunset at {sunsetHour.Hours}:{sunsetHour.Minutes:00}");

            lastCalculation = date.Date;
        }
    }
}
