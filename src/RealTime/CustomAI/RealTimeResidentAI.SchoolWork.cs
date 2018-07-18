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
        private const int ShiftBitsCount = 5;
        private const uint WorkShiftsMask = (1u << ShiftBitsCount) - 1;

        private uint secondShiftQuota;
        private uint nightShiftQuota;
        private uint secondShiftValue;
        private uint nightShiftValue;

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
                case ItemClass.Service.Monument:
                    return true;

                default:
                    return false;
            }
        }

        private static int GetBuildingWorkShiftCount(ItemClass.Service service)
        {
            switch (service)
            {
                case ItemClass.Service.Office:
                case ItemClass.Service.Garbage:
                case ItemClass.Service.Education:
                    return 1;

                case ItemClass.Service.Road:
                case ItemClass.Service.Beautification:
                case ItemClass.Service.Monument:
                case ItemClass.Service.Citizen:
                    return 2;

                case ItemClass.Service.Commercial:
                case ItemClass.Service.Industrial:
                case ItemClass.Service.Tourism:
                case ItemClass.Service.Electricity:
                case ItemClass.Service.Water:
                case ItemClass.Service.HealthCare:
                case ItemClass.Service.PoliceDepartment:
                case ItemClass.Service.FireDepartment:
                case ItemClass.Service.PublicTransport:
                case ItemClass.Service.Disaster:
                case ItemClass.Service.Natural:
                    return 3;

                default:
                    return 1;
            }
        }

        private static bool ShouldWorkAtDawn(ItemClass.Service service, ItemClass.SubService subService)
        {
            switch (service)
            {
                case ItemClass.Service.Commercial when subService == ItemClass.SubService.CommercialLow:
                case ItemClass.Service.Beautification:
                case ItemClass.Service.Garbage:
                case ItemClass.Service.Road:
                    return true;

                default:
                    return false;
            }
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

        private WorkerShift GetWorkerShift(uint citizenId)
        {
            if (secondShiftQuota != Config.SecondShiftQuota || nightShiftQuota != Config.NightShiftQuota)
            {
                secondShiftQuota = Config.SecondShiftQuota;
                nightShiftQuota = Config.NightShiftQuota;
                CalculateWorkShiftValues();
            }

            uint value = citizenId & WorkShiftsMask;
            if (value <= secondShiftValue)
            {
                return WorkerShift.Second;
            }

            value = (citizenId >> ShiftBitsCount) & WorkShiftsMask;
            if (value <= nightShiftValue)
            {
                return WorkerShift.Night;
            }

            return WorkerShift.First;
        }

        private void CalculateWorkShiftValues()
        {
            secondShiftValue = Config.SecondShiftQuota - 1;
            nightShiftValue = Config.NightShiftQuota - 1;
        }

        private void ProcessCitizenAtSchoolOrWork(TAI instance, uint citizenId, ref TCitizen citizen)
        {
            ushort workBuilding = CitizenProxy.GetWorkBuilding(ref citizen);
            if (workBuilding == 0)
            {
                Log.Debug($"WARNING: {GetCitizenDesc(citizenId, ref citizen)} is in corrupt state: at school/work with no work building. Teleporting home.");
                CitizenProxy.SetLocation(ref citizen, Citizen.Location.Home);
                return;
            }

            ushort currentBuilding = CitizenProxy.GetCurrentBuilding(ref citizen);
            if (ShouldGoToLunch(CitizenProxy.GetAge(ref citizen), citizenId))
            {
                ushort lunchPlace = MoveToCommercialBuilding(instance, citizenId, ref citizen, LocalSearchDistance);
                if (lunchPlace != 0)
                {
                    residentStates[citizenId].WorkStatus = WorkStatus.AtLunch;
                    Log.Debug(TimeInfo.Now, $"{GetCitizenDesc(citizenId, ref citizen)} is going for lunch from {currentBuilding} to {lunchPlace}");
                }
                else
                {
                    Log.Debug(TimeInfo.Now, $"{GetCitizenDesc(citizenId, ref citizen)} wanted to go for lunch from {currentBuilding}, but there were no buildings close enough");
                }

                return;
            }

            if (!ShouldReturnFromSchoolOrWork(citizenId, currentBuilding, CitizenProxy.GetAge(ref citizen)))
            {
                return;
            }

            Log.Debug(TimeInfo.Now, $"{GetCitizenDesc(citizenId, ref citizen)} leaves their workplace {workBuilding}");

            if (CitizenGoesToEvent(instance, citizenId, ref citizen))
            {
                return;
            }

            if (!CitizenGoesShopping(instance, citizenId, ref citizen) && !CitizenGoesRelaxing(instance, citizenId, ref citizen))
            {
                residentAI.StartMoving(instance, citizenId, ref citizen, workBuilding, CitizenProxy.GetHomeBuilding(ref citizen));
            }
        }

        private bool CitizenGoesWorking(TAI instance, uint citizenId, ref TCitizen citizen)
        {
            ushort homeBuilding = CitizenProxy.GetHomeBuilding(ref citizen);
            ushort workBuilding = CitizenProxy.GetWorkBuilding(ref citizen);
            ushort currentBuilding = CitizenProxy.GetCurrentBuilding(ref citizen);

            if (!ShouldMoveToSchoolOrWork(citizenId, homeBuilding, currentBuilding, workBuilding, CitizenProxy.GetAge(ref citizen)))
            {
                return false;
            }

            Log.Debug(TimeInfo.Now, $"{GetCitizenDesc(citizenId, ref citizen)} is going from {currentBuilding} to school/work {workBuilding}");

            ref ResidentStateDescriptor state = ref residentStates[citizenId];
            state.DepartureTime = TimeInfo.Now;
            residentAI.StartMoving(instance, citizenId, ref citizen, homeBuilding, workBuilding);

            return true;
        }

        private bool ShouldMoveToSchoolOrWork(uint citizenId, ushort homeBuilding, ushort currentBuilding, ushort workBuilding, Citizen.AgeGroup citizenAge)
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

            // TODO: replace the day off logic.
            if ((citizenId & 0x7FF) == TimeInfo.Now.Day)
            {
                Log.Debug(TimeInfo.Now, $"Citizen {citizenId} has a day off work today");
                return false;
            }

            float travelTime = currentBuilding == homeBuilding
                ? GetTravelTimeHomeToWork(citizenId, homeBuilding, workBuilding)
                : 0;

            if (citizenAge == Citizen.AgeGroup.Child || citizenAge == Citizen.AgeGroup.Teen)
            {
                return ShouldMoveToSchoolOrWork(workBuilding, Config.SchoolBegin, Config.SchoolEnd, 0, travelTime);
            }

            GetWorkShiftTimes(citizenId, buildingSevice, buildingSubService, out float workBeginHour, out float workEndHour);
            if (!CheckMinimumShiftDuration(workBeginHour, workEndHour))
            {
                return false;
            }

            float overtime = currentBuilding != homeBuilding || Random.ShouldOccur(Config.OnTimeQuota)
                ? 0
                : Config.MaxOvertime * Random.GetRandomValue(100u) / 200f;

            return ShouldMoveToSchoolOrWork(workBuilding, workBeginHour, workEndHour, overtime, travelTime);
        }

        private bool ShouldMoveToSchoolOrWork(ushort workBuilding, float workBeginHour, float workEndHour, float overtime, float travelTime)
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

            gotoHour = workBeginHour - overtime - travelTime;
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

        private bool ShouldGoToLunch(Citizen.AgeGroup citizenAge, uint citizenId)
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
            if (!IsBadWeather(citizenId) && currentHour >= Config.LunchBegin && currentHour <= Config.LunchEnd)
            {
                return Random.ShouldOccur(Config.LunchQuota);
            }

            return false;
        }

        private bool CitizenReturnsFromLunch(TAI instance, uint citizenId, ref TCitizen citizen)
        {
            if (IsLunchHour)
            {
                return false;
            }

            ushort workBuilding = CitizenProxy.GetWorkBuilding(ref citizen);
            if (workBuilding != 0)
            {
                Log.Debug(TimeInfo.Now, $"{GetCitizenDesc(citizenId, ref citizen)} returning from lunch to {workBuilding}");
                ReturnFromVisit(instance, citizenId, ref citizen, workBuilding, Citizen.Location.Work);
            }
            else
            {
                Log.Debug($"WARNING: {GetCitizenDesc(citizenId, ref citizen)} is at lunch but no work building. Teleporting home.");
                CitizenProxy.SetLocation(ref citizen, Citizen.Location.Home);
            }

            residentStates[citizenId].WorkStatus = WorkStatus.Default;
            return true;
        }

        private void GetWorkShiftTimes(uint citizenId, ItemClass.Service sevice, ItemClass.SubService subService, out float beginHour, out float endHour)
        {
            float begin = -1;
            float end = -1;

            int shiftCount = GetBuildingWorkShiftCount(sevice);
            if (shiftCount > 1)
            {
                switch (GetWorkerShift(citizenId))
                {
                    case WorkerShift.Second:
                        begin = Config.WorkEnd;
                        end = 0;
                        break;

                    case WorkerShift.Night when shiftCount == 3:
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

        private float GetTravelTimeHomeToWork(uint citizenId, ushort homeBuilding, ushort workBuilding)
        {
            float result = residentStates[citizenId].TravelTimeToWork;
            if (result <= 0)
            {
                float distance = BuildingMgr.GetDistanceBetweenBuildings(homeBuilding, workBuilding);
                result = Mathf.Clamp(distance / OnTheWayDistancePerHour, MinHoursOnTheWay, MaxHoursOnTheWay);
            }

            return result;
        }
    }
}
