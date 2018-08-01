// <copyright file="SpareTimeBehavior.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.CustomAI
{
    using System;
    using RealTime.Config;
    using RealTime.Simulation;
    using RealTime.Tools;
    using static Constants;

    /// <summary>
    /// A class that provides custom logic for the spare time simulation.
    /// </summary>
    internal sealed class SpareTimeBehavior : ISpareTimeBehavior
    {
        private readonly RealTimeConfig config;
        private readonly ITimeInfo timeInfo;
        private readonly uint[] defaultChances;
        private readonly uint[] secondShiftChances;
        private readonly uint[] nightShiftChances;
        private readonly uint[] shoppingChances;
        private readonly uint[] vacationChances;

        private float simulationCycle;

        /// <summary>Initializes a new instance of the <see cref="SpareTimeBehavior"/> class.</summary>
        /// <param name="config">The configuration to run with.</param>
        /// <param name="timeInfo">The object providing the game time information.</param>
        /// <exception cref="ArgumentNullException">Thrown when any argument is null.</exception>
        public SpareTimeBehavior(RealTimeConfig config, ITimeInfo timeInfo)
        {
            this.config = config ?? throw new ArgumentNullException(nameof(config));
            this.timeInfo = timeInfo ?? throw new ArgumentNullException(nameof(timeInfo));

            int agesCount = Enum.GetValues(typeof(Citizen.AgeGroup)).Length;
            defaultChances = new uint[agesCount];
            secondShiftChances = new uint[agesCount];
            nightShiftChances = new uint[agesCount];
            shoppingChances = new uint[agesCount];

            vacationChances = new uint[Enum.GetValues(typeof(Citizen.Wealth)).Length];
        }

        /// <summary>Sets the duration (in hours) of a full simulation cycle for all citizens.
        /// The game calls the simulation methods for a particular citizen with this period.</summary>
        /// <param name="cyclePeriod">The citizens simulation cycle period, in game hours.</param>
        public void SetSimulationCyclePeriod(float cyclePeriod)
        {
            simulationCycle = cyclePeriod;
        }

        /// <summary>Calculates the chances for the citizens to go out based on the current game time.</summary>
        public void RefreshChances()
        {
            uint weekdayModifier;
            if (config.IsWeekendEnabled)
            {
                weekdayModifier = timeInfo.Now.IsWeekendTime(12f, config.GoToSleepHour)
                    ? 11u
                    : 1u;
            }
            else
            {
                weekdayModifier = 1u;
            }

            bool isWeekend = weekdayModifier > 1u;
            float currentHour = timeInfo.CurrentHour;

            CalculateDefaultChances(currentHour, weekdayModifier);
            CalculateSecondShiftChances(currentHour, isWeekend);
            CalculateNightShiftChances(currentHour, isWeekend);
            CalculateShoppingChance(currentHour);
            CalculateVacationChances();
        }

        /// <summary>
        /// Gets the probability whether a citizen with specified age would go shopping on current time.
        /// </summary>
        ///
        /// <param name="citizenAge">The age of the citizen to check.</param>
        ///
        /// <returns>A percentage value in range of 0..100 that describes the probability whether
        /// a citizen with specified age would go shopping on current time.</returns>
        public uint GetShoppingChance(Citizen.AgeGroup citizenAge)
        {
            return shoppingChances[(int)citizenAge];
        }

        /// <summary>
        /// Gets the probability whether a citizen with specified age would go relaxing on current time.
        /// </summary>
        ///
        /// <param name="citizenAge">The age of the citizen to check.</param>
        /// <param name="workShift">The citizen's assigned work shift (default is <see cref="WorkShift.Unemployed"/>).</param>
        /// <param name="isOnVacation"><c>true</c> if the citizen is on vacation.</param>
        ///
        /// <returns>A percentage value in range of 0..100 that describes the probability whether
        /// a citizen with specified age would go relaxing on current time.</returns>
        public uint GetRelaxingChance(Citizen.AgeGroup citizenAge, WorkShift workShift = WorkShift.Unemployed, bool isOnVacation = false)
        {
            if (isOnVacation)
            {
                return defaultChances[(int)citizenAge] * 2u;
            }

            int age = (int)citizenAge;
            switch (citizenAge)
            {
                case Citizen.AgeGroup.Young:
                case Citizen.AgeGroup.Adult:
                    switch (workShift)
                    {
                        case WorkShift.Second:
                            return secondShiftChances[age];

                        case WorkShift.Night:
                            return nightShiftChances[age];

                        default:
                            return defaultChances[age];
                    }

                default:
                    return defaultChances[age];
            }
        }

        /// <summary>Gets a precise probability (in percent multiplied by 100) for a citizen with specified
        /// wealth to go on vacation on current day.</summary>
        /// <param name="wealth">The citizen's wealth.</param>
        /// <returns>The precise probability (in percent multiplied by 100) for the citizen to go on vacation
        /// on current day.</returns>
        public uint GetPreciseVacationChance(Citizen.Wealth wealth)
        {
            return vacationChances[(int)wealth];
        }

        private void CalculateDefaultChances(float currentHour, uint weekdayModifier)
        {
            float latestGoingOutHour = config.GoToSleepHour - simulationCycle;
            bool isDayTime = currentHour >= config.WakeUpHour && currentHour < latestGoingOutHour;
            float timeModifier;
            if (isDayTime)
            {
                timeModifier = RealTimeMath.Clamp(currentHour - config.WakeUpHour, 0, 4f);
            }
            else
            {
                float nightDuration = 24f - (latestGoingOutHour - config.WakeUpHour);
                float relativeHour = currentHour - latestGoingOutHour;
                if (relativeHour < 0)
                {
                    relativeHour += 24f;
                }

                timeModifier = 3f / nightDuration * (nightDuration - relativeHour);
            }

            float chance = (timeModifier + weekdayModifier) * timeModifier;
            uint roundedChance = (uint)Math.Round(chance);

#if DEBUG
            bool dump = defaultChances[(int)Citizen.AgeGroup.Adult] != roundedChance;
#endif

            defaultChances[(int)Citizen.AgeGroup.Child] = isDayTime ? roundedChance : 0;
            defaultChances[(int)Citizen.AgeGroup.Teen] = isDayTime ? (uint)Math.Round(chance * 0.9f) : 0;
            defaultChances[(int)Citizen.AgeGroup.Young] = (uint)Math.Round(chance * 1.3f);
            defaultChances[(int)Citizen.AgeGroup.Adult] = roundedChance;
            defaultChances[(int)Citizen.AgeGroup.Senior] = isDayTime ? (uint)Math.Round(chance * 0.8f) : 0;

#if DEBUG
            if (dump)
            {
                Log.Debug(LogCategories.Simulation, $"DEFAULT GOING OUT CHANCES for {timeInfo.Now}: child = {defaultChances[0]}, teen = {defaultChances[1]}, young = {defaultChances[2]}, adult = {defaultChances[3]}, senior = {defaultChances[4]}");
            }
#endif
        }

        private void CalculateSecondShiftChances(float currentHour, bool isWeekend)
        {
#if DEBUG
            uint oldChance = secondShiftChances[(int)Citizen.AgeGroup.Adult];
#endif

            float wakeUpHour = config.WakeUpHour - config.GoToSleepHour + 24f;
            if (isWeekend || currentHour < config.WakeUpHour || currentHour >= wakeUpHour)
            {
                secondShiftChances[(int)Citizen.AgeGroup.Young] = defaultChances[(int)Citizen.AgeGroup.Young];
                secondShiftChances[(int)Citizen.AgeGroup.Adult] = defaultChances[(int)Citizen.AgeGroup.Adult];
            }
            else
            {
                secondShiftChances[(int)Citizen.AgeGroup.Young] = 0;
                secondShiftChances[(int)Citizen.AgeGroup.Adult] = 0;
            }

#if DEBUG
            if (oldChance != secondShiftChances[(int)Citizen.AgeGroup.Adult])
            {
                Log.Debug(LogCategories.Simulation, $"SECOND SHIFT GOING OUT CHANCE for {timeInfo.Now}: young = {secondShiftChances[2]}, adult = {secondShiftChances[3]}");
            }
#endif
        }

        private void CalculateNightShiftChances(float currentHour, bool isWeekend)
        {
#if DEBUG
            uint oldChance = nightShiftChances[(int)Citizen.AgeGroup.Adult];
#endif

            float wakeUpHour = config.WorkBegin + (config.WakeUpHour - config.GoToSleepHour + 24f);
            if (isWeekend || currentHour < config.WakeUpHour || currentHour >= wakeUpHour)
            {
                nightShiftChances[(int)Citizen.AgeGroup.Young] = defaultChances[(int)Citizen.AgeGroup.Young];
                nightShiftChances[(int)Citizen.AgeGroup.Adult] = defaultChances[(int)Citizen.AgeGroup.Adult];
            }
            else
            {
                nightShiftChances[(int)Citizen.AgeGroup.Young] = 0;
                nightShiftChances[(int)Citizen.AgeGroup.Adult] = 0;
            }

#if DEBUG
            if (oldChance != nightShiftChances[(int)Citizen.AgeGroup.Adult])
            {
                Log.Debug(LogCategories.Simulation, $"NIGHT SHIFT GOING OUT CHANCE for {timeInfo.Now}: young = {nightShiftChances[2]}, adult = {nightShiftChances[3]}");
            }
#endif
        }

        private void CalculateShoppingChance(float currentHour)
        {
            float minShoppingChanceEndHour = Math.Min(config.WakeUpHour, EarliestWakeUp);
            float maxShoppingChanceStartHour = Math.Max(config.WorkBegin, config.WakeUpHour);
            if (minShoppingChanceEndHour == maxShoppingChanceStartHour)
            {
                minShoppingChanceEndHour = RealTimeMath.Clamp(maxShoppingChanceStartHour - 1f, 2f, maxShoppingChanceStartHour - 1f);
            }

#if DEBUG
            uint oldChance = shoppingChances[(int)Citizen.AgeGroup.Adult];
#endif

            float chance;
            bool isNight;
            float maxShoppingChanceEndHour = Math.Max(config.GoToSleepHour, config.WorkEnd);
            if (currentHour < minShoppingChanceEndHour)
            {
                isNight = true;
                chance = NightShoppingChance;
            }
            else if (currentHour < maxShoppingChanceStartHour)
            {
                isNight = true;
                chance = NightShoppingChance +
                    ((100u - NightShoppingChance) * (currentHour - minShoppingChanceEndHour) / (maxShoppingChanceStartHour - minShoppingChanceEndHour));
            }
            else if (currentHour < maxShoppingChanceEndHour)
            {
                isNight = false;
                chance = 100;
            }
            else
            {
                isNight = true;
                chance = NightShoppingChance +
                    ((100u - NightShoppingChance) * (24f - currentHour) / (24f - maxShoppingChanceEndHour));
            }

            uint roundedChance = (uint)Math.Round(chance);

            shoppingChances[(int)Citizen.AgeGroup.Child] = isNight ? 0u : roundedChance;
            shoppingChances[(int)Citizen.AgeGroup.Teen] = isNight ? 0u : roundedChance;
            shoppingChances[(int)Citizen.AgeGroup.Young] = roundedChance;
            shoppingChances[(int)Citizen.AgeGroup.Adult] = roundedChance;
            shoppingChances[(int)Citizen.AgeGroup.Senior] = isNight ? (uint)Math.Round(chance * 0.1f) : roundedChance;

#if DEBUG
            if (oldChance != roundedChance)
            {
                Log.Debug(LogCategories.Simulation, $"SHOPPING CHANCES for {timeInfo.Now}: child = {shoppingChances[0]}, teen = {shoppingChances[1]}, young = {shoppingChances[2]}, adult = {shoppingChances[3]}, senior = {shoppingChances[4]}");
            }
#endif
        }

        private void CalculateVacationChances()
        {
            uint baseChance;
            int dayOfYear = timeInfo.Now.DayOfYear;
            if (dayOfYear < 7)
            {
                baseChance = 100u * 8u;
            }
            else if (dayOfYear < 30 * 5)
            {
                baseChance = 100u * 1u;
            }
            else if (dayOfYear < 30 * 9)
            {
                baseChance = 100u * 3u;
            }
            else if (dayOfYear < 352)
            {
                baseChance = 100u * 1u;
            }
            else
            {
                baseChance = 100u * 20u;
            }

#if DEBUG
            bool dump = baseChance != vacationChances[(int)Citizen.Wealth.Medium];
#endif

            vacationChances[(int)Citizen.Wealth.Low] = baseChance / 2;
            vacationChances[(int)Citizen.Wealth.Medium] = baseChance;
            vacationChances[(int)Citizen.Wealth.High] = baseChance * 3 / 2;

#if DEBUG
            if (dump)
            {
                Log.Debug(LogCategories.Simulation, $"VACATION CHANCES for {timeInfo.Now}: low = {vacationChances[0]}, medium = {vacationChances[1]}, high = {vacationChances[2]}");
            }
#endif
        }
    }
}
