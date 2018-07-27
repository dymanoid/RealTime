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
            if (!Random.ShouldOccur(spareTimeBehavior.GetRelaxingChance(citizenAge, schedule.WorkShift)) || IsBadWeather())
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
                    if (leisure == 0)
                    {
                        Log.Debug(TimeInfo.Now, $"{GetCitizenDesc(citizenId, ref citizen)} wanted relax but didn't find a leisure building");
                    }
                    else
                    {
                        Log.Debug(TimeInfo.Now, $"{GetCitizenDesc(citizenId, ref citizen)} heading to a leisure building {leisure}");
                    }

                    return;

                case ScheduleHint.AttendingEvent:
                    DateTime returnTime = default;
                    ICityEvent cityEvent = EventMgr.GetCityEvent(schedule.EventBuilding);
                    if (cityEvent == null)
                    {
                        Log.Debug(TimeInfo.Now, $"{GetCitizenDesc(citizenId, ref citizen)} wanted attend an event at '{schedule.EventBuilding}', but there was no event there");
                    }
                    else if (StartMovingToVisitBuilding(instance, citizenId, ref citizen, schedule.EventBuilding))
                    {
                        returnTime = cityEvent.EndTime;
                        Log.Debug(TimeInfo.Now, $"{GetCitizenDesc(citizenId, ref citizen)} wanna attend an event at '{schedule.EventBuilding}', will return at {returnTime}");
                    }

                    schedule.Schedule(ResidentState.Unknown, returnTime);
                    schedule.EventBuilding = 0;
                    return;
            }

            uint relaxChance = spareTimeBehavior.GetRelaxingChance(CitizenProxy.GetAge(ref citizen), schedule.WorkShift);
            ResidentState nextState = Random.ShouldOccur(relaxChance)
                    ? ResidentState.Relaxing
                    : ResidentState.Unknown;

            schedule.Schedule(nextState, default);

            if (schedule.CurrentState != ResidentState.Relaxing)
            {
                Log.Debug(TimeInfo.Now, $"{GetCitizenDesc(citizenId, ref citizen)} in state {schedule.CurrentState} wanna relax and then schedules {nextState}, heading to an entertainment building.");
                residentAI.FindVisitPlace(instance, citizenId, buildingId, residentAI.GetEntertainmentReason(instance));
            }
        }

        private bool ProcessCitizenRelaxing(ref CitizenSchedule schedule, ref TCitizen citizen)
        {
            ushort currentBuilding = CitizenProxy.GetVisitBuilding(ref citizen);
            if (CitizenProxy.HasFlags(ref citizen, Citizen.Flags.NeedGoods)
                && BuildingMgr.GetBuildingSubService(currentBuilding) == ItemClass.SubService.CommercialLeisure)
            {
                // No Citizen.Flags.NeedGoods flag reset here, because we only bought 'beer' or 'champagne' in a leisure building.
                BuildingMgr.ModifyMaterialBuffer(currentBuilding, TransferManager.TransferReason.Shopping, -ShoppingGoodsAmount);
            }

            return RescheduleVisit(ref schedule, ref citizen, currentBuilding);
        }

        private bool ScheduleShopping(ref CitizenSchedule schedule, ref TCitizen citizen, bool localOnly)
        {
            // If the citizen doesn't need any good, he/she still can go shopping just for fun
            if (!CitizenProxy.HasFlags(ref citizen, Citizen.Flags.NeedGoods))
            {
                if (schedule.Hint == ScheduleHint.NoShoppingAnyMore || !Random.ShouldOccur(FunShoppingChance))
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

            schedule.Schedule(ResidentState.Shopping, default);
            return true;
        }

        private void DoScheduledShopping(ref CitizenSchedule schedule, TAI instance, uint citizenId, ref TCitizen citizen)
        {
            ushort currentBuilding = CitizenProxy.GetCurrentBuilding(ref citizen);

            if (schedule.Hint == ScheduleHint.LocalShoppingOnly)
            {
                schedule.Schedule(ResidentState.Unknown, default);

                ushort shop = MoveToCommercialBuilding(instance, citizenId, ref citizen, LocalSearchDistance);
                if (shop == 0)
                {
                    Log.Debug(TimeInfo.Now, $"{GetCitizenDesc(citizenId, ref citizen)} wanted go shopping, but didn't find a local shop");
                }
                else
                {
                    if (TimeInfo.IsNightTime)
                    {
                        schedule.Hint = ScheduleHint.NoShoppingAnyMore;
                    }

                    Log.Debug(TimeInfo.Now, $"{GetCitizenDesc(citizenId, ref citizen)} goes shopping at a local shop {shop}");
                }
            }
            else
            {
                uint moreShoppingChance = spareTimeBehavior.GetShoppingChance(CitizenProxy.GetAge(ref citizen));
                ResidentState nextState = schedule.Hint != ScheduleHint.NoShoppingAnyMore && Random.ShouldOccur(moreShoppingChance)
                    ? ResidentState.Shopping
                    : ResidentState.Unknown;

                schedule.Schedule(nextState, default);

                if (schedule.CurrentState != ResidentState.Shopping)
                {
                    Log.Debug(TimeInfo.Now, $"{GetCitizenDesc(citizenId, ref citizen)} in state {schedule.CurrentState} wanna go shopping and schedules {nextState}, heading to a random shop, hint = {schedule.Hint}");
                    residentAI.FindVisitPlace(instance, citizenId, currentBuilding, residentAI.GetShoppingReason(instance));
                }
            }
        }

        private bool ProcessCitizenShopping(ref CitizenSchedule schedule, ref TCitizen citizen)
        {
            ushort currentBuilding = CitizenProxy.GetVisitBuilding(ref citizen);
            if (CitizenProxy.HasFlags(ref citizen, Citizen.Flags.NeedGoods) && currentBuilding != 0)
            {
                BuildingMgr.ModifyMaterialBuffer(currentBuilding, TransferManager.TransferReason.Shopping, -ShoppingGoodsAmount);
                CitizenProxy.RemoveFlags(ref citizen, Citizen.Flags.NeedGoods);
            }

            return RescheduleVisit(ref schedule, ref citizen, currentBuilding);
        }

        private bool ProcessCitizenVisit(ref CitizenSchedule schedule, ref TCitizen citizen)
        {
            if (schedule.Hint == ScheduleHint.OnTour)
            {
                Log.Debug(TimeInfo.Now, $"{GetCitizenDesc(0, ref citizen)} quits a tour (see next line for citizen ID)");
                schedule.Schedule(ResidentState.Unknown, default);
                return true;
            }

            return RescheduleVisit(ref schedule, ref citizen, CitizenProxy.GetVisitBuilding(ref citizen));
        }

        private bool IsBuildingNoiseRestricted(ushort targetBuilding, ushort currentBuilding)
        {
            if (BuildingMgr.GetBuildingSubService(targetBuilding) != ItemClass.SubService.CommercialLeisure)
            {
                return false;
            }

            float currentHour = TimeInfo.CurrentHour;
            if (currentHour >= Config.GoToSleepUpHour || currentHour <= Config.WakeupHour)
            {
                return BuildingMgr.IsBuildingNoiseRestricted(targetBuilding);
            }

            float travelTime = travelBehavior.GetEstimatedTravelTime(currentBuilding, targetBuilding);
            if (travelTime == 0)
            {
                return false;
            }

            float arriveHour = (float)TimeInfo.Now.AddHours(travelTime).TimeOfDay.TotalHours;
            if (arriveHour >= Config.GoToSleepUpHour || arriveHour <= Config.WakeupHour)
            {
                return BuildingMgr.IsBuildingNoiseRestricted(targetBuilding);
            }

            return false;
        }

        private bool RescheduleVisit(ref CitizenSchedule schedule, ref TCitizen citizen, ushort currentBuilding)
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
                Log.Debug(TimeInfo.Now, $"{GetCitizenDesc(0, ref citizen)} quits a visit because of bad weather (see next line for citizen ID)");
                schedule.Schedule(ResidentState.AtHome, default);
                return true;
            }

            if (IsBuildingNoiseRestricted(currentBuilding, currentBuilding))
            {
                Log.Debug(TimeInfo.Now, $"{GetCitizenDesc(0, ref citizen)} quits a visit because of NIMBY policy (see next line for citizen ID)");
                schedule.Schedule(ResidentState.Unknown, default);
                return true;
            }

            Citizen.AgeGroup age = CitizenProxy.GetAge(ref citizen);
            uint stayChance = schedule.CurrentState == ResidentState.Shopping
                ? spareTimeBehavior.GetShoppingChance(age)
                : spareTimeBehavior.GetRelaxingChance(age, schedule.WorkShift);

            if (!Random.ShouldOccur(stayChance))
            {
                Log.Debug(TimeInfo.Now, $"{GetCitizenDesc(0, ref citizen)} quits a visit because of time (see next line for citizen ID)");
                schedule.Schedule(ResidentState.AtHome, default);
                return true;
            }

            return false;
        }
    }
}
