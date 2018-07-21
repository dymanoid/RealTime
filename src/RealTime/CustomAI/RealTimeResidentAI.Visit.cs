// <copyright file="RealTimeResidentAI.Visit.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.CustomAI
{
    using System;
    using RealTime.Events;
    using RealTime.Tools;
    using static Constants;

    internal sealed partial class RealTimeResidentAI<TAI, TCitizen>
    {
        private bool ScheduleRelaxing(ref CitizenSchedule schedule, uint citizenId, ref TCitizen citizen)
        {
            Citizen.AgeGroup citizenAge = CitizenProxy.GetAge(ref citizen);
            if (!Random.ShouldOccur(GetGoOutChance(citizenAge)) || IsBadWeather())
            {
                return false;
            }

            ICityEvent cityEvent = GetUpcomingEventToAttend(citizenId, ref citizen);
            if (cityEvent != null)
            {
                ushort currentBuilding = CitizenProxy.GetCurrentBuilding(ref citizen);
                DateTime departureTime = cityEvent.StartTime.AddHours(-GetEstimatedTravelTime(currentBuilding, cityEvent.BuildingId));
                schedule.Schedule(ResidentState.Relaxing, departureTime);
                schedule.EventBuilding = cityEvent.BuildingId;
                schedule.Hint = ScheduleHint.AttendingEvent;
                return true;
            }

            schedule.Schedule(ResidentState.Relaxing, default);
            schedule.Hint = TimeInfo.IsNightTime
                ? ScheduleHint.RelaxAtLeisureBuilding
                : ScheduleHint.None;

            return true;
        }

        private void DoScheduledRelaxing(ref CitizenSchedule schedule, TAI instance, uint citizenId, ref TCitizen citizen)
        {
            ushort buildingId = CitizenProxy.GetCurrentBuilding(ref citizen);

            switch (schedule.Hint)
            {
                case ScheduleHint.RelaxAtLeisureBuilding:
                    schedule.Schedule(ResidentState.Unknown, default);

                    ushort leisure = MoveToLeisureBuilding(instance, citizenId, ref citizen, buildingId);
                    if (leisure != 0)
                    {
                        Log.Debug(TimeInfo.Now, $"{GetCitizenDesc(citizenId, ref citizen)} heading to a leisure building {leisure}");
                    }

                    return;

                case ScheduleHint.AttendingEvent:
                    Log.Debug(TimeInfo.Now, $"{GetCitizenDesc(citizenId, ref citizen)} wanna attend an event at '{schedule.EventBuilding}', on the way now.");
                    StartMovingToVisitBuilding(instance, citizenId, ref citizen, schedule.EventBuilding);
                    ICityEvent cityEvent = EventMgr.GetUpcomingCityEvent(schedule.EventBuilding);
                    DateTime returnTime = cityEvent == null
                        ? default
                        : cityEvent.EndTime;

                    schedule.Schedule(ResidentState.Unknown, returnTime);
                    schedule.EventBuilding = 0;
                    return;
            }

            ResidentState nextState = Random.ShouldOccur(ReturnFromVisitChance)
                    ? ResidentState.Unknown
                    : ResidentState.Relaxing;

            schedule.Schedule(nextState, default);

            if (schedule.CurrentState != ResidentState.Relaxing)
            {
                Log.Debug(TimeInfo.Now, $"{GetCitizenDesc(citizenId, ref citizen)} wanna relax, heading to an entertainment building.");
                residentAI.FindVisitPlace(instance, citizenId, buildingId, residentAI.GetEntertainmentReason(instance));
            }
        }

        private void ProcessCitizenRelaxing(ref TCitizen citizen)
        {
            ushort currentBuilding = CitizenProxy.GetVisitBuilding(ref citizen);
            if (CitizenProxy.HasFlags(ref citizen, Citizen.Flags.NeedGoods)
                && BuildingMgr.GetBuildingSubService(currentBuilding) == ItemClass.SubService.CommercialLeisure)
            {
                // No Citizen.Flags.NeedGoods flag reset here, because we only bought 'beer' or 'champagne' in a leisure building.
                BuildingMgr.ModifyMaterialBuffer(currentBuilding, TransferManager.TransferReason.Shopping, -ShoppingGoodsAmount);
            }
        }

        private bool ScheduleShopping(ref CitizenSchedule schedule, ref TCitizen citizen, bool localOnly)
        {
            if (!CitizenProxy.HasFlags(ref citizen, Citizen.Flags.NeedGoods) || IsBadWeather())
            {
                return false;
            }

            if (TimeInfo.IsNightTime)
            {
                if (Random.ShouldOccur(GetGoOutChance(CitizenProxy.GetAge(ref citizen))))
                {
                    schedule.Hint = ScheduleHint.LocalShoppingOnly;
                    schedule.Schedule(ResidentState.Shopping, default);
                    return true;
                }

                return false;
            }

            schedule.Hint = ScheduleHint.None;
            if (Random.ShouldOccur(GoShoppingChance))
            {
                if (localOnly || Random.ShouldOccur(Config.LocalBuildingSearchQuota))
                {
                    schedule.Hint = ScheduleHint.LocalShoppingOnly;
                }

                schedule.Schedule(ResidentState.Shopping, default);
                return true;
            }

            return false;
        }

        private void DoScheduledShopping(ref CitizenSchedule schedule, TAI instance, uint citizenId, ref TCitizen citizen)
        {
            ushort currentBuilding = CitizenProxy.GetCurrentBuilding(ref citizen);

            if ((schedule.Hint & ScheduleHint.LocalShoppingOnly) != 0)
            {
                schedule.Schedule(ResidentState.Unknown, default);

                ushort shop = MoveToCommercialBuilding(instance, citizenId, ref citizen, LocalSearchDistance);
                if (shop == 0)
                {
                    Log.Debug(TimeInfo.Now, $"{GetCitizenDesc(citizenId, ref citizen)} wanted go shopping, but didn't find a local shop");
                }
            }
            else
            {
                ResidentState nextState = Random.ShouldOccur(ReturnFromShoppingChance)
                    ? ResidentState.Unknown
                    : ResidentState.Shopping;

                schedule.Schedule(nextState, default);

                if (schedule.CurrentState != ResidentState.Shopping)
                {
                    Log.Debug(TimeInfo.Now, $"{GetCitizenDesc(citizenId, ref citizen)} wanna go shopping, heading to a random shop");
                    residentAI.FindVisitPlace(instance, citizenId, currentBuilding, residentAI.GetShoppingReason(instance));
                }
            }
        }

        private void ProcessCitizenShopping(ref TCitizen citizen)
        {
            if (!CitizenProxy.HasFlags(ref citizen, Citizen.Flags.NeedGoods))
            {
                return;
            }

            ushort shop = CitizenProxy.GetVisitBuilding(ref citizen);
            if (shop == 0)
            {
                return;
            }

            BuildingMgr.ModifyMaterialBuffer(shop, TransferManager.TransferReason.Shopping, -ShoppingGoodsAmount);
            CitizenProxy.RemoveFlags(ref citizen, Citizen.Flags.NeedGoods);
        }

        private void ProcessCitizenVisit(ref CitizenSchedule schedule, ref TCitizen citizen)
        {
            ushort visitBuilding = CitizenProxy.GetVisitBuilding(ref citizen);

            if (schedule.Hint == ScheduleHint.OnTour
                || Random.ShouldOccur(ReturnFromVisitChance)
                || (BuildingMgr.GetBuildingSubService(visitBuilding) == ItemClass.SubService.CommercialLeisure
                    && TimeInfo.IsNightTime
                    && BuildingMgr.IsBuildingNoiseRestricted(visitBuilding)))
            {
                schedule.Schedule(ResidentState.Unknown, default);
            }
            else
            {
                schedule.Schedule(ResidentState.Visiting, default);
            }
        }

        private bool IsBuildingNoiseRestricted(ushort building)
        {
            float arriveHour = (float)TimeInfo.Now.AddHours(MaxTravelTime).TimeOfDay.TotalHours;
            return (arriveHour >= Config.GoToSleepUpHour || TimeInfo.CurrentHour >= Config.GoToSleepUpHour
                || arriveHour <= Config.WakeupHour || TimeInfo.CurrentHour <= Config.WakeupHour)
                && BuildingMgr.IsBuildingNoiseRestricted(building);
        }
    }
}
