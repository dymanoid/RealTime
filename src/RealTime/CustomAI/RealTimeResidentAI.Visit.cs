// <copyright file="RealTimeResidentAI.Visit.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.CustomAI
{
    using System;
    using RealTime.Events;
    using SkyTools.Tools;
    using static Constants;

    internal sealed partial class RealTimeResidentAI<TAI, TCitizen>
    {
        private bool ScheduleRelaxing(ref CitizenSchedule schedule, uint citizenId, ref TCitizen citizen)
        {
            Citizen.AgeGroup citizenAge = CitizenProxy.GetAge(ref citizen);

            uint relaxChance = spareTimeBehavior.GetRelaxingChance(citizenAge, schedule.WorkShift, schedule.WorkStatus == WorkStatus.OnVacation);
            if (!Random.ShouldOccur(relaxChance) || IsBadWeather())
            {
                return false;
            }

            ICityEvent cityEvent = GetUpcomingEventToAttend(citizenId, ref citizen);
            if (cityEvent != null)
            {
                ushort currentBuilding = CitizenProxy.GetCurrentBuilding(ref citizen);
                DateTime departureTime = cityEvent.StartTime.AddHours(-travelBehavior.GetEstimatedTravelTime(currentBuilding, cityEvent.BuildingId));
                schedule.Schedule(ResidentState.Relaxing, departureTime);
                schedule.EventBuilding = cityEvent.BuildingId;
                schedule.Hint = ScheduleHint.AttendingEvent;
                return true;
            }

            schedule.Schedule(ResidentState.Relaxing);
            schedule.Hint = TimeInfo.IsNightTime
                ? ScheduleHint.RelaxAtLeisureBuilding
                : ScheduleHint.None;

            return true;
        }

        private bool DoScheduledRelaxing(ref CitizenSchedule schedule, TAI instance, uint citizenId, ref TCitizen citizen)
        {
            // Relaxing was already scheduled last time, but the citizen is still at school/work.
            // This can occur when the game's transfer manager can't find any activity for the citizen.
            // In that case, move back home.
            if (schedule.CurrentState == ResidentState.AtSchoolOrWork && schedule.LastScheduledState == ResidentState.Relaxing)
            {
                Log.Debug(LogCategory.Movement, TimeInfo.Now, $"{GetCitizenDesc(citizenId, ref citizen)} wanted relax but is still at work. No relaxing activity found. Now going home.");
                return false;
            }

            ushort buildingId = CitizenProxy.GetCurrentBuilding(ref citizen);
            switch (schedule.Hint)
            {
                case ScheduleHint.RelaxAtLeisureBuilding:
                    schedule.Schedule(ResidentState.Unknown);

                    ushort leisure = MoveToLeisureBuilding(instance, citizenId, ref citizen, buildingId);
                    if (leisure == 0)
                    {
                        Log.Debug(LogCategory.Movement, TimeInfo.Now, $"{GetCitizenDesc(citizenId, ref citizen)} wanted relax but didn't find a leisure building");
                        return false;
                    }

                    Log.Debug(LogCategory.Movement, TimeInfo.Now, $"{GetCitizenDesc(citizenId, ref citizen)} heading to a leisure building {leisure}");
                    return true;

                case ScheduleHint.AttendingEvent:
                    ushort eventBuilding = schedule.EventBuilding;
                    schedule.EventBuilding = 0;

                    ICityEvent cityEvent = EventMgr.GetCityEvent(eventBuilding);
                    if (cityEvent == null)
                    {
                        Log.Debug(LogCategory.Events, TimeInfo.Now, $"{GetCitizenDesc(citizenId, ref citizen)} wanted attend an event at '{eventBuilding}', but there was no event there");
                    }
                    else if (StartMovingToVisitBuilding(instance, citizenId, ref citizen, eventBuilding))
                    {
                        schedule.Schedule(ResidentState.Unknown, cityEvent.EndTime);
                        Log.Debug(LogCategory.Events, TimeInfo.Now, $"{GetCitizenDesc(citizenId, ref citizen)} wanna attend an event at '{eventBuilding}', will return at {cityEvent.EndTime}");
                        return true;
                    }

                    schedule.Schedule(ResidentState.Unknown);
                    return false;
            }

            uint relaxChance = spareTimeBehavior.GetRelaxingChance(
                CitizenProxy.GetAge(ref citizen),
                schedule.WorkShift,
                schedule.WorkStatus == WorkStatus.OnVacation);

            ResidentState nextState = Random.ShouldOccur(relaxChance)
                    ? ResidentState.Relaxing
                    : ResidentState.Unknown;

            schedule.Schedule(nextState);
            if (schedule.CurrentState != ResidentState.Relaxing || Random.ShouldOccur(FindAnotherShopOrEntertainmentChance))
            {
                Log.Debug(LogCategory.Movement, TimeInfo.Now, $"{GetCitizenDesc(citizenId, ref citizen)} in state {schedule.CurrentState} wanna relax and then schedules {nextState}, heading to an entertainment building.");
                residentAI.FindVisitPlace(instance, citizenId, buildingId, residentAI.GetEntertainmentReason(instance));
            }
#if DEBUG
            else
            {
                Log.Debug(LogCategory.Movement, TimeInfo.Now, $"{GetCitizenDesc(citizenId, ref citizen)} continues relaxing in the same entertainment building.");
            }
#endif

            return true;
        }

        private bool ProcessCitizenRelaxing(ref CitizenSchedule schedule, uint citizenId, ref TCitizen citizen)
        {
            ushort currentBuilding = CitizenProxy.GetVisitBuilding(ref citizen);
            if (CitizenProxy.HasFlags(ref citizen, Citizen.Flags.NeedGoods)
                && BuildingMgr.GetBuildingSubService(currentBuilding) == ItemClass.SubService.CommercialLeisure)
            {
                // No Citizen.Flags.NeedGoods flag reset here, because we only bought 'beer' or 'champagne' in a leisure building.
                BuildingMgr.ModifyMaterialBuffer(currentBuilding, TransferManager.TransferReason.Shopping, -ShoppingGoodsAmount);
            }

            return RescheduleVisit(ref schedule, citizenId, ref citizen, currentBuilding);
        }

        private bool ScheduleShopping(ref CitizenSchedule schedule, ref TCitizen citizen, bool localOnly)
        {
            // If the citizen doesn't need any goods, he/she still can go shopping just for fun
            if (!CitizenProxy.HasFlags(ref citizen, Citizen.Flags.NeedGoods))
            {
                if (schedule.Hint == ScheduleHint.NoShoppingAnyMore || IsBadWeather() || !Random.ShouldOccur(Config.ShoppingForFunQuota))
                {
                    schedule.Hint = ScheduleHint.None;
                    return false;
                }

                schedule.Hint = ScheduleHint.NoShoppingAnyMore;
            }
            else
            {
                schedule.Hint = ScheduleHint.None;
            }

            if (!Random.ShouldOccur(spareTimeBehavior.GetShoppingChance(CitizenProxy.GetAge(ref citizen))))
            {
                return false;
            }

            if (TimeInfo.IsNightTime || localOnly || Random.ShouldOccur(Config.LocalBuildingSearchQuota))
            {
                schedule.Hint = ScheduleHint.LocalShoppingOnly;
            }

            schedule.Schedule(ResidentState.Shopping);
            return true;
        }

        private bool DoScheduledShopping(ref CitizenSchedule schedule, TAI instance, uint citizenId, ref TCitizen citizen)
        {
            // Shopping was already scheduled last time, but the citizen is still at school/work.
            // This can occur when the game's transfer manager can't find any activity for the citizen.
            // In that case, move back home.
            if (schedule.CurrentState == ResidentState.AtSchoolOrWork && schedule.LastScheduledState == ResidentState.Shopping)
            {
                Log.Debug(LogCategory.Movement, TimeInfo.Now, $"{GetCitizenDesc(citizenId, ref citizen)} wanted go shopping but is still at work. No shopping activity found. Now going home.");
                return false;
            }

            ushort currentBuilding = CitizenProxy.GetCurrentBuilding(ref citizen);
            if (schedule.Hint == ScheduleHint.LocalShoppingOnly)
            {
                schedule.Schedule(ResidentState.Unknown);

                ushort shop = MoveToCommercialBuilding(instance, citizenId, ref citizen, LocalSearchDistance);
                if (shop == 0)
                {
                    Log.Debug(LogCategory.Movement, TimeInfo.Now, $"{GetCitizenDesc(citizenId, ref citizen)} wanted go shopping, but didn't find a local shop");
                    return false;
                }

                if (TimeInfo.IsNightTime)
                {
                    schedule.Hint = ScheduleHint.NoShoppingAnyMore;
                }

                Log.Debug(LogCategory.Movement, TimeInfo.Now, $"{GetCitizenDesc(citizenId, ref citizen)} goes shopping at a local shop {shop}");
                return true;
            }

            uint moreShoppingChance = spareTimeBehavior.GetShoppingChance(CitizenProxy.GetAge(ref citizen));
            ResidentState nextState = schedule.Hint != ScheduleHint.NoShoppingAnyMore && Random.ShouldOccur(moreShoppingChance)
                ? ResidentState.Shopping
                : ResidentState.Unknown;

            schedule.Schedule(nextState);

            if (schedule.CurrentState != ResidentState.Shopping || Random.ShouldOccur(FindAnotherShopOrEntertainmentChance))
            {
                Log.Debug(LogCategory.Movement, TimeInfo.Now, $"{GetCitizenDesc(citizenId, ref citizen)} in state {schedule.CurrentState} wanna go shopping and schedules {nextState}, heading to a random shop, hint = {schedule.Hint}");
                residentAI.FindVisitPlace(instance, citizenId, currentBuilding, residentAI.GetShoppingReason(instance));
            }
#if DEBUG
            else
            {
                Log.Debug(LogCategory.Movement, TimeInfo.Now, $"{GetCitizenDesc(citizenId, ref citizen)} continues shopping in the same building.");
            }
#endif

            return true;
        }

        private bool ProcessCitizenShopping(ref CitizenSchedule schedule, uint citizenId, ref TCitizen citizen)
        {
            ushort currentBuilding = CitizenProxy.GetVisitBuilding(ref citizen);
            if (CitizenProxy.HasFlags(ref citizen, Citizen.Flags.NeedGoods) && currentBuilding != 0)
            {
                BuildingMgr.ModifyMaterialBuffer(currentBuilding, TransferManager.TransferReason.Shopping, -ShoppingGoodsAmount);
                CitizenProxy.RemoveFlags(ref citizen, Citizen.Flags.NeedGoods);
            }

            return RescheduleVisit(ref schedule, citizenId, ref citizen, currentBuilding);
        }

        private bool ProcessCitizenVisit(ref CitizenSchedule schedule, uint citizenId, ref TCitizen citizen)
        {
            if (schedule.Hint == ScheduleHint.OnTour)
            {
                Log.Debug(LogCategory.Movement, TimeInfo.Now, $"{GetCitizenDesc(citizenId, ref citizen)} quits a tour");
                schedule.Schedule(ResidentState.Unknown);
                return true;
            }

            return RescheduleVisit(ref schedule, citizenId, ref citizen, CitizenProxy.GetVisitBuilding(ref citizen));
        }

        private bool RescheduleVisit(ref CitizenSchedule schedule, uint citizenId, ref TCitizen citizen, ushort currentBuilding)
        {
            switch (schedule.ScheduledState)
            {
                case ResidentState.Shopping:
                case ResidentState.Relaxing:
                case ResidentState.Visiting:
                    break;

                default:
                    return false;
            }

            if (schedule.CurrentState != ResidentState.Shopping && IsBadWeather())
            {
                Log.Debug(LogCategory.Movement, TimeInfo.Now, $"{GetCitizenDesc(citizenId, ref citizen)} quits a visit because of bad weather");
                schedule.Schedule(ResidentState.AtHome);
                return true;
            }

            if (buildingAI.IsNoiseRestricted(currentBuilding, currentBuilding))
            {
                Log.Debug(LogCategory.Movement, TimeInfo.Now, $"{GetCitizenDesc(citizenId, ref citizen)} quits a visit because of NIMBY policy");
                schedule.Schedule(ResidentState.Unknown);
                return true;
            }

            Citizen.AgeGroup age = CitizenProxy.GetAge(ref citizen);
            uint stayChance = schedule.CurrentState == ResidentState.Shopping
                ? spareTimeBehavior.GetShoppingChance(age)
                : spareTimeBehavior.GetRelaxingChance(age, schedule.WorkShift, schedule.WorkStatus == WorkStatus.OnVacation);

            if (!Random.ShouldOccur(stayChance))
            {
                Log.Debug(LogCategory.Movement, TimeInfo.Now, $"{GetCitizenDesc(citizenId, ref citizen)} quits a visit because of time");
                schedule.Schedule(ResidentState.AtHome);
                return true;
            }

            return false;
        }
    }
}
