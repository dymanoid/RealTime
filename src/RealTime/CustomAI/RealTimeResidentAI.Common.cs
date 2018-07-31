// <copyright file="RealTimeResidentAI.Common.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.CustomAI
{
    using System;
    using RealTime.Tools;
    using static Constants;

    internal sealed partial class RealTimeResidentAI<TAI, TCitizen>
    {
        private DateTime todayWakeUp;

        private enum ScheduleAction
        {
            Ignore,
            ProcessTransition,
            ProcessState
        }

        private void ProcessCitizenDead(TAI instance, uint citizenId, ref TCitizen citizen)
        {
            ushort currentBuilding = CitizenProxy.GetCurrentBuilding(ref citizen);
            Citizen.Location currentLocation = CitizenProxy.GetLocation(ref citizen);
            switch (currentLocation)
            {
                case Citizen.Location.Home when currentBuilding != 0:
                    CitizenProxy.SetWorkplace(ref citizen, citizenId, 0);
                    CitizenProxy.SetVisitPlace(ref citizen, citizenId, 0);
                    break;

                case Citizen.Location.Work when currentBuilding != 0:
                    CitizenProxy.SetHome(ref citizen, citizenId, 0);
                    CitizenProxy.SetVisitPlace(ref citizen, citizenId, 0);
                    break;

                case Citizen.Location.Visit when currentBuilding != 0:
                    CitizenProxy.SetHome(ref citizen, citizenId, 0);
                    CitizenProxy.SetWorkplace(ref citizen, citizenId, 0);

                    if (BuildingMgr.GetBuildingService(CitizenProxy.GetVisitBuilding(ref citizen)) == ItemClass.Service.HealthCare)
                    {
                        return;
                    }

                    break;

                case Citizen.Location.Moving when CitizenProxy.GetVehicle(ref citizen) != 0:
                    CitizenProxy.SetHome(ref citizen, citizenId, 0);
                    CitizenProxy.SetWorkplace(ref citizen, citizenId, 0);
                    CitizenProxy.SetVisitPlace(ref citizen, citizenId, 0);
                    return;

                default:
                    Log.Debug(TimeInfo.Now, $"{GetCitizenDesc(citizenId, ref citizen)} is released because of death");
                    residentSchedules[citizenId] = default;
                    CitizenMgr.ReleaseCitizen(citizenId);
                    return;
            }

            residentAI.FindHospital(instance, citizenId, currentBuilding, TransferManager.TransferReason.Dead);
            Log.Debug(TimeInfo.Now, $"{GetCitizenDesc(citizenId, ref citizen)} is dead, body should get serviced");
        }

        private bool ProcessCitizenArrested(ref TCitizen citizen)
        {
            switch (CitizenProxy.GetLocation(ref citizen))
            {
                case Citizen.Location.Moving:
                    return false;
                case Citizen.Location.Visit
                    when BuildingMgr.GetBuildingService(CitizenProxy.GetVisitBuilding(ref citizen)) == ItemClass.Service.PoliceDepartment:
                    return true;
            }

            CitizenProxy.SetArrested(ref citizen, false);
            return false;
        }

        private bool ProcessCitizenSick(TAI instance, uint citizenId, ref TCitizen citizen)
        {
            Citizen.Location currentLocation = CitizenProxy.GetLocation(ref citizen);
            if (currentLocation == Citizen.Location.Moving)
            {
                return false;
            }

            ushort currentBuilding = CitizenProxy.GetCurrentBuilding(ref citizen);

            if (currentLocation != Citizen.Location.Home && currentBuilding == 0)
            {
                Log.Debug($"Teleporting {GetCitizenDesc(citizenId, ref citizen)} back home because they are sick but no building is specified");
                CitizenProxy.SetLocation(ref citizen, Citizen.Location.Home);
                return true;
            }

            if (currentLocation != Citizen.Location.Home && CitizenProxy.GetVehicle(ref citizen) != 0)
            {
                return true;
            }

            if (currentLocation == Citizen.Location.Visit)
            {
                ItemClass.Service service = BuildingMgr.GetBuildingService(CitizenProxy.GetVisitBuilding(ref citizen));
                if (service == ItemClass.Service.HealthCare || service == ItemClass.Service.Disaster)
                {
                    return true;
                }
            }

            Log.Debug(TimeInfo.Now, $"{GetCitizenDesc(citizenId, ref citizen)} is sick, trying to get to a hospital");
            residentAI.FindHospital(instance, citizenId, currentBuilding, TransferManager.TransferReason.Sick);
            return true;
        }

        private void DoScheduledEvacuation(ref CitizenSchedule schedule, TAI instance, uint citizenId, ref TCitizen citizen)
        {
            ushort building = CitizenProxy.GetCurrentBuilding(ref citizen);
            if (building == 0)
            {
                schedule.Schedule(ResidentState.AtHome, default);
                return;
            }

            schedule.Schedule(ResidentState.InShelter, default);
            if (schedule.CurrentState != ResidentState.InShelter)
            {
                Log.Debug(TimeInfo.Now, $"{GetCitizenDesc(citizenId, ref citizen)} is trying to find an evacuation place");
                residentAI.FindEvacuationPlace(instance, citizenId, building, residentAI.GetEvacuationReason(instance, building));
            }
        }

        private bool ProcessCitizenInShelter(ref CitizenSchedule schedule, ref TCitizen citizen)
        {
            ushort shelter = CitizenProxy.GetVisitBuilding(ref citizen);
            if (BuildingMgr.BuildingHasFlags(shelter, Building.Flags.Downgrading))
            {
                schedule.Schedule(ResidentState.Unknown, default);
                return true;
            }

            return false;
        }

        private ScheduleAction UpdateCitizenState(ref TCitizen citizen, ref CitizenSchedule schedule)
        {
            if (schedule.CurrentState == ResidentState.Ignored)
            {
                return ScheduleAction.Ignore;
            }

            if (CitizenProxy.HasFlags(ref citizen, Citizen.Flags.DummyTraffic))
            {
                schedule.CurrentState = ResidentState.Ignored;
                return ScheduleAction.Ignore;
            }

            Citizen.Location location = CitizenProxy.GetLocation(ref citizen);
            if (location == Citizen.Location.Moving)
            {
                if (CitizenMgr.InstanceHasFlags(
                    CitizenProxy.GetInstance(ref citizen),
                    CitizenInstance.Flags.OnTour | CitizenInstance.Flags.TargetIsNode,
                    true))
                {
                    // Guided tours are treated as visits
                    schedule.CurrentState = ResidentState.Visiting;
                    schedule.Hint = ScheduleHint.OnTour;
                    return ScheduleAction.ProcessState;
                }

                return ScheduleAction.ProcessTransition;
            }

            ushort currentBuilding = CitizenProxy.GetCurrentBuilding(ref citizen);
            if (currentBuilding == 0)
            {
                schedule.CurrentState = ResidentState.Unknown;
                return ScheduleAction.ProcessState;
            }

            ItemClass.Service buildingService = BuildingMgr.GetBuildingService(currentBuilding);
            if (BuildingMgr.BuildingHasFlags(currentBuilding, Building.Flags.Evacuating)
                && buildingService != ItemClass.Service.Disaster)
            {
                schedule.CurrentState = ResidentState.Evacuation;
                schedule.Schedule(ResidentState.InShelter, default);
                return ScheduleAction.ProcessState;
            }

            switch (location)
            {
                case Citizen.Location.Home:
                    schedule.CurrentState = ResidentState.AtHome;
                    return ScheduleAction.ProcessState;

                case Citizen.Location.Work:
                    if (buildingService == ItemClass.Service.Disaster && CitizenProxy.HasFlags(ref citizen, Citizen.Flags.Evacuating))
                    {
                        schedule.CurrentState = ResidentState.InShelter;
                        return ScheduleAction.ProcessState;
                    }

                    if (CitizenProxy.GetVisitBuilding(ref citizen) == currentBuilding)
                    {
                        // A citizen may visit their own work building (e.g. shopping)
                        goto case Citizen.Location.Visit;
                    }

                    schedule.CurrentState = ResidentState.AtSchoolOrWork;
                    return ScheduleAction.ProcessState;

                case Citizen.Location.Visit:
                    switch (buildingService)
                    {
                        case ItemClass.Service.Beautification:
                        case ItemClass.Service.Monument:
                        case ItemClass.Service.Tourism:
                        case ItemClass.Service.Commercial
                            when BuildingMgr.GetBuildingSubService(currentBuilding) == ItemClass.SubService.CommercialLeisure
                                && schedule.WorkStatus != WorkStatus.Working:

                            schedule.CurrentState = ResidentState.Relaxing;
                            return ScheduleAction.ProcessState;

                        case ItemClass.Service.Commercial:
                            schedule.CurrentState = ResidentState.Shopping;
                            return ScheduleAction.ProcessState;

                        case ItemClass.Service.Disaster:
                            schedule.CurrentState = ResidentState.InShelter;
                            return ScheduleAction.ProcessState;
                    }

                    schedule.CurrentState = ResidentState.Visiting;
                    return ScheduleAction.ProcessState;
            }

            return ScheduleAction.Ignore;
        }

        private bool UpdateCitizenSchedule(ref CitizenSchedule schedule, uint citizenId, ref TCitizen citizen)
        {
            // If the game changed the work building, we have to update the work shifts first
            ushort workBuilding = CitizenProxy.GetWorkBuilding(ref citizen);
            if (schedule.WorkBuilding != workBuilding)
            {
                schedule.WorkBuilding = workBuilding;
                workBehavior.UpdateWorkShift(ref schedule, CitizenProxy.GetAge(ref citizen));
                if (schedule.CurrentState == ResidentState.AtSchoolOrWork && schedule.ScheduledStateTime == default)
                {
                    // When enabling for an existing game, the citizens that are working have no schedule yet
                    schedule.Schedule(ResidentState.Unknown, TimeInfo.Now.FutureHour(schedule.WorkShiftEndHour));
                }
                else if (schedule.WorkBuilding == 0
                    && (schedule.ScheduledState == ResidentState.AtSchoolOrWork || schedule.WorkStatus == WorkStatus.Working))
                {
                    // This is for the case when the citizen becomes unemployed while at work
                    schedule.Schedule(ResidentState.Unknown, default);
                }

                Log.Debug($"Updated work shifts for citizen {citizenId}: work shift {schedule.WorkShift}, {schedule.WorkShiftStartHour} - {schedule.WorkShiftEndHour}, weekends: {schedule.WorksOnWeekends}");
            }

            if (schedule.ScheduledState != ResidentState.Unknown)
            {
                return false;
            }

            Log.Debug(TimeInfo.Now, $"Scheduling for {GetCitizenDesc(citizenId, ref citizen)}...");

            if (schedule.WorkStatus == WorkStatus.Working)
            {
                schedule.WorkStatus = WorkStatus.None;
            }

            DateTime nextActivityTime = todayWakeUp;
            if (schedule.CurrentState != ResidentState.AtSchoolOrWork && workBuilding != 0)
            {
                if (ScheduleWork(ref schedule, ref citizen))
                {
                    return true;
                }

                if (schedule.ScheduledStateTime > nextActivityTime)
                {
                    nextActivityTime = schedule.ScheduledStateTime;
                }
            }

            if (ScheduleShopping(ref schedule, ref citizen, false))
            {
                Log.Debug($"  - Schedule shopping");
                return true;
            }

            if (ScheduleRelaxing(ref schedule, citizenId, ref citizen))
            {
                Log.Debug($"  - Schedule relaxing");
                return true;
            }

            if (schedule.CurrentState == ResidentState.AtHome)
            {
                if (Random.ShouldOccur(StayHomeAllDayChance))
                {
                    if (nextActivityTime < TimeInfo.Now)
                    {
                        nextActivityTime = todayWakeUp.FutureHour(Config.WakeUpHour);
                    }
                }
                else
                {
                    nextActivityTime = default;
                }

#if DEBUG
                if (nextActivityTime <= TimeInfo.Now)
                {
                    Log.Debug($"  - Schedule idle until next scheduling run");
                }
                else
                {
                    Log.Debug($"  - Schedule idle until {nextActivityTime}");
                }
#endif
                schedule.Schedule(ResidentState.Unknown, nextActivityTime);
            }
            else
            {
                Log.Debug($"  - Schedule moving home");
                schedule.Schedule(ResidentState.AtHome, default);
            }

            return true;
        }

        private void ExecuteCitizenSchedule(ref CitizenSchedule schedule, TAI instance, uint citizenId, ref TCitizen citizen, bool noReschedule)
        {
            if (ProcessCurrentState(ref schedule, citizenId, ref citizen)
                && schedule.ScheduledState == ResidentState.Unknown
                && !noReschedule)
            {
                Log.Debug(TimeInfo.Now, $"{GetCitizenDesc(citizenId, ref citizen)} will re-schedule now");

                // If the state processing changed the schedule, we need to update it
                UpdateCitizenSchedule(ref schedule, citizenId, ref citizen);
            }

            if (TimeInfo.Now < schedule.ScheduledStateTime)
            {
                return;
            }

            if (schedule.CurrentState == ResidentState.AtHome && IsCitizenVirtual(instance, ref citizen, ShouldRealizeCitizen))
            {
                Log.Debug($" *** Citizen {citizenId} is virtual this time");
                schedule.Schedule(ResidentState.Unknown, default);
                return;
            }

            switch (schedule.ScheduledState)
            {
                case ResidentState.AtHome:
                    DoScheduledHome(ref schedule, instance, citizenId, ref citizen);
                    break;

                case ResidentState.AtSchoolOrWork:
                    DoScheduledWork(ref schedule, instance, citizenId, ref citizen);
                    break;

                case ResidentState.Shopping when schedule.WorkStatus == WorkStatus.Working:
                    DoScheduledLunch(ref schedule, instance, citizenId, ref citizen);
                    break;

                case ResidentState.Shopping:
                    DoScheduledShopping(ref schedule, instance, citizenId, ref citizen);
                    break;

                case ResidentState.Relaxing:
                    DoScheduledRelaxing(ref schedule, instance, citizenId, ref citizen);
                    break;

                case ResidentState.InShelter:
                    DoScheduledEvacuation(ref schedule, instance, citizenId, ref citizen);
                    break;
            }
        }

        private bool ProcessCurrentState(ref CitizenSchedule schedule, uint citizenId, ref TCitizen citizen)
        {
            switch (schedule.CurrentState)
            {
                case ResidentState.AtHome:
                    return RescheduleAtHome(ref schedule, citizenId, ref citizen);

                case ResidentState.Shopping:
                    return ProcessCitizenShopping(ref schedule, citizenId, ref citizen);

                case ResidentState.Relaxing:
                    return ProcessCitizenRelaxing(ref schedule, citizenId, ref citizen);

                case ResidentState.Visiting:
                    return ProcessCitizenVisit(ref schedule, citizenId, ref citizen);

                case ResidentState.InShelter:
                    return ProcessCitizenInShelter(ref schedule, ref citizen);
            }

            return false;
        }

        private bool ShouldRealizeCitizen(TAI ai)
        {
            return residentAI.DoRandomMove(ai);
        }
    }
}
