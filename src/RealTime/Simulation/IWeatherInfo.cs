// <copyright file="IWeatherInfo.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.Simulation
{
    /// <summary>
    /// An interface for a service that provides the game's weather information.
    /// </summary>
    internal interface IWeatherInfo
    {
        /// <summary>Gets a probability (0-100) that the citizens will stay inside buildings due to weather conditions.</summary>
        uint StayInsideChance { get; }

        /// <summary>Gets a value indicating whether a disaster hazard is currently active in the city.</summary>
        bool IsDisasterHazardActive { get; }
    }
}
