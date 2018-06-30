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
        private enum WorkerShift
        {
            None,
            First,
            Second,
            Night,
            Any
        }

        private bool IsLunchHour => IsWorkDayAndBetweenHours(Config.LunchBegin, Config.LunchEnd);

        private static bool IsBuildingActiveOnWeekend(ItemClass.Service service, ItemClass.SubService subService)
        {
            switch (service)
            {
                case ItemClass.Service.Commercial
                    when subService != ItemClass.SubService.CommercialHigh && subService != ItemClass.SubService.CommercialEco:
                case ItemClass.Service.Tourism:
                case ItemClass.Service.Electricity:
                case ItemClass.Service.Water:
                case ItemClass.Service.Beautification:
                case ItemClass.Service.HealthCare:
                case ItemClass.Service.PoliceDepartment:
                case ItemClass.Service.FireDepartment:
                case ItemClass.Service.PublicTransport:
                case ItemClass.Service.Disaster:
                    return true;

                default:
                    return false;
            }
        }

        private static bool IsBuildingActive24h(ItemClass.Service service)
        {
            switch (service)
            {
                case ItemClass.Service.Commercial:
                case ItemClass.Service.Tourism:
                case ItemClass.Service.Electricity:
                case ItemClass.Service.Water:
                case ItemClass.Service.HealthCare:
                case ItemClass.Service.PoliceDepartment:
                case ItemClass.Service.FireDepartment:
                case ItemClass.Service.PublicTransport:
                case ItemClass.Service.Disaster:
                    return true;

                default:
                    return false;
            }
        }

        private static bool ShouldWorkAtDawn(ItemClass.Service service, ItemClass.SubService subService)
        {
            return service == ItemClass.Service.Commercial && subService == ItemClass.SubService.CommercialLow;
        }

        private static bool CheckMinimumShiftDuration(float beginHour, float endHour)
        {
            if (beginHour < endHour)
            {
                return endHour - beginHour >= MinimumWorkShiftDuration;
            }
            else
            {
                return 24f - beginHour + endHour >= MinimumWorkShiftDuration;
            }
        }

        private static bool IsWorkHour(float currentHour, float gotoWorkHour, float leaveWorkHour)
        {
            if (gotoWorkHour < leaveWorkHour)
            {
                if (currentHour >= leaveWorkHour || currentHour < gotoWorkHour)
                {
                    return false;
                }
            }
            else
            {
                if (currentHour >= leaveWorkHour && currentHour < gotoWorkHour)
                {
                    return false;
                }
            }

            return true;
        }

        private static WorkerShift GetWorkerShift(uint citizenId)
        {
            switch (citizenId & 0b_11)
            {
                case 0b_01:
                    return WorkerShift.Second;
                case 0b_10:
                    return WorkerShift.Night;
                default:
                    return WorkerShift.Any;
            }
        }

        private void ProcessCitizenAtSchoolOrWork(TAI instance, uint citizenId, ref TCitizen citizen, bool isVirtual)
        {
            ushort workBuilding = CitizenProxy.GetWorkBuilding(ref citizen);
            if (workBuilding == 0)
            {
                Log.Debug($"WARNING: {GetCitizenDesc(citizenId, ref citizen, isVirtual)} is in corrupt state: at school/work with no work building. Teleporting home.");
                CitizenProxy.SetLocation(ref citizen, Citizen.Location.Home);
                return;
            }

            ushort currentBuilding = CitizenProxy.GetCurrentBuilding(ref citizen);
            if (ShouldGoToLunch(CitizenProxy.GetAge(ref citizen)))
            {
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

            if (!ShouldReturnFromSchoolOrWork(citizenId, currentBuilding, CitizenProxy.GetAge(ref citizen)))
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

            if (!ShouldMoveToSchoolOrWork(citizenId, workBuilding, currentBuilding, CitizenProxy.GetAge(ref citizen)))
            {
                return false;
            }

            Log.Debug(TimeInfo.Now, $"{GetCitizenDesc(citizenId, ref citizen, isVirtual)} is going from {currentBuilding} to school/work {workBuilding}");

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

        private bool ShouldMoveToSchoolOrWork(uint citizenId, ushort workBuilding, ushort currentBuilding, Citizen.AgeGroup citizenAge)
        {
            if (workBuilding == 0 || citizenAge == Citizen.AgeGroup.Senior)
            {
                return false;
            }

            ItemClass.Service buildingSevice = BuildingMgr.GetBuildingService(workBuilding);
            ItemClass.SubService buildingSubService = BuildingMgr.GetBuildingSubService(workBuilding);

            if (IsWeekend && !IsBuildingActiveOnWeekend(buildingSevice, buildingSubService))
            {
                return false;
            }

            if (citizenAge == Citizen.AgeGroup.Child || citizenAge == Citizen.AgeGroup.Teen)
            {
                return ShouldMoveToSchoolOrWork(currentBuilding, workBuilding, Config.SchoolBegin, Config.SchoolEnd, 0);
            }

            GetWorkShiftTimes(citizenId, buildingSevice, buildingSubService, out float workBeginHour, out float workEndHour);
            if (!CheckMinimumShiftDuration(workBeginHour, workEndHour))
            {
                return false;
            }

            float overtime = Random.ShouldOccur(Config.OnTimeQuota) ? 0 : Config.MaxOvertime * Random.GetRandomValue(100u) / 200f;

            return ShouldMoveToSchoolOrWork(currentBuilding, workBuilding, workBeginHour, workEndHour, overtime);
        }

        private bool ShouldMoveToSchoolOrWork(ushort currentBuilding, ushort workBuilding, float workBeginHour, float workEndHour, float overtime)
        {
            float gotoHour = workBeginHour - overtime - MaxHoursOnTheWay;
            if (gotoHour < 0)
            {
                gotoHour += 24f;
            }

            float leaveHour = workEndHour + overtime;
            if (leaveHour >= 24f)
            {
                leaveHour -= 24f;
            }

            float currentHour = TimeInfo.CurrentHour;
            if (!IsWorkHour(currentHour, gotoHour, leaveHour))
            {
                return false;
            }

            float distance = BuildingMgr.GetDistanceBetweenBuildings(currentBuilding, workBuilding);
            float onTheWay = Mathf.Clamp(distance / OnTheWayDistancePerHour, MinHoursOnTheWay, MaxHoursOnTheWay);

            gotoHour = workBeginHour - overtime - onTheWay;
            if (gotoHour < 0)
            {
                gotoHour += 24f;
            }

            return IsWorkHour(currentHour, gotoHour, leaveHour);
        }

        private bool ShouldReturnFromSchoolOrWork(uint citizenId, ushort buildingId, Citizen.AgeGroup citizenAge)
        {
            if (citizenAge == Citizen.AgeGroup.Senior)
            {
                return true;
            }

            ItemClass.Service buildingSevice = BuildingMgr.GetBuildingService(buildingId);
            ItemClass.SubService buildingSubService = BuildingMgr.GetBuildingSubService(buildingId);

            if (IsWeekend && !IsBuildingActiveOnWeekend(buildingSevice, buildingSubService))
            {
                return true;
            }

            float currentHour = TimeInfo.CurrentHour;

            if (citizenAge == Citizen.AgeGroup.Child || citizenAge == Citizen.AgeGroup.Teen)
            {
                return currentHour >= Config.SchoolEnd || currentHour < Config.SchoolBegin - MaxHoursOnTheWay;
            }

            GetWorkShiftTimes(citizenId, buildingSevice, buildingSubService, out float workBeginHour, out float workEndHour);
            if (!CheckMinimumShiftDuration(workBeginHour, workEndHour))
            {
                return true;
            }

            float earliestGotoHour = workBeginHour - MaxHoursOnTheWay - Config.MaxOvertime;
            if (earliestGotoHour < 0)
            {
                earliestGotoHour += 24f;
            }

            float latestLeaveHour = workEndHour + Config.MaxOvertime;
            if (latestLeaveHour >= 24f)
            {
                latestLeaveHour -= 24f;
            }

            if (earliestGotoHour < latestLeaveHour)
            {
                if (currentHour >= latestLeaveHour || currentHour < earliestGotoHour)
                {
                    return true;
                }
                else if (currentHour >= workEndHour)
                {
                    return Random.ShouldOccur(Config.OnTimeQuota);
                }
            }
            else
            {
                if (currentHour >= latestLeaveHour && currentHour < earliestGotoHour)
                {
                    return true;
                }
                else if (currentHour >= workEndHour && currentHour < earliestGotoHour)
                {
                    return Random.ShouldOccur(Config.OnTimeQuota);
                }
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
                return Random.ShouldOccur(Config.LunchQuota);
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

        private void GetWorkShiftTimes(uint citizenId, ItemClass.Service sevice, ItemClass.SubService subService, out float beginHour, out float endHour)
        {
            float begin = -1;
            float end = -1;

            if (IsBuildingActive24h(sevice))
            {
                switch (GetWorkerShift(citizenId))
                {
                    case WorkerShift.Second:
                        begin = Config.WorkEnd;
                        end = 0;
                        break;

                    case WorkerShift.Night:
                        begin = 0;
                        end = Config.WorkBegin;
                        break;
                }
            }

            if (begin < 0 || end < 0)
            {
                end = Config.WorkEnd;

                if (ShouldWorkAtDawn(sevice, subService))
                {
                    begin = Mathf.Min(TimeInfo.SunriseHour, EarliestWakeUp);
                }
                else
                {
                    begin = Config.WorkBegin;
                }
            }

            beginHour = begin;
            endHour = end;
        }
    }
}
