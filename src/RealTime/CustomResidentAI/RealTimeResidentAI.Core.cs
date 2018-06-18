// <copyright file="RealTimeResidentAI.Core.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.CustomResidentAI
{
    using System;
    using RealTime.Tools;
    using UnityEngine;

    internal sealed partial class RealTimeResidentAI<TAI, TCitizen>
    {
        private bool IsLunchHour => IsWorkDayAndBetweenHours(config.LunchBegin, config.LunchEnd);

        private bool IsWeekend => config.IsWeekendEnabled && timeInfo.Now.IsWeekend();

        private bool IsWorkDay => !config.IsWeekendEnabled || !timeInfo.Now.IsWeekend();

        private bool ShouldMoveToSchoolOrWork(ushort workBuilding, ushort currentBuilding, Citizen.AgeGroup citizenAge)
        {
            if (workBuilding == 0 || IsWeekend)
            {
                return false;
            }

            float currentHour = timeInfo.CurrentHour;
            float gotoWorkHour;
            float leaveWorkHour;
            bool overtime;

            switch (citizenAge)
            {
                case Citizen.AgeGroup.Child:
                case Citizen.AgeGroup.Teen:
                    gotoWorkHour = config.SchoolBegin;
                    leaveWorkHour = config.SchoolEnd;
                    overtime = false;
                    break;

                case Citizen.AgeGroup.Young:
                case Citizen.AgeGroup.Adult:
                    gotoWorkHour = config.WorkBegin;
                    leaveWorkHour = config.WorkEnd;
                    overtime = IsChance(config.OnTimeQuota);
                    break;

                default:
                    return false;
            }

            // Performance optimization:
            // If the current hour is far away from the working hours, don't even calculate the overtime and on the way time
            if (overtime
                && (currentHour < gotoWorkHour - MaxHoursOnTheWayToWork - config.MaxOvertime || currentHour > leaveWorkHour + config.MaxOvertime))
            {
                return false;
            }
            else if (currentHour < gotoWorkHour - MaxHoursOnTheWayToWork || currentHour > leaveWorkHour)
            {
                return false;
            }

            if (overtime)
            {
                gotoWorkHour -= config.MaxOvertime * Randomizer.Int32(100) / 200f;
                leaveWorkHour += config.MaxOvertime;
            }

            float distance = buildingManager.GetDistanceBetweenBuildings(currentBuilding, workBuilding);
            float onTheWay = Mathf.Clamp(distance / OnTheWayDistancePerHour, MinHoursOnTheWayToWork, MaxHoursOnTheWayToWork);

            gotoWorkHour -= onTheWay;

            return currentHour >= gotoWorkHour && currentHour < leaveWorkHour;
        }

        private bool ShouldReturnFromSchoolOrWork(Citizen.AgeGroup citizenAge)
        {
            if (IsWeekend)
            {
                return true;
            }

            float currentHour = timeInfo.CurrentHour;

            switch (citizenAge)
            {
                case Citizen.AgeGroup.Child:
                case Citizen.AgeGroup.Teen:
                    return currentHour >= config.SchoolEnd || currentHour < config.SchoolBegin - MaxHoursOnTheWayToWork;

                case Citizen.AgeGroup.Young:
                case Citizen.AgeGroup.Adult:
                    if (currentHour >= (config.WorkEnd + config.MaxOvertime) || currentHour < config.WorkBegin - MaxHoursOnTheWayToWork)
                    {
                        return true;
                    }
                    else if (currentHour >= config.WorkEnd)
                    {
                        return IsChance(config.OnTimeQuota);
                    }

                    break;

                default:
                    return true;
            }

            return false;
        }

        private bool ShouldGoToLunch(Citizen.AgeGroup citizenAge)
        {
            if (!config.IsLunchTimeEnabled)
            {
                return false;
            }

            switch (citizenAge)
            {
                case Citizen.AgeGroup.Child:
                case Citizen.AgeGroup.Teen:
                case Citizen.AgeGroup.Senior:
                    return false;
            }

            float currentHour = timeInfo.CurrentHour;
            if (currentHour >= config.LunchBegin && currentHour <= config.LunchEnd)
            {
                return IsChance(config.LunchQuota);
            }

            return false;
        }

        private bool ShouldFindEntertainment(Citizen.AgeGroup citizenAge)
        {
            float dayHourStart;
            float dayHourEnd;

            switch (citizenAge)
            {
                case Citizen.AgeGroup.Child:
                    dayHourStart = WakeUpHour;
                    dayHourEnd = timeInfo.SunsetHour - 1f;
                    break;

                case Citizen.AgeGroup.Teen:
                    dayHourStart = WakeUpHour;
                    dayHourEnd = LatestTeenEntertainmentHour;
                    break;

                case Citizen.AgeGroup.Senior:
                    dayHourStart = WakeUpHour;
                    dayHourEnd = timeInfo.SunsetHour;
                    break;

                default:
                    dayHourStart = WakeUpHour;
                    dayHourEnd = LatestAdultEntertainmentHour;
                    break;
            }

            float currentHour = timeInfo.CurrentHour;
            return IsChance(GetGoOutChance(citizenAge, currentHour > dayHourStart && currentHour < dayHourEnd));
        }

        private uint GetGoOutChance(Citizen.AgeGroup citizenAge, bool isDayTime)
        {
            float currentHour = timeInfo.CurrentHour;
            uint multiplier;

            if ((IsWeekend && timeInfo.Now.IsWeekendAfter(AssumedGoOutDuration)) || timeInfo.Now.DayOfWeek == DayOfWeek.Friday)
            {
                multiplier = isDayTime
                    ? 5u
                    : (uint)Mathf.Clamp(Mathf.Abs(timeInfo.SunriseHour - currentHour), 0f, 5f);
            }
            else
            {
                multiplier = 1u;
            }

            switch (citizenAge)
            {
                case Citizen.AgeGroup.Child when isDayTime:
                    return 60 + (4 * multiplier);

                case Citizen.AgeGroup.Teen when isDayTime:
                case Citizen.AgeGroup.Young:
                    return 50 + (8 * multiplier);

                case Citizen.AgeGroup.Adult:
                    return 30 + (6 * multiplier);

                case Citizen.AgeGroup.Senior when isDayTime:
                    return 90;

                default:
                    return 0;
            }
        }

        private bool IsWorkDayAndBetweenHours(float fromInclusive, float toExclusive)
        {
            float currentHour = timeInfo.CurrentHour;
            return IsWorkDay && (currentHour >= fromInclusive && currentHour < toExclusive);
        }

        private bool IsWorkDayMorning()
        {
            if (!IsWorkDay)
            {
                return false;
            }

            float currentHour = timeInfo.CurrentHour;
            return currentHour >= timeInfo.SunriseHour && currentHour < Math.Min(config.WorkBegin, config.SchoolBegin);
        }
    }
}
