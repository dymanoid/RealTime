// <copyright file="WeatherInfo.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.CustomAI
{
    using System;
    using RealTime.GameConnection;
    using RealTime.Simulation;
    using RealTime.Tools;
    using static Constants;

    /// <summary>
    /// The default <see cref="IWeatherInfo"/> implementation.
    /// </summary>
    /// <seealso cref="IWeatherInfo" />
    internal sealed class WeatherInfo : IWeatherInfo
    {
        private readonly IWeatherManagerConnection weatherManager;

        private uint stayInsideChance;
        private bool isDisasterHazardActive;

        /// <summary>Initializes a new instance of the <see cref="WeatherInfo"/> class.</summary>
        /// <param name="weatherManager">The game's weather manager.</param>
        /// <exception cref="ArgumentNullException">Thrown when the argument is null.</exception>
        public WeatherInfo(IWeatherManagerConnection weatherManager)
        {
            this.weatherManager = weatherManager ?? throw new ArgumentNullException(nameof(weatherManager));
        }

        /// <summary>
        /// Gets a probability (0-100) that the citizens will stay inside buildings due to weather conditions.
        /// </summary>
        uint IWeatherInfo.StayInsideChance => stayInsideChance;

        /// <summary>
        /// Gets a value indicating whether a disaster hazard is currently active in the city.
        /// </summary>
        bool IWeatherInfo.IsDisasterHazardActive => isDisasterHazardActive;

        /// <summary>Updates this object's state using the current game state.</summary>
        public void Update()
        {
            float weatherFactor = weatherManager.GetRainIntensity() + (weatherManager.GetSnowIntensity() / 2f);

            if (weatherFactor < BadWeatherPrecipitationThreshold)
            {
                stayInsideChance = 0u;
                return;
            }

            stayInsideChance = MinimumStayInsideChanceOnPrecipitation +
                    (uint)((weatherFactor - BadWeatherPrecipitationThreshold)
                    * ((100u - MinimumStayInsideChanceOnPrecipitation) / (1f - BadWeatherPrecipitationThreshold)));

            UpdateDisasterHazard();
        }

        private void UpdateDisasterHazard()
        {
            if (DisasterManager.instance.m_disasterCount == 0)
            {
                Log.DebugIf(isDisasterHazardActive, "A disaster ended.");
                isDisasterHazardActive = false;
                return;
            }

            for (int i = 1; i < DisasterManager.instance.m_disasters.m_size; i++)
            {
                ref DisasterData disaster = ref DisasterManager.instance.m_disasters.m_buffer[i];

                if ((disaster.m_flags & DisasterData.Flags.Significant) == 0
                    || (disaster.m_flags & (DisasterData.Flags.Active | DisasterData.Flags.Located)) == 0)
                {
                    continue;
                }

                Log.DebugIf(!isDisasterHazardActive, "An active disaster has been detected!");
                isDisasterHazardActive = true;
                return;
            }

            Log.DebugIf(isDisasterHazardActive, "A disaster ended.");
            isDisasterHazardActive = false;
        }
    }
}
