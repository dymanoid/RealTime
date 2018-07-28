// <copyright file="DayTimeCalculator.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.Simulation
{
    using System;

    /// <summary>
    /// Calculates the sunrise and sunset time based on the map latitude and current date.
    /// </summary>
    internal sealed class DayTimeCalculator
    {
        private readonly float phase;
        private readonly float halfAmplitude;

        /// <summary>
        /// Initializes a new instance of the <see cref="DayTimeCalculator"/> class.
        /// </summary>
        ///
        /// <param name="latitude">The latitude coordinate (in degrees) to perform the calculation for.
        /// Valid values are -80..+80.</param>
        public DayTimeCalculator(float latitude)
        {
            bool southSemisphere = latitude < 0;

            latitude = Math.Abs(latitude);
            if (latitude > 80f)
            {
                latitude = 80f;
            }

            halfAmplitude = (0.5f + (latitude / 15f)) / 2f;
            phase = southSemisphere ? 0f : (float)Math.PI;
        }

        /// <summary>
        /// Calculates the sunrise and sunset hours for the specified <paramref name="date"/>.
        /// </summary>
        ///
        /// <param name="date">The game date to calculate the sunrise and sunset times for.</param>
        /// <param name="sunriseHour">The calculated sunrise hour (relative to the midnight).</param>
        /// <param name="sunsetHour">The calculated sunset hour (relative to the midnight).</param>
        public void Calculate(DateTime date, out float sunriseHour, out float sunsetHour)
        {
            float modifier = (float)Math.Cos((2 * Math.PI * (date.DayOfYear + 10) / 365.25) + phase);
            sunriseHour = 5.5f - (halfAmplitude * modifier);
            sunsetHour = 20.5f + (halfAmplitude * modifier);
        }
    }
}
