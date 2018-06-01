// <copyright file="DayTime.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.Simulation
{
    using System;
    using UnityEngine;

    /// <summary>
    /// Calculates the sunrise and sunset time based on the map latitude and current date.
    /// </summary>
    internal sealed class DayTime
    {
        private double phase;
        private double halfAmplitude;

        /// <summary>
        /// Gets a value indicating whether this object has been properly set up
        /// and can be used for the calculations.
        /// </summary>
        public bool IsReady { get; private set; }

        /// <summary>
        /// Sets up this object so that it can correctly perform the sunrise and sunset time
        /// calculations.
        /// </summary>
        ///
        /// <param name="latitude">The latitude coordinate to assume for the city.
        /// Valid values are -80° to 80°. </param>
        public void Setup(float latitude)
        {
            bool southSemisphere = latitude < 0;

            latitude = Mathf.Clamp(Mathf.Abs(latitude), 0f, 80f);
            halfAmplitude = (0.5 + (latitude / 15d)) / 2d;

            phase = southSemisphere ? 0 : Math.PI;

            IsReady = true;
        }

        /// <summary>
        /// Calculates the sunrise and sunset hours for the provided <paramref name="date"/>.
        /// If this object is not properly set up yet (so <see cref="IsReady"/> returns false),
        /// then the out values will be initialized with default empty <see cref="TimeSpan"/>s.
        /// </summary>
        ///
        /// <param name="date">The game date to calculate the sunrise and sunset times for.</param>
        /// <param name="sunriseHour">The calculated sunrise hour (relative to the midnight).</param>
        /// <param name="sunsetHour">The calculated sunset hour (relative to the midnight).</param>
        ///
        /// <returns>True when the values are successfully calculated; otherwise, false.</returns>
        public bool Calculate(DateTime date, out TimeSpan sunriseHour, out TimeSpan sunsetHour)
        {
            if (!IsReady)
            {
                sunriseHour = default;
                sunsetHour = default;
                return false;
            }

            double modifier = Math.Cos((2 * Math.PI * (date.DayOfYear + 10) / 365.25) + phase);
            sunriseHour = TimeSpan.FromHours(6d - (halfAmplitude * modifier));
            sunsetHour = TimeSpan.FromHours(18d + (halfAmplitude * modifier));
            return true;
        }
    }
}
