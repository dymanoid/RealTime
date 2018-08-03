// <copyright file="RealTimeResidentAI.SchoolWork.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.CustomAI
{
    using SkyTools.Tools;
    using static Constants;

    internal sealed partial class RealTimeResidentAI<TAI, TCitizen>
    {
        private readonly uint[] familyBuffer = new uint[4];

        private bool ScheduleWork(ref CitizenSchedule schedule, ref TCitizen citizen)
        {
            ushort currentBuilding = CitizenProxy.GetCurrentBuilding(ref citizen);
            if (!workBehavior.ScheduleGoToWork(ref schedule, currentBuilding, simulationCycle))
            {
                return false;
            }

            Log.Debug(LogCategory.Schedule, $"  - Schedule work at {schedule.ScheduledStateTime}");

            float timeLeft = (float)(schedule.ScheduledStateTime - TimeInfo.Now).TotalHours;
            if (timeLeft <= PrepareToWorkHours)
            {
                // Just sit at home if the work time will come soon
                Log.Debug(LogCategory.Schedule, $"  - Work time in {timeLeft} hours, preparing for departure");
                return true;
            }

            if (timeLeft <= MaxTravelTime)
            {
                if (schedule.CurrentState != ResidentState.AtHome)
                {
                    Log.Debug(LogCategory.Schedule, $"  - Work time in {timeLeft} hours, returning home");
                    schedule.Schedule(ResidentState.AtHome);
                    return true;
                }

                // If we have some time, try to shop locally.
                if (ScheduleShopping(ref schedule, ref citizen, true))
                {
                    Log.Debug(LogCategory.Schedule, $"  - Work time in {timeLeft} hours, trying local shop");
                }
                else
                {
                    Log.Debug(LogCategory.Schedule, $"  - Work time in {timeLeft} hours, doing nothing");
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
                    Log.Debug(LogCategory.Movement, TimeInfo.Now, $"{GetCitizenDesc(citizenId, ref citizen)} is going from {currentBuilding} to school/work {schedule.WorkBuilding} and will go to lunch at {schedule.ScheduledStateTime}");
                }
                else
                {
                    workBehavior.ScheduleReturnFromWork(ref schedule, citizenAge);
                    Log.Debug(LogCategory.Movement, TimeInfo.Now, $"{GetCitizenDesc(citizenId, ref citizen)} is going from {currentBuilding} to school/work {schedule.WorkBuilding} and will leave work at {schedule.ScheduledStateTime}");
                }
            }
            else
            {
                Log.Debug(LogCategory.Movement, TimeInfo.Now, $"{GetCitizenDesc(citizenId, ref citizen)} wanted to go to work from {currentBuilding} but can't, will try once again next time");
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
                Log.Debug(LogCategory.Movement, TimeInfo.Now, $"{citizenDesc} is going for lunch from {currentBuilding} to {lunchPlace}");
                workBehavior.ScheduleReturnFromLunch(ref schedule);
            }
            else
            {
                Log.Debug(LogCategory.Movement, TimeInfo.Now, $"{citizenDesc} wanted to go for lunch from {currentBuilding}, but there were no buildings close enough");
                workBehavior.ScheduleReturnFromWork(ref schedule, CitizenProxy.GetAge(ref citizen));
            }
        }

        private void ProcessVacation(uint citizenId)
        {
            ref CitizenSchedule schedule = ref residentSchedules[citizenId];

            // Note: this might lead to different vacation durations for family members even if they all were initialized to same length.
            // This is because the simulation loop for a family member could process this citizen right after their vacation has been set.
            // But we intentionally don't avoid this - let's add some randomness.
            if (schedule.VacationDaysLeft > 0)
            {
                --schedule.VacationDaysLeft;
                if (schedule.VacationDaysLeft == 0)
                {
                    Log.Debug(LogCategory.State, $"The citizen {citizenId} returns from vacation");
                    schedule.WorkStatus = WorkStatus.None;
                }

                return;
            }

            // We do allow processing on weekends, because some citizens work on weekends and might be on vacation too
            if (schedule.WorkBuilding == 0)
            {
                return;
            }

            Citizen.Wealth wealth = CitizenMgr.GetCitizenWealth(citizenId);
            if (!Random.ShouldOccurPrecise(spareTimeBehavior.GetPreciseVacationChance(wealth)))
            {
                return;
            }

            int days = 1 + Random.GetRandomValue(Config.MaxVacationLength - 1);
            schedule.WorkStatus = WorkStatus.OnVacation;
            schedule.VacationDaysLeft = (byte)days;

            Log.Debug(LogCategory.State, $"The citizen {citizenId} is now on vacation for {days} days");
            if (!Random.ShouldOccur(FamilyVacationChance)
                || !CitizenMgr.TryGetFamily(citizenId, familyBuffer))
            {
                return;
            }

            for (int i = 0; i < familyBuffer.Length; ++i)
            {
                uint familyMemberId = familyBuffer[i];
                if (familyMemberId != 0)
                {
                    Log.Debug(LogCategory.State, $"The citizen {familyMemberId} goes on vacation with {citizenId} as a family member");
                    residentSchedules[familyMemberId].WorkStatus = WorkStatus.OnVacation;
                    residentSchedules[familyMemberId].VacationDaysLeft = (byte)days;
                }
            }
        }
    }
}
