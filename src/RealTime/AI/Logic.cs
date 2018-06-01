// <copyright file="Logic.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.AI
{
    using System;
    using ColossalFramework.Math;
    using RealTime.Config;
    using RealTime.Tools;
    using UnityEngine;

    /// <summary>
    /// The <see cref="ILogic"/> implementation. Describes the basic Real Time customized logic for the Cims.
    /// </summary>
    internal sealed class Logic : ILogic
    {
        private readonly Configuration config;
        private readonly Func<DateTime> timeProvider;
        private readonly Randomizer randomizer;

        /// <summary>
        /// Initializes a new instance of the <see cref="Logic"/> class.
        /// </summary>
        ///
        /// <exception cref="ArgumentNullException">Thrown when any argument is null.</exception>
        ///
        /// <param name="config">A <see cref="Configuration"/> instance containing the mod's configuration.</param>
        /// <param name="timeProvider">A method that can provide the current game date and time.</param>
        /// <param name="randomizer">A <see cref="Randomizer"/> instance to use for randomization.</param>
        public Logic(Configuration config,  Func<DateTime> timeProvider, Randomizer randomizer)
        {
            this.config = config ?? throw new ArgumentNullException(nameof(config));
            this.timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
            this.randomizer = randomizer;
        }

        /// <summary>
        /// Gets a value indicating whether the current game time represents a weekend (no work on those days).
        /// Cims have free time.
        /// </summary>
        public bool IsWeekend => config.EnableWeekends && timeProvider().IsWeekend();

        /// <summary>
        /// Gets a value indicating whether the current game time represents a work day.
        /// Cims go to work.
        /// </summary>
        public bool IsWorkDay => !config.EnableWeekends || !timeProvider().IsWeekend();

        /// <summary>
        /// Gets a value indicating whether the current game time represents a work hour.
        /// Cims are working.
        /// </summary>
        public bool IsWorkHour => IsWorkDayAndBetweenHours(config.MinWorkHour, config.EndWorkHour);

        /// <summary>
        /// Gets a value indicating whether the current game time represents a school hour.
        /// Students are studying.
        /// </summary>
        public bool IsSchoolHour => IsWorkDayAndBetweenHours(config.MinSchoolHour, config.EndSchoolHour);

        /// <summary>
        /// Gets a value indicating whether the current game time represents a lunch hour.
        /// Cims are going out for lunch.
        /// </summary>
        public bool IsLunchHour => IsWorkDayAndBetweenHours(config.LunchBegin, config.LunchEnd);

        // Hours to attempt to go to school, if not already at school. Don't want them travelling only to go home straight away
        private float MaxSchoolAttemptHour => config.EndSchoolHour - config.MinSchoolDuration;

        // Hours to attempt to go to work, if not already at work. Don't want them travelling only to go home straight away
        private float MaxWorkAttemptHour => config.EndWorkHour - config.MinWorkDuration;

        /// <summary>
        /// Determines whether the provided <paramref name="citizen"/> should go to work.
        /// </summary>
        ///
        /// <param name="citizen">The <see cref="Citizen"/> to check.</param>
        /// <param name="ignoreMinimumDuration">True to ignore the time constraints for the minimum work duration.</param>
        ///
        /// <returns>True if the <paramref name="citizen"/> should go go to work; otherwise, false.</returns>
        public bool ShouldGoToWork(ref Citizen citizen, bool ignoreMinimumDuration = false)
        {
            if (!CanWorkOrStudyToday(ref citizen))
            {
                return false;
            }

            float currentHour = timeProvider().HourOfDay();

            switch (Citizen.GetAgeGroup(citizen.Age))
            {
                case Citizen.AgeGroup.Child:
                case Citizen.AgeGroup.Teen:
                    if (currentHour > config.MinSchoolHour - config.WorkOrSchoolTravelTime
                        && currentHour < config.StartSchoolHour - config.WorkOrSchoolTravelTime)
                    {
                        return randomizer.Int32(100) < config.EarlyStartQuota * 100;
                    }
                    else if (currentHour >= config.StartSchoolHour - config.WorkOrSchoolTravelTime
                        && currentHour < (ignoreMinimumDuration ? config.EndSchoolHour : MaxSchoolAttemptHour))
                    {
                        return true;
                    }

                    break;

                case Citizen.AgeGroup.Young:
                case Citizen.AgeGroup.Adult:
                    if (currentHour > config.MinWorkHour - config.WorkOrSchoolTravelTime
                        && currentHour < config.StartWorkHour - config.WorkOrSchoolTravelTime)
                    {
                        return randomizer.Int32(100) < config.EarlyStartQuota * 100;
                    }
                    else if (currentHour >= config.StartWorkHour - config.WorkOrSchoolTravelTime
                        && currentHour < (ignoreMinimumDuration ? config.EndWorkHour : MaxWorkAttemptHour))
                    {
                        return true;
                    }

                    break;
            }

            return false;
        }

        /// <summary>
        /// Determines whether the provided <paramref name="citizen"/> should return back from work.
        /// </summary>
        ///
        /// <param name="citizen">The <see cref="Citizen"/> to check.</param>
        ///
        /// <returns>True if the <paramref name="citizen"/> should return back from work; otherwise, false.</returns>
        public bool ShouldReturnFromWork(ref Citizen citizen)
        {
            if (IsWeekend)
            {
                return true;
            }

            float currentHour = timeProvider().HourOfDay();

            switch (Citizen.GetAgeGroup(citizen.Age))
            {
                case Citizen.AgeGroup.Child:
                case Citizen.AgeGroup.Teen:
                    if (currentHour >= config.EndSchoolHour && currentHour < config.MaxSchoolHour)
                    {
                        return randomizer.Int32(100) < config.LeaveOnTimeQuota * 100;
                    }
                    else if (currentHour > config.MaxSchoolHour || currentHour < config.MinSchoolHour)
                    {
                        return true;
                    }

                    break;

                case Citizen.AgeGroup.Young:
                case Citizen.AgeGroup.Adult:
                    if (currentHour >= config.EndWorkHour && currentHour < config.MaxWorkHour)
                    {
                        return randomizer.Int32(100) < config.LeaveOnTimeQuota * 100;
                    }
                    else if (currentHour > config.MaxWorkHour || currentHour < config.MinWorkHour)
                    {
                        return true;
                    }

                    break;

                default:
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Determines whether the provided <paramref name="citizen"/> should go out for lunch.
        /// </summary>
        ///
        /// <param name="citizen">The <see cref="Citizen"/> to check.</param>
        ///
        /// <returns>True if the <paramref name="citizen"/> should go out for lunch; otherwise, false.</returns>
        public bool ShouldGoToLunch(ref Citizen citizen)
        {
            if (!config.SimulateLunchTime)
            {
                return false;
            }

            switch (Citizen.GetAgeGroup(citizen.Age))
            {
                case Citizen.AgeGroup.Child:
                    return false;

                case Citizen.AgeGroup.Teen:
                case Citizen.AgeGroup.Young:
                case Citizen.AgeGroup.Adult:
                case Citizen.AgeGroup.Senior:
                    break;
            }

            float currentHour = timeProvider().HourOfDay();
            if (currentHour > config.LunchBegin && currentHour < config.LunchEnd)
            {
                return randomizer.Int32(100) < config.LunchQuota * 100;
            }

            return false;
        }

        /// <summary>
        /// Determines whether the provided <paramref name="citizen"/> should find some entertainment.
        /// </summary>
        ///
        /// <param name="citizen">The <see cref="Citizen"/> to check.</param>
        ///
        /// <returns>True if the <paramref name="citizen"/> should find entertainment; otherwise, false.</returns>
        public bool ShouldFindEntertainment(ref Citizen citizen)
        {
            float dayHourStart = 7f;
            float dayHourEnd = 20f;

            switch (Citizen.GetAgeGroup(citizen.Age))
            {
                case Citizen.AgeGroup.Child:
                    dayHourStart = 8f;
                    dayHourEnd = 18f;
                    break;

                case Citizen.AgeGroup.Teen:
                    dayHourStart = 10f;
                    dayHourEnd = 20f;
                    break;

                case Citizen.AgeGroup.Young:
                case Citizen.AgeGroup.Adult:
                    dayHourStart = 7f;
                    dayHourEnd = 20f;
                    break;

                case Citizen.AgeGroup.Senior:
                    dayHourStart = 6f;
                    dayHourEnd = 18f;
                    break;
            }

            float currentHour = timeProvider().HourOfDay();
            if (currentHour > dayHourStart && currentHour < dayHourEnd)
            {
                return randomizer.Int32(100) < GetGoOutThroughDayChance(citizen.Age) * 100;
            }
            else
            {
                return randomizer.Int32(100) < GetGoOutAtNightChance(citizen.Age) * 100;
            }
        }

        /// <summary>
        /// Determines whether the provided <paramref name="citizen"/> can stay out.
        /// </summary>
        ///
        /// <param name="citizen">The <see cref="Citizen"/> to check.</param>
        ///
        /// <returns>True if the <paramref name="citizen"/> can stay out; otherwise, false.</returns>
        public bool CanStayOut(ref Citizen citizen)
        {
            float currentHour = timeProvider().HourOfDay();
            return (currentHour >= 7f && currentHour < 20f) || GetGoOutAtNightChance(citizen.Age) > 0;
        }

        private bool IsWorkDayAndBetweenHours(float fromInclusive, float toExclusive)
        {
            float currentHour = timeProvider().HourOfDay();
            return IsWorkDay && (currentHour >= fromInclusive && currentHour < toExclusive);
        }

        private bool CanWorkOrStudyToday(ref Citizen citizen)
        {
            return citizen.m_workBuilding != 0 && IsWorkDay;
        }

        private bool IsAfterLunchTime(float afterLunchHours)
        {
            return IsWorkDay && timeProvider().HourOfDay() < (config.LunchEnd + afterLunchHours);
        }

        private float GetGoOutAtNightChance(int age)
        {
            float currentHour = timeProvider().HourOfDay();
            int weekendMultiplier = IsWeekend && timeProvider().IsWeekendAfter(TimeSpan.FromHours(12))
                ? currentHour > 12f
                    ? 6
                    : Mathf.RoundToInt(Mathf.Clamp(-((currentHour * 1.5f) - 6f), 0f, 6f))
                : 1;

            switch (Citizen.GetAgeGroup(age))
            {
                case Citizen.AgeGroup.Child:
                case Citizen.AgeGroup.Senior:
                    return 0;

                case Citizen.AgeGroup.Teen:
                    return 0.1f * weekendMultiplier;

                case Citizen.AgeGroup.Young:
                    return 0.08f * weekendMultiplier;

                case Citizen.AgeGroup.Adult:
                    return 0.02f * weekendMultiplier;

                default:
                    return 0;
            }
        }

        private float GetGoOutThroughDayChance(int age)
        {
            int weekendMultiplier = IsWeekend && timeProvider().IsWeekendAfter(TimeSpan.FromHours(12))
                ? 4
                : 1;

            switch (Citizen.GetAgeGroup(age))
            {
                case Citizen.AgeGroup.Child:
                    return 0.6f;

                case Citizen.AgeGroup.Teen:
                    return 0.13f * weekendMultiplier;

                case Citizen.AgeGroup.Young:
                    return 0.12f * weekendMultiplier;

                case Citizen.AgeGroup.Adult:
                    return 0.1f * weekendMultiplier;

                case Citizen.AgeGroup.Senior:
                    return 0.06f;

                default:
                    return 0;
            }
        }
    }
}
