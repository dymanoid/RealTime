// <copyright file="IWeatherManagerConnection.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.GameConnection
{
    /// <summary>An interface for the game specific logic related to the weather management.</summary>
    internal interface IWeatherManagerConnection
    {
        /// <summary>Gets the current rain intensity (0.0 to 1.0).</summary>
        /// <returns>The rain intensity value from 0 to 1.0.</returns>
        float GetRainIntensity();

        /// <summary>Gets the current snow intensity (0.0 to 1.0).</summary>
        /// <returns>The snow intensity value from 0 to 1.0.</returns>
        float GetSnowIntensity();
    }
}
