// <copyright file="RealTimeResidentAI.SchoolWork.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.CustomAI
{
    using RealTime.Tools;
    using UnityEngine;
    using static Constants;

    internal sealed partial class RealTimeResidentAI<TAI, TCitizen>
    {
        private bool IsLunchHour => IsWorkDayAndBetweenHours(Config.LunchBegin, Config.LunchEnd);

        private void ProcessCitizenAtSchoolOrWork(TAI instance, uint citizenId, ref TCitizen citizen, bool isVirtual)
        {
            ushort workBuilding = CitizenProxy.GetWorkBuilding(ref citizen);
            if (workBuilding == 0)
            {
                Log.Debug($"WARNING: {GetCitizenDesc(citizenId, ref citizen, isVirtual)} is in corrupt state: at school/work with no work building. Teleporting home.");
                CitizenProxy.SetLocation(ref citizen, Citizen.Location.Home);
                return;
            }

            if (ShouldGoToLunch(CitizenProxy.GetAge(ref citizen)))
            {
                ushort currentBuilding = CitizenProxy.GetCurrentBuilding(ref citizen);

                ushort lunchPlace = MoveToCommercialBuilding(instance, citizenId, ref citizen, LocalSearchDistance, isVirtual);
                if (lunchPlace != 0)
                {
                    Log.Debug(TimeInfo.Now, $"{GetCitizenDesc(citizenId, ref citizen, isVirtual)} is going for lunch from {currentBuilding} to {lunchPlace}");
                }
                else
                {
                    Log.Debug(TimeInfo.Now, $"{GetCitizenDesc(citizenId, ref citizen, isVirtual)} wanted to go for lunch from {currentBuilding}, but there were no buildings close enough");
                }

                return;
            }

            if (!ShouldReturnFromSchoolOrWork(CitizenProxy.GetAge(ref citizen)))
            {
                return;
            }

            Log.Debug(TimeInfo.Now, $"{GetCitizenDesc(citizenId, ref citizen, isVirtual)} leaves their workplace {workBuilding}");

            if (CitizenGoesToEvent(instance, citizenId, ref citizen, isVirtual))
            {
                return;
            }

            if (!CitizenGoesShopping(instance, citizenId, ref citizen, isVirtual) && !CitizenGoesRelaxing(instance, citizenId, ref citizen, isVirtual))
            {
                if (isVirtual)
                {
                    CitizenProxy.SetLocation(ref citizen, Citizen.Location.Home);
                }
                else
                {
                    residentAI.StartMoving(instance, citizenId, ref citizen, workBuilding, CitizenProxy.GetHomeBuilding(ref citizen));
                }
            }
        }

        private bool CitizenGoesWorking(TAI instance, uint citizenId, ref TCitizen citizen, bool isVirtual)
        {
            ushort homeBuilding = CitizenProxy.GetHomeBuilding(ref citizen);
            ushort workBuilding = CitizenProxy.GetWorkBuilding(ref citizen);
            ushort currentBuilding = CitizenProxy.GetCurrentBuilding(ref citizen);

            if (!ShouldMoveToSchoolOrWork(workBuilding, currentBuilding, CitizenProxy.GetAge(ref citizen)))
            {
                return false;
            }

            Log.Debug(TimeInfo.Now, $"{GetCitizenDesc(citizenId, ref citizen, isVirtual)} is going from {currentBuilding} ({CitizenProxy.GetLocation(ref citizen)}) to school/work {workBuilding}");

            if (isVirtual)
            {
                CitizenProxy.SetLocation(ref citizen, Citizen.Location.Work);
            }
            else
            {
                residentAI.StartMoving(instance, citizenId, ref citizen, homeBuilding, workBuilding);
            }

            return true;
        }

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
                && (currentHour < gotoWorkHour - MaxHoursOnTheWay - Config.MaxOvertime || currentHour > leaveWorkHour + Config.MaxOvertime))
            {
                return false;
            }
            else if (currentHour < gotoWorkHour - MaxHoursOnTheWay || currentHour > leaveWorkHour)
            {
                return false;
            }

            if (overtime)
            {
                gotoWorkHour -= Config.MaxOvertime * Randomizer.Int32(100) / 200f;
                leaveWorkHour += Config.MaxOvertime;
            }

            float distance = BuildingMgr.GetDistanceBetweenBuildings(currentBuilding, workBuilding);
            float onTheWay = Mathf.Clamp(distance / OnTheWayDistancePerHour, MinHoursOnTheWay, MaxHoursOnTheWay);

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
                    return currentHour >= Config.SchoolEnd || currentHour < Config.SchoolBegin - MaxHoursOnTheWay;

                case Citizen.AgeGroup.Young:
                case Citizen.AgeGroup.Adult:
                    if (currentHour >= (Config.WorkEnd + Config.MaxOvertime) || currentHour < Config.WorkBegin - MaxHoursOnTheWay)
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
            if (!Config.IsLunchtimeEnabled)
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

        private bool CitizenReturnsFromLunch(TAI instance, uint citizenId, ref TCitizen citizen, bool isVirtual)
        {
            if (IsLunchHour)
            {
                return false;
            }

            ushort workBuilding = CitizenProxy.GetWorkBuilding(ref citizen);
            if (workBuilding != 0)
            {
                Log.Debug(TimeInfo.Now, $"{GetCitizenDesc(citizenId, ref citizen, isVirtual)} returning from lunch to {workBuilding}");
                ReturnFromVisit(instance, citizenId, ref citizen, workBuilding, Citizen.Location.Work, isVirtual);
            }
            else
            {
                Log.Debug($"WARNING: {GetCitizenDesc(citizenId, ref citizen, isVirtual)} is at lunch but no work building. Teleporting home.");
                CitizenProxy.SetLocation(ref citizen, Citizen.Location.Home);
            }

            return true;
        }
    }
}
