// <copyright file="RealTimeResidentAI.Home.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.CustomAI
{
    using RealTime.Tools;

    internal sealed partial class RealTimeResidentAI<TAI, TCitizen>
    {
        private void DoScheduledHome(ref CitizenSchedule schedule, TAI instance, uint citizenId, ref TCitizen citizen)
        {
            ushort homeBuilding = CitizenProxy.GetHomeBuilding(ref citizen);
            if (homeBuilding == 0)
            {
                Log.Debug($"WARNING: {GetCitizenDesc(citizenId, ref citizen)} is in corrupt state: want to go home with no home building. Releasing the poor citizen.");
                CitizenMgr.ReleaseCitizen(citizenId);
                schedule = default;
                return;
            }

            ushort currentBuilding = CitizenProxy.GetCurrentBuilding(ref citizen);
            CitizenProxy.RemoveFlags(ref citizen, Citizen.Flags.Evacuating);

            if (residentAI.StartMoving(instance, citizenId, ref citizen, currentBuilding, homeBuilding))
            {
                CitizenProxy.SetVisitPlace(ref citizen, citizenId, 0);
                schedule.Schedule(ResidentState.Unknown, default);
                Log.Debug(TimeInfo.Now, $"{GetCitizenDesc(citizenId, ref citizen)} is going from {currentBuilding} back home");
            }
            else
            {
                Log.Debug(TimeInfo.Now, $"{GetCitizenDesc(citizenId, ref citizen)} wanted to go home from {currentBuilding} but can't, waiting for the next opportunity");
            }
        }

        private bool RescheduleAtHome(ref CitizenSchedule schedule, ref TCitizen citizen)
        {
            if (schedule.CurrentState != ResidentState.AtHome || TimeInfo.Now < schedule.ScheduledStateTime)
            {
                return false;
            }

            if (schedule.ScheduledState != ResidentState.Relaxing && schedule.ScheduledState != ResidentState.Shopping)
            {
                return false;
            }

            if (schedule.ScheduledState != ResidentState.Shopping && IsBadWeather())
            {
                Log.Debug(TimeInfo.Now, $"{GetCitizenDesc(0, ref citizen)} re-schedules an activity because of bad weather (see next line for citizen ID)");
                schedule.Schedule(ResidentState.Unknown, default);
                return true;
            }

            var age = CitizenProxy.GetAge(ref citizen);
            uint goOutChance = schedule.ScheduledState == ResidentState.Shopping
                ? spareTimeBehavior.GetShoppingChance(age)
                : spareTimeBehavior.GetRelaxingChance(age, schedule.WorkShift);

            if (Random.ShouldOccur(goOutChance))
            {
                return false;
            }

            Log.Debug(TimeInfo.Now, $"{GetCitizenDesc(0, ref citizen)} re-schedules an activity because of time (see next line for citizen ID)");
            schedule.Schedule(ResidentState.Unknown, default);
            return true;
        }
    }
}
