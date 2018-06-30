// <copyright file="RealTimeResidentAI.Visit.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.CustomAI
{
    using RealTime.Events;
    using RealTime.Tools;
    using static Constants;

    internal sealed partial class RealTimeResidentAI<TAI, TCitizen>
    {
        private void ProcessCitizenVisit(TAI instance, ResidentState citizenState, uint citizenId, ref TCitizen citizen, bool isVirtual)
        {
            ushort currentBuilding = CitizenProxy.GetVisitBuilding(ref citizen);
            if (currentBuilding == 0)
            {
                Log.Debug($"WARNING: {GetCitizenDesc(citizenId, ref citizen, isVirtual)} is in corrupt state: visiting with no visit building. Teleporting home.");
                CitizenProxy.SetLocation(ref citizen, Citizen.Location.Home);
                return;
            }

            switch (citizenState)
            {
                case ResidentState.AtLunch:
                    CitizenReturnsFromLunch(instance, citizenId, ref citizen, isVirtual);

                    return;

                case ResidentState.AtLeisureArea:
                    if (CitizenProxy.HasFlags(ref citizen, Citizen.Flags.NeedGoods)
                        && BuildingMgr.GetBuildingSubService(currentBuilding) == ItemClass.SubService.CommercialLeisure)
                    {
                        // No Citizen.Flags.NeedGoods flag reset here, because we only bought 'beer' or 'champagne' in a leisure building.
                        BuildingMgr.ModifyMaterialBuffer(CitizenProxy.GetVisitBuilding(ref citizen), TransferManager.TransferReason.Shopping, -ShoppingGoodsAmount);
                    }

                    goto case ResidentState.Visiting;

                case ResidentState.Visiting:
                    if (!CitizenGoesWorking(instance, citizenId, ref citizen, isVirtual))
                    {
                        CitizenReturnsHomeFromVisit(instance, citizenId, ref citizen, isVirtual);
                    }

                    return;

                case ResidentState.Shopping:
                    if (CitizenProxy.HasFlags(ref citizen, Citizen.Flags.NeedGoods))
                    {
                        BuildingMgr.ModifyMaterialBuffer(CitizenProxy.GetVisitBuilding(ref citizen), TransferManager.TransferReason.Shopping, -ShoppingGoodsAmount);
                        CitizenProxy.RemoveFlags(ref citizen, Citizen.Flags.NeedGoods);
                    }

                    if (CitizenGoesWorking(instance, citizenId, ref citizen, isVirtual)
                        || CitizenGoesToEvent(instance, citizenId, ref citizen, isVirtual))
                    {
                        return;
                    }

                    if (IsChance(ReturnFromShoppingChance) || IsWorkDayMorning(CitizenProxy.GetAge(ref citizen)))
                    {
                        Log.Debug(TimeInfo.Now, $"{GetCitizenDesc(citizenId, ref citizen, isVirtual)} returning from shopping at {currentBuilding} back home");
                        ReturnFromVisit(instance, citizenId, ref citizen, CitizenProxy.GetHomeBuilding(ref citizen), Citizen.Location.Home, isVirtual);
                    }

                    return;
            }
        }

        private void ProcessCitizenOnTour(TAI instance, uint citizenId, ref TCitizen citizen)
        {
            if (!CitizenMgr.InstanceHasFlags(CitizenProxy.GetInstance(ref citizen), CitizenInstance.Flags.TargetIsNode))
            {
                return;
            }

            ushort homeBuilding = CitizenProxy.GetHomeBuilding(ref citizen);
            if (homeBuilding != 0)
            {
                Log.Debug(TimeInfo.Now, $"{GetCitizenDesc(citizenId, ref citizen, false)} exits a guided tour and moves back home.");
                residentAI.StartMoving(instance, citizenId, ref citizen, 0, homeBuilding);
            }
        }

        private bool CitizenReturnsFromShelter(TAI instance, uint citizenId, ref TCitizen citizen, bool isVirtual)
        {
            ushort visitBuilding = CitizenProxy.GetVisitBuilding(ref citizen);
            if (BuildingMgr.GetBuildingService(visitBuilding) != ItemClass.Service.Disaster)
            {
                return true;
            }

            if (!BuildingMgr.BuildingHasFlags(visitBuilding, Building.Flags.Downgrading))
            {
                return false;
            }

            ushort homeBuilding = CitizenProxy.GetHomeBuilding(ref citizen);
            if (homeBuilding == 0)
            {
                Log.Debug($"WARNING: {GetCitizenDesc(citizenId, ref citizen, isVirtual)} was in a shelter but seems to be homeless. Releasing the citizen.");
                CitizenMgr.ReleaseCitizen(citizenId);
                return true;
            }

            Log.Debug(TimeInfo.Now, $"{GetCitizenDesc(citizenId, ref citizen, isVirtual)} returning from evacuation place {visitBuilding} back home");
            ReturnFromVisit(instance, citizenId, ref citizen, homeBuilding, Citizen.Location.Home, isVirtual);
            return true;
        }

        private bool CitizenReturnsHomeFromVisit(TAI instance, uint citizenId, ref TCitizen citizen, bool isVirtual)
        {
            ushort homeBuilding = CitizenProxy.GetHomeBuilding(ref citizen);
            if (homeBuilding == 0 || CitizenProxy.GetVehicle(ref citizen) != 0)
            {
                return false;
            }

            ushort visitBuilding = CitizenProxy.GetVisitBuilding(ref citizen);
            switch (EventMgr.GetEventState(visitBuilding, TimeInfo.Now.AddHours(MaxHoursOnTheWay)))
            {
                case CityEventState.Upcoming:
                case CityEventState.Ongoing:
                    return false;

                case CityEventState.Finished:
                    Log.Debug(TimeInfo.Now, $"{GetCitizenDesc(citizenId, ref citizen, isVirtual)} returning from an event at {visitBuilding} back home to {homeBuilding}");
                    ReturnFromVisit(instance, citizenId, ref citizen, homeBuilding, Citizen.Location.Home, isVirtual);
                    return true;
            }

            if (IsChance(ReturnFromVisitChance))
            {
                Log.Debug(TimeInfo.Now, $"{GetCitizenDesc(citizenId, ref citizen, isVirtual)} returning from visit back home");
                ReturnFromVisit(instance, citizenId, ref citizen, homeBuilding, Citizen.Location.Home, isVirtual);
                return true;
            }

            return false;
        }

        private void ReturnFromVisit(
            TAI instance,
            uint citizenId,
            ref TCitizen citizen,
            ushort targetBuilding,
            Citizen.Location targetLocation,
            bool isVirtual)
        {
            if (targetBuilding != 0 && CitizenProxy.GetVehicle(ref citizen) == 0)
            {
                CitizenProxy.RemoveFlags(ref citizen, Citizen.Flags.Evacuating);
                CitizenProxy.SetVisitPlace(ref citizen, citizenId, 0);

                if (isVirtual)
                {
                    CitizenProxy.SetLocation(ref citizen, targetLocation);
                }
                else
                {
                    residentAI.StartMoving(instance, citizenId, ref citizen, CitizenProxy.GetVisitBuilding(ref citizen), targetBuilding);
                }
            }
        }

        private bool CitizenGoesShopping(TAI instance, uint citizenId, ref TCitizen citizen, bool isVirtual)
        {
            if (!CitizenProxy.HasFlags(ref citizen, Citizen.Flags.NeedGoods))
            {
                return false;
            }

            if (TimeInfo.IsNightTime)
            {
                if (IsChance(GetGoOutChance(CitizenProxy.GetAge(ref citizen))))
                {
                    ushort localVisitPlace = MoveToCommercialBuilding(instance, citizenId, ref citizen, LocalSearchDistance, isVirtual);
                    Log.Debug(TimeInfo.Now, $"{GetCitizenDesc(citizenId, ref citizen, isVirtual)} wanna go shopping at night, trying local shop '{localVisitPlace}'");
                    return localVisitPlace > 0;
                }

                return false;
            }

            if (IsChance(GoShoppingChance))
            {
                bool localOnly = CitizenProxy.GetWorkBuilding(ref citizen) != 0 && IsWorkDayMorning(CitizenProxy.GetAge(ref citizen));
                ushort localVisitPlace = 0;

                if (IsChance(Config.LocalBuildingSearchQuota))
                {
                    localVisitPlace = MoveToCommercialBuilding(instance, citizenId, ref citizen, LocalSearchDistance, isVirtual);
                    Log.Debug(TimeInfo.Now, $"{GetCitizenDesc(citizenId, ref citizen, isVirtual)} wanna go shopping, tries a local shop '{localVisitPlace}'");
                }

                if (localVisitPlace == 0)
                {
                    if (localOnly)
                    {
                        Log.Debug(TimeInfo.Now, $"{GetCitizenDesc(citizenId, ref citizen, isVirtual)} wanna go shopping, but didn't find a local shop");
                        return false;
                    }

                    Log.Debug(TimeInfo.Now, $"{GetCitizenDesc(citizenId, ref citizen, isVirtual)} wanna go shopping, heading to a random shop");
                    residentAI.FindVisitPlace(instance, citizenId, CitizenProxy.GetHomeBuilding(ref citizen), residentAI.GetShoppingReason(instance));
                }

                return true;
            }

            return false;
        }

        private bool CitizenGoesToEvent(TAI instance, uint citizenId, ref TCitizen citizen, bool isVirtual)
        {
            if (!IsChance(GetGoOutChance(CitizenProxy.GetAge(ref citizen))))
            {
                return false;
            }

            if (!AttendUpcomingEvent(citizenId, ref citizen, out ushort buildingId))
            {
                return false;
            }

            Log.Debug(TimeInfo.Now, $"{GetCitizenDesc(citizenId, ref citizen, isVirtual)} wanna attend an event at '{buildingId}', on the way now.");
            return StartMovingToVisitBuilding(instance, citizenId, ref citizen, buildingId, isVirtual);
        }

        private bool CitizenGoesRelaxing(TAI instance, uint citizenId, ref TCitizen citizen, bool isVirtual)
        {
            Citizen.AgeGroup citizenAge = CitizenProxy.GetAge(ref citizen);
            if (!IsChance(GetGoOutChance(citizenAge)))
            {
                return false;
            }

            ushort buildingId = CitizenProxy.GetCurrentBuilding(ref citizen);
            if (buildingId == 0)
            {
                return false;
            }

            if (TimeInfo.IsNightTime)
            {
                ushort leisure = MoveToLeisure(instance, citizenId, ref citizen, buildingId, isVirtual);
                Log.Debug(TimeInfo.Now, $"{GetCitizenDesc(citizenId, ref citizen, isVirtual)} wanna relax at night, trying leisure area '{leisure}'");
                return leisure != 0;
            }

            if (CitizenProxy.GetWorkBuilding(ref citizen) != 0 && IsWorkDayMorning(citizenAge))
            {
                return false;
            }

            if (!isVirtual)
            {
                Log.Debug(TimeInfo.Now, $"{GetCitizenDesc(citizenId, ref citizen, isVirtual)} wanna relax, heading to an entertainment place");
                residentAI.FindVisitPlace(instance, citizenId, buildingId, residentAI.GetEntertainmentReason(instance));
            }

            return true;
        }

        private ushort MoveToCommercialBuilding(TAI instance, uint citizenId, ref TCitizen citizen, float distance, bool isVirtual)
        {
            ushort buildingId = CitizenProxy.GetCurrentBuilding(ref citizen);
            if (buildingId == 0)
            {
                return 0;
            }

            ushort foundBuilding = BuildingMgr.FindActiveBuilding(buildingId, distance, ItemClass.Service.Commercial);
            StartMovingToVisitBuilding(instance, citizenId, ref citizen, foundBuilding, isVirtual);
            return foundBuilding;
        }

        private ushort MoveToLeisure(TAI instance, uint citizenId, ref TCitizen citizen, ushort buildingId, bool isVirtual)
        {
            ushort leisureBuilding = BuildingMgr.FindActiveBuilding(
                buildingId,
                LeisureSearchDistance,
                ItemClass.Service.Commercial,
                ItemClass.SubService.CommercialLeisure);

            StartMovingToVisitBuilding(instance, citizenId, ref citizen, leisureBuilding, isVirtual);
            return leisureBuilding;
        }
    }
}
