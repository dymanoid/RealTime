// <copyright file="Logic.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.AI
{
    using System;
    using ColossalFramework.Math;
    using RealTime.Config;
    using RealTime.Simulation;
    using RealTime.Tools;
    using UnityEngine;

    /// <summary>
    /// The <see cref="ILogic"/> implementation. Describes the basic Real Time customized logic for the Cims.
    /// </summary>
    internal sealed class Logic : ILogic
    {
        private readonly Configuration config;
        private readonly ITimeInfo timeInfo;
        private readonly Randomizer randomizer;

        /// <summary>
        /// Initializes a new instance of the <see cref="Logic"/> class.
        /// </summary>
        ///
        /// <exception cref="ArgumentNullException">Thrown when any argument is null.</exception>
        ///
        /// <param name="config">A <see cref="Configuration"/> instance containing the mod's configuration.</param>
        /// <param name="timeInfo">An object implementing the <see cref="ITimeInfo"/> interface that provides
        /// the current game date and time information.</param>
        /// <param name="randomizer">A <see cref="Randomizer"/> instance to use for randomization.</param>
        public Logic(Configuration config,  ITimeInfo timeInfo, ref Randomizer randomizer)
        {
            this.config = config ?? throw new ArgumentNullException(nameof(config));
            this.timeInfo = timeInfo ?? throw new ArgumentNullException(nameof(timeInfo));
            this.randomizer = randomizer;
        }

        /// <summary>
        /// Gets a value indicating whether the current game time represents a weekend (no work on those days).
        /// Cims have free time.
        /// </summary>
        public bool IsWeekend => config.IsWeekendEnabled && timeInfo.Now.IsWeekend();

        /// <summary>
        /// Gets a value indicating whether the current game time represents a work day.
        /// Cims go to work.
        /// </summary>
        public bool IsWorkDay => !config.IsWeekendEnabled || !timeInfo.Now.IsWeekend();

        /// <summary>
        /// Gets a value indicating whether the current game time represents a work hour.
        /// Cims are working.
        /// </summary>
        public bool IsWorkHour => IsWorkDayAndBetweenHours(config.WorkBegin, config.WorkEnd);

        /// <summary>
        /// Gets a value indicating whether the current game time represents a school hour.
        /// Students are studying.
        /// </summary>
        public bool IsSchoolHour => IsWorkDayAndBetweenHours(config.SchoolBegin, config.SchoolEnd);

        /// <summary>
        /// Gets a value indicating whether the current game time represents a lunch hour.
        /// Cims are going out for lunch.
        /// </summary>
        public bool IsLunchHour => IsWorkDayAndBetweenHours(config.LunchBegin, config.LunchEnd);

        /// <summary>
        /// Gets a reference to the current logic configuration.
        /// </summary>
        public Configuration CurrentConfig => config;

        /// <summary>
        /// Determines whether the provided <paramref name="citizen"/> should go to school or to work.
        /// </summary>
        ///
        /// <param name="citizen">The <see cref="Citizen"/> to check.</param>
        /// <param name="buildingManager">A reference to a <see cref="BuildingManager"/> instance.</param>
        ///
        /// <returns>True if the <paramref name="citizen"/> should go go to work; otherwise, false.</returns>
        public bool ShouldGoToSchoolOrWork(ref Citizen citizen, BuildingManager buildingManager)
        {
            if (!CanWorkOrStudyToday(ref citizen))
            {
                return false;
            }

            float currentHour = timeInfo.CurrentHour;
            float leaveHomeHour;
            float returnHomeHour;

            switch (Citizen.GetAgeGroup(citizen.Age))
            {
                case Citizen.AgeGroup.Child:
                case Citizen.AgeGroup.Teen:
                    leaveHomeHour = config.SchoolBegin;
                    returnHomeHour = config.SchoolEnd;
                    break;

                case Citizen.AgeGroup.Young:
                case Citizen.AgeGroup.Adult:
                    leaveHomeHour = randomizer.Int32(100) < config.OnTimeQuota
                        ? config.WorkBegin
                        : config.WorkBegin - (config.MaxOvertime * randomizer.Int32(100) / 100f);
                    returnHomeHour = config.WorkEnd;
                    break;

                default:
                    return false;
            }

            ref Building home = ref buildingManager.m_buildings.m_buffer[citizen.m_homeBuilding];
            ref Building target = ref buildingManager.m_buildings.m_buffer[citizen.m_workBuilding];

            float distance = Mathf.Clamp(Vector3.Distance(home.m_position, target.m_position), 0, 1000f);
            float onTheWay = 0.5f + (distance / 500f);

            leaveHomeHour -= onTheWay;

            return currentHour >= leaveHomeHour && currentHour < returnHomeHour;
        }

        /// <summary>
        /// Determines whether the provided <paramref name="citizen"/> should return back from school or from work.
        /// </summary>
        ///
        /// <param name="citizen">The <see cref="Citizen"/> to check.</param>
        ///
        /// <returns>True if the <paramref name="citizen"/> should return back from work; otherwise, false.</returns>
        public bool ShouldReturnFromSchoolOrWork(ref Citizen citizen)
        {
            if (IsWeekend)
            {
                return true;
            }

            float currentHour = timeInfo.CurrentHour;

            switch (Citizen.GetAgeGroup(citizen.Age))
            {
                case Citizen.AgeGroup.Child:
                case Citizen.AgeGroup.Teen:
                    return currentHour >= config.SchoolEnd || currentHour < config.SchoolBegin;

                case Citizen.AgeGroup.Young:
                case Citizen.AgeGroup.Adult:
                    if (currentHour >= (config.WorkEnd + config.MaxOvertime) || currentHour < config.WorkBegin)
                    {
                        return true;
                    }
                    else if (currentHour >= config.WorkEnd)
                    {
                        return randomizer.Int32(100) < config.OnTimeQuota;
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
            if (!config.IsLunchTimeEnabled)
            {
                return false;
            }

            switch (Citizen.GetAgeGroup(citizen.Age))
            {
                case Citizen.AgeGroup.Child:
                case Citizen.AgeGroup.Senior:
                    return false;
            }

            float currentHour = timeInfo.CurrentHour;
            if (currentHour >= config.LunchBegin && currentHour <= config.LunchEnd)
            {
                return randomizer.Int32(100) < config.LunchQuota;
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
            float dayHourStart = timeInfo.SunriseHour;
            float dayHourEnd = timeInfo.SunsetHour;

            switch (Citizen.GetAgeGroup(citizen.Age))
            {
                case Citizen.AgeGroup.Child:
                    dayHourStart += 1f;
                    dayHourEnd -= 2f;
                    break;

                case Citizen.AgeGroup.Teen:
                    dayHourStart += 1f;
                    break;

                case Citizen.AgeGroup.Senior:
                    dayHourEnd -= 2f;
                    break;
            }

            float currentHour = timeInfo.CurrentHour;
            if (currentHour > dayHourStart && currentHour < dayHourEnd)
            {
                return randomizer.Int32(100) < GetGoOutThroughDayChance(citizen.Age);
            }
            else
            {
                return randomizer.Int32(100) < GetGoOutAtNightChance(citizen.Age);
            }
        }

        /// <summary>
        /// Determines whether the provided <paramref name="citizen"/> can stay out at current time.
        /// </summary>
        ///
        /// <param name="citizen">The <see cref="Citizen"/> to check.</param>
        ///
        /// <returns>True if the <paramref name="citizen"/> can stay out; otherwise, false.</returns>
        public bool CanStayOut(ref Citizen citizen)
        {
            float currentHour = timeInfo.CurrentHour;
            return (currentHour >= timeInfo.SunriseHour && currentHour < timeInfo.SunsetHour) || GetGoOutAtNightChance(citizen.Age) > 0;
        }

        /// <summary>
        /// Gets a value in range from 0 to 100 that indicates a chance in percent that a Cim
        /// with provided <paramref name="age"/> will go out at night.
        /// </summary>
        /// <param name="age">The Cim's age.</param>
        ///
        /// <returns>A value in range from 0 to 100.</returns>
        public int GetGoOutAtNightChance(int age)
        {
            float currentHour = timeInfo.CurrentHour;
            int weekendMultiplier;

            if (IsWeekend && timeInfo.Now.IsWeekendAfter(TimeSpan.FromHours(12)))
            {
                weekendMultiplier = currentHour > 12f
                    ? 6
                    : Mathf.RoundToInt(Mathf.Clamp(-((currentHour * 1.5f) - 6f), 0f, 6f));
            }
            else
            {
                weekendMultiplier = 1;
            }

            switch (Citizen.GetAgeGroup(age))
            {
                case Citizen.AgeGroup.Teen:
                    return 8 * weekendMultiplier;

                case Citizen.AgeGroup.Young:
                    return 10 * weekendMultiplier;

                case Citizen.AgeGroup.Adult:
                    return 2 * weekendMultiplier;

                default:
                    return 0;
            }
        }

        /// <summary>
        /// Determines whether the current game day is a work day and the time is between
        /// the provided hours.
        /// </summary>
        /// <param name="fromInclusive">The start day time hour of the range to check.</param>
        /// <param name="toExclusive">The end day time hour of the range to check.</param>
        ///
        /// <returns>True if the curent game day is a work day and the day time hour is
        /// between the provided values; otherwise, false.</returns>
        public bool IsWorkDayAndBetweenHours(float fromInclusive, float toExclusive)
        {
            float currentHour = timeInfo.CurrentHour;
            return IsWorkDay && (currentHour >= fromInclusive && currentHour < toExclusive);
        }

        private bool CanWorkOrStudyToday(ref Citizen citizen)
        {
            return citizen.m_workBuilding != 0 && IsWorkDay;
        }

        private int GetGoOutThroughDayChance(int age)
        {
            int weekendMultiplier = IsWeekend && timeInfo.Now.IsWeekendAfter(TimeSpan.FromHours(12))
                ? 4
                : 1;

            switch (Citizen.GetAgeGroup(age))
            {
                case Citizen.AgeGroup.Child:
                    return 60;

                case Citizen.AgeGroup.Teen:
                    return 13 * weekendMultiplier;

                case Citizen.AgeGroup.Young:
                    return 12 * weekendMultiplier;

                case Citizen.AgeGroup.Adult:
                    return 1 * weekendMultiplier;

                case Citizen.AgeGroup.Senior:
                    return 90;

                default:
                    return 0;
            }
        }
    }
}
