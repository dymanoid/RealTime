// <copyright file="RealTimeResidentAI.SchoolWork.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.CustomAI
{
    using RealTime.Tools;
    using static Constants;

    internal sealed partial class RealTimeResidentAI<TAI, TCitizen>
    {
        private bool ScheduleWork(ref CitizenSchedule schedule, ref TCitizen citizen)
        {
            ushort currentBuilding = CitizenProxy.GetCurrentBuilding(ref citizen);
            if (!workBehavior.ScheduleGoToWork(ref schedule, currentBuilding, simulationCycle))
            {
                return false;
            }

            Log.Debug($"  - Schedule work at {schedule.ScheduledStateTime}");

            float timeLeft = (float)(schedule.ScheduledStateTime - TimeInfo.Now).TotalHours;
            if (timeLeft <= PrepareToWorkHours)
            {
                // Just sit at home if the work time will come soon
                Log.Debug($"  - Work time in {timeLeft} hours, preparing for departure");
                return true;
            }

            if (timeLeft <= MaxTravelTime)
            {
                if (schedule.CurrentState != ResidentState.AtHome)
                {
                    Log.Debug($"  - Work time in {timeLeft} hours, returning home");
                    schedule.Schedule(ResidentState.AtHome);
                    return true;
                }

                // If we have some time, try to shop locally.
                if (ScheduleShopping(ref schedule, ref citizen, true))
                {
                    Log.Debug($"  - Work time in {timeLeft} hours, trying local shop");
                }
                else
                {
                    Log.Debug($"  - Work time in {timeLeft} hours, doing nothing");
                }

                return true;
            }

            return false;
        }

        private void DoScheduledWork(ref CitizenSchedule schedule, TAI instance, uint citizenId, ref TCitizen citizen)
        {
            ushort currentBuilding = CitizenProxy.GetCurrentBuilding(ref citizen);
            schedule.WorkStatus = WorkStatus.Working;
            schedule.DepartureToWorkTime = default;

            if (currentBuilding == schedule.WorkBuilding && schedule.CurrentState != ResidentState.AtSchoolOrWork)
            {
                CitizenProxy.SetVisitPlace(ref citizen, citizenId, 0);
                CitizenProxy.SetLocation(ref citizen, Citizen.Location.Work);
                return;
            }

            if (residentAI.StartMoving(instance, citizenId, ref citizen, currentBuilding, schedule.WorkBuilding))
            {
                if (schedule.CurrentState == ResidentState.AtHome)
                {
                    schedule.DepartureToWorkTime = TimeInfo.Now;
                }

                Citizen.AgeGroup citizenAge = CitizenProxy.GetAge(ref citizen);
                if (workBehavior.ScheduleLunch(ref schedule, citizenAge))
                {
                    Log.Debug(TimeInfo.Now, $"{GetCitizenDesc(citizenId, ref citizen)} is going from {currentBuilding} to school/work {schedule.WorkBuilding} and will go to lunch at {schedule.ScheduledStateTime}");
                }
                else
                {
                    workBehavior.ScheduleReturnFromWork(ref schedule, citizenAge);
                    Log.Debug(TimeInfo.Now, $"{GetCitizenDesc(citizenId, ref citizen)} is going from {currentBuilding} to school/work {schedule.WorkBuilding} and will leave work at {schedule.ScheduledStateTime}");
                }
            }
            else
            {
                Log.Debug(TimeInfo.Now, $"{GetCitizenDesc(citizenId, ref citizen)} wanted to go to work from {currentBuilding} but can't, will try once again next time");
                schedule.Schedule(ResidentState.Unknown);
            }
        }

        private void DoScheduledLunch(ref CitizenSchedule schedule, TAI instance, uint citizenId, ref TCitizen citizen)
        {
            ushort currentBuilding = CitizenProxy.GetCurrentBuilding(ref citizen);
#if DEBUG
            string citizenDesc = GetCitizenDesc(citizenId, ref citizen);
#else
            string citizenDesc = null;
#endif
            ushort lunchPlace = MoveToCommercialBuilding(instance, citizenId, ref citizen, LocalSearchDistance);
            if (lunchPlace != 0)
            {
                Log.Debug(TimeInfo.Now, $"{citizenDesc} is going for lunch from {currentBuilding} to {lunchPlace}");
                workBehavior.ScheduleReturnFromLunch(ref schedule);
            }
            else
            {
                Log.Debug(TimeInfo.Now, $"{citizenDesc} wanted to go for lunch from {currentBuilding}, but there were no buildings close enough");
                workBehavior.ScheduleReturnFromWork(ref schedule, CitizenProxy.GetAge(ref citizen));
            }
        }
    }
}
