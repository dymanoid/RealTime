// <copyright file="RealTimeResidentAI.Core.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.CustomAI
{
    using UnityEngine;
    using static Constants;

    internal sealed partial class RealTimeResidentAI<TAI, TCitizen>
    {
        private bool IsLunchHour => IsWorkDayAndBetweenHours(Config.LunchBegin, Config.LunchEnd);

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
    }
}
