// <copyright file="WeatherManagerConnection.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.GameConnection
{
    /// <summary>
    /// A default implementation of the <see cref="IWeatherManagerConnection"/> interface.
    /// </summary>
    /// <seealso cref="IWeatherManagerConnection" />
    internal sealed class WeatherManagerConnection : IWeatherManagerConnection
    {
        /// <summary>Gets the current rain intensity (0.0 to 1.0).</summary>
        /// <returns>The rain intensity value from 0 to 1.0.</returns>
        public float GetRainIntensity()
        {
            return WeatherManager.instance.m_properties.m_rainIsSnow
                ? 0f
                : WeatherManager.instance.m_currentRain;
        }

        /// <summary>Gets the current snow intensity (0.0 to 1.0).</summary>
        /// <returns>The snow intensity value from 0 to 1.0.</returns>
        public float GetSnowIntensity()
        {
            return WeatherManager.instance.m_properties.m_rainIsSnow
                ? WeatherManager.instance.m_currentRain
                : 0f;
        }
    }
}
