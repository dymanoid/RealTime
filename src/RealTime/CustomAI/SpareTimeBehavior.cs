// <copyright file="SpareTimeBehavior.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.CustomAI
{
    using System;
    using RealTime.Config;
    using RealTime.Simulation;
    using RealTime.Tools;

    /// <summary>
    /// A class that provides custom logic for the spare time simulation.
    /// </summary>
    internal sealed class SpareTimeBehavior
    {
        private readonly RealTimeConfig config;
        private readonly ITimeInfo timeInfo;
        private readonly uint[] chances;
        private float simulationCycle;

        /// <summary>Initializes a new instance of the <see cref="SpareTimeBehavior"/> class.</summary>
        /// <param name="config">The configuration to run with.</param>
        /// <param name="timeInfo">The object providing the game time information.</param>
        /// <exception cref="ArgumentNullException">Thrown when any argument is null.</exception>
        public SpareTimeBehavior(RealTimeConfig config, ITimeInfo timeInfo)
        {
            this.config = config ?? throw new ArgumentNullException(nameof(config));
            this.timeInfo = timeInfo ?? throw new ArgumentNullException(nameof(timeInfo));
            chances = new uint[Enum.GetValues(typeof(Citizen.AgeGroup)).Length];
        }

        /// <summary>Sets the duration (in hours) of a full simulation cycle for all citizens.
        /// The game calls the simulation methods for a particular citizen with this period.</summary>
        /// <param name="cyclePeriod">The citizens simulation cycle period, in game hours.</param>
        public void SetSimulationCyclePeriod(float cyclePeriod)
        {
            simulationCycle = cyclePeriod;
        }

        /// <summary>Calculates the chances for the citizens to go out based on the current game time.</summary>
        public void RefreshGoOutChances()
        {
            uint weekdayModifier;
            if (config.IsWeekendEnabled)
            {
                weekdayModifier = timeInfo.Now.IsWeekendTime(12f, config.GoToSleepUpHour)
                    ? 11u
                    : 1u;
            }
            else
            {
                weekdayModifier = 1u;
            }

            float currentHour = timeInfo.CurrentHour;

            float latestGoOutHour = config.GoToSleepUpHour - simulationCycle;
            bool isDayTime = currentHour >= config.WakeupHour && currentHour < latestGoOutHour;
            float timeModifier;
            if (isDayTime)
            {
                timeModifier = RealTimeMath.Clamp((currentHour - config.WakeupHour) * 2f, 0, 4f);
            }
            else
            {
                float nightDuration = 24f - (latestGoOutHour - config.WakeupHour);
                float relativeHour = currentHour - latestGoOutHour;
                if (relativeHour < 0)
                {
                    relativeHour += 24f;
                }

                timeModifier = 3f / nightDuration * (nightDuration - relativeHour);
            }

            uint defaultChance = (uint)((timeModifier + weekdayModifier) * timeModifier);

            bool dump = chances[(int)Citizen.AgeGroup.Young] != defaultChance;

            chances[(int)Citizen.AgeGroup.Child] = isDayTime ? defaultChance : 0;
            chances[(int)Citizen.AgeGroup.Teen] = isDayTime ? defaultChance : 0;
            chances[(int)Citizen.AgeGroup.Young] = defaultChance;
            chances[(int)Citizen.AgeGroup.Adult] = defaultChance;
            chances[(int)Citizen.AgeGroup.Senior] = defaultChance;

            if (dump)
            {
                Log.Debug($"GO OUT CHANCES for {timeInfo.Now}: child = {chances[0]}, teen = {chances[1]}, young = {chances[2]}, adult = {chances[3]}, senior = {chances[4]}");
            }
        }

        /// <summary>
        /// Gets the probability whether a citizen with provided age would go out on current time.
        /// </summary>
        ///
        /// <param name="citizenAge">The citizen age to check.</param>
        ///
        /// <returns>A percentage value in range of 0..100 that describes the probability whether
        /// a citizen with provided age would go out on current time.</returns>
        public uint GetGoOutChance(Citizen.AgeGroup citizenAge)
        {
            return chances[(int)citizenAge];
        }
    }
}
