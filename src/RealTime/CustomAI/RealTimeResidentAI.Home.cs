// <copyright file="RealTimeResidentAI.Home.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.CustomAI
{
    using ColossalFramework;
    using SkyTools.Tools;

    internal sealed partial class RealTimeResidentAI<TAI, TCitizen>
    {
        private void DoScheduledHome(ref CitizenSchedule schedule, TAI instance, uint citizenId, ref TCitizen citizen, bool doNotReleaseCitizen = false)
        {
            ushort homeBuilding = CitizenProxy.GetHomeBuilding(ref citizen);
            if (homeBuilding == 0)
            {
                if (!doNotReleaseCitizen)
                {
                    Log.Debug(LogCategory.State, $"WARNING: {GetCitizenDesc(citizenId, ref citizen)} is in corrupt state: want to go home with no home building. Releasing the poor citizen.");
                    CitizenMgr.ReleaseCitizen(citizenId);
                    schedule = default;
                    return;
                }
            }

            ushort currentBuilding = CitizenProxy.GetCurrentBuilding(ref citizen);
            CitizenProxy.RemoveFlags(ref citizen, Citizen.Flags.Evacuating);

            if (residentAI.StartMoving(instance, citizenId, ref citizen, currentBuilding, homeBuilding))
            {
                CitizenProxy.SetVisitPlace(ref citizen, citizenId, 0);
                schedule.Schedule(ResidentState.Unknown);
                Log.Debug(LogCategory.Movement, TimeInfo.Now, $"{GetCitizenDesc(citizenId, ref citizen)} is going from {currentBuilding} back home");
            }
            else
            {
                Log.Debug(LogCategory.Movement, TimeInfo.Now, $"{GetCitizenDesc(citizenId, ref citizen)} wanted to go home from {currentBuilding} but can't, waiting for the next opportunity");
            }
        }

        private bool RescheduleAtHome(ref CitizenSchedule schedule, uint citizenId, ref TCitizen citizen)
        {
            if (schedule.CurrentState != ResidentState.AtHome || TimeInfo.Now < schedule.ScheduledStateTime)
            {
                return false;
            }

            if (schedule.ScheduledState != ResidentState.Relaxing && schedule.ScheduledState != ResidentState.Shopping)
            {
                return false;
            }

            if (schedule.ScheduledState != ResidentState.Shopping && WeatherInfo.IsBadWeather)
            {
                Log.Debug(LogCategory.Schedule, TimeInfo.Now, $"{GetCitizenDesc(citizenId, ref citizen)} re-schedules an activity because of bad weather");
                schedule.Schedule(ResidentState.Unknown);
                return true;
            }

            Citizen.AgeGroup age = CitizenProxy.GetAge(ref citizen);
            uint goingOutChance = schedule.ScheduledState == ResidentState.Shopping
                ? spareTimeBehavior.GetShoppingChance(age)
                : spareTimeBehavior.GetRelaxingChance(age, schedule.WorkShift);

            if (goingOutChance > 0)
            {
                return false;
            }

            Log.Debug(LogCategory.Schedule, TimeInfo.Now, $"{GetCitizenDesc(citizenId, ref citizen)} re-schedules an activity because of time");
            schedule.Schedule(ResidentState.Unknown);
            return true;
        }
    }
}
