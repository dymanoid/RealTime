// <copyright file="RealTimeResidentAI.Core.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.CustomAI
{
    using System;
    using RealTime.Tools;
    using UnityEngine;

    internal sealed partial class RealTimeResidentAI<TAI, TCitizen>
    {
        private bool IsLunchHour => IsWorkDayAndBetweenHours(Config.LunchBegin, Config.LunchEnd);

        private bool IsWeekend => Config.IsWeekendEnabled && TimeInfo.Now.IsWeekend();

        private bool IsWorkDay => !Config.IsWeekendEnabled || !TimeInfo.Now.IsWeekend();

        private bool ShouldMoveToSchoolOrWork(ushort workBuilding, ushort currentBuilding, Citizen.AgeGroup citizenAge)
        {
            if (workBuilding == 0 || IsWeekend)
            {
                return false;
            }

            float currentHour = TimeInfo.CurrentHour;
            float gotoWorkHour;
            float leaveWorkHour;
            bool overtime;

            switch (citizenAge)
            {
                case Citizen.AgeGroup.Child:
                case Citizen.AgeGroup.Teen:
                    gotoWorkHour = Config.SchoolBegin;
                    leaveWorkHour = Config.SchoolEnd;
                    overtime = false;
                    break;

                case Citizen.AgeGroup.Young:
                case Citizen.AgeGroup.Adult:
                    gotoWorkHour = Config.WorkBegin;
                    leaveWorkHour = Config.WorkEnd;
                    overtime = IsChance(Config.OnTimeQuota);
                    break;

                default:
                    return false;
            }

            // Performance optimization:
            // If the current hour is far away from the working hours, don't even calculate the overtime and on the way time
            if (overtime
                && (currentHour < gotoWorkHour - MaxHoursOnTheWayToWork - Config.MaxOvertime || currentHour > leaveWorkHour + Config.MaxOvertime))
            {
                return false;
            }
            else if (currentHour < gotoWorkHour - MaxHoursOnTheWayToWork || currentHour > leaveWorkHour)
            {
                return false;
            }

            if (overtime)
            {
                gotoWorkHour -= Config.MaxOvertime * Randomizer.Int32(100) / 200f;
                leaveWorkHour += Config.MaxOvertime;
            }

            float distance = BuildingManager.GetDistanceBetweenBuildings(currentBuilding, workBuilding);
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

            float currentHour = TimeInfo.CurrentHour;

            switch (citizenAge)
            {
                case Citizen.AgeGroup.Child:
                case Citizen.AgeGroup.Teen:
                    return currentHour >= Config.SchoolEnd || currentHour < Config.SchoolBegin - MaxHoursOnTheWayToWork;

                case Citizen.AgeGroup.Young:
                case Citizen.AgeGroup.Adult:
                    if (currentHour >= (Config.WorkEnd + Config.MaxOvertime) || currentHour < Config.WorkBegin - MaxHoursOnTheWayToWork)
                    {
                        return true;
                    }
                    else if (currentHour >= Config.WorkEnd)
                    {
                        return IsChance(Config.OnTimeQuota);
                    }

                    break;

                default:
                    return true;
            }

            return false;
        }

        private bool ShouldGoToLunch(Citizen.AgeGroup citizenAge)
        {
            if (!Config.IsLunchTimeEnabled)
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

            float currentHour = TimeInfo.CurrentHour;
            if (currentHour >= Config.LunchBegin && currentHour <= Config.LunchEnd)
            {
                return IsChance(Config.LunchQuota);
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
                    dayHourEnd = TimeInfo.SunsetHour - 1f;
                    break;

                case Citizen.AgeGroup.Teen:
                    dayHourStart = WakeUpHour;
                    dayHourEnd = LatestTeenEntertainmentHour;
                    break;

                case Citizen.AgeGroup.Senior:
                    dayHourStart = WakeUpHour;
                    dayHourEnd = TimeInfo.SunsetHour;
                    break;

                default:
                    dayHourStart = WakeUpHour;
                    dayHourEnd = LatestAdultEntertainmentHour;
                    break;
            }

            float currentHour = TimeInfo.CurrentHour;
            return IsChance(GetGoOutChance(citizenAge, currentHour > dayHourStart && currentHour < dayHourEnd));
        }

        private uint GetGoOutChance(Citizen.AgeGroup citizenAge, bool isDayTime)
        {
            float currentHour = TimeInfo.CurrentHour;
            uint multiplier;

            if ((IsWeekend && TimeInfo.Now.IsWeekendAfter(AssumedGoOutDuration)) || TimeInfo.Now.DayOfWeek == DayOfWeek.Friday)
            {
                multiplier = isDayTime
                    ? 5u
                    : (uint)Mathf.Clamp(Mathf.Abs(TimeInfo.SunriseHour - currentHour), 0f, 5f);
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
            float currentHour = TimeInfo.CurrentHour;
            return IsWorkDay && (currentHour >= fromInclusive && currentHour < toExclusive);
        }

        private bool IsWorkDayMorning()
        {
            if (!IsWorkDay)
            {
                return false;
            }

            float currentHour = TimeInfo.CurrentHour;
            return currentHour >= TimeInfo.SunriseHour && currentHour < Math.Min(Config.WorkBegin, Config.SchoolBegin);
        }
    }
}
