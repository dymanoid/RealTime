// <copyright file="RealTimeResidentAI.Visit.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.CustomAI
{
    using RealTime.Tools;
    using static Constants;

    internal sealed partial class RealTimeResidentAI<TAI, TCitizen>
    {
        private void ProcessCitizenVisit(TAI instance, ResidentState citizenState, uint citizenId, ref TCitizen citizen)
        {
            if (CitizenProxy.GetVisitBuilding(ref citizen) == 0)
            {
                Log.Debug($"WARNING: {GetCitizenDesc(citizenId, ref citizen)} is in corrupt state: visiting with no visit building. Teleporting home.");
                CitizenProxy.SetLocation(ref citizen, Citizen.Location.Home);
                return;
            }

            switch (citizenState)
            {
                case ResidentState.AtLunch:
                    CitizenReturnsFromLunch(instance, citizenId, ref citizen);

                    return;

                case ResidentState.AtLeisureArea:
                case ResidentState.Visiting:
                    if (!CitizenGoesWorking(instance, citizenId, ref citizen))
                    {
                        CitizenReturnsHomeFromVisit(instance, citizenId, ref citizen);
                    }

                    return;

                case ResidentState.Shopping:
                    if (CitizenGoesWorking(instance, citizenId, ref citizen))
                    {
                        return;
                    }

                    if (IsChance(ReturnFromShoppingChance) || IsWorkDayMorning(CitizenProxy.GetAge(ref citizen)))
                    {
                        Log.Debug(TimeInfo.Now, $"{GetCitizenDesc(citizenId, ref citizen)} returning from shopping back home");
                        ReturnFromVisit(instance, citizenId, ref citizen, CitizenProxy.GetHomeBuilding(ref citizen));
                        return;
                    }

                    break;

                default:
                    return;
            }

            if ((CitizenProxy.GetFlags(ref citizen) & Citizen.Flags.NeedGoods) != 0)
            {
                BuildingManager.ModifyMaterialBuffer(CitizenProxy.GetVisitBuilding(ref citizen), TransferManager.TransferReason.Shopping, -ShoppingGoodsAmount);
                CitizenProxy.RemoveFlags(ref citizen, Citizen.Flags.NeedGoods);
            }
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
                ReturnFromVisit(instance, citizenId, ref citizen, workBuilding);
            }
            else
            {
                Log.Debug($"WARNING: {GetCitizenDesc(citizenId, ref citizen)} is at lunch but no work building. Teleporting home.");
                CitizenProxy.SetLocation(ref citizen, Citizen.Location.Home);
            }

            return true;
        }

        private bool CitizenReturnsHomeFromVisit(TAI instance, uint citizenId, ref TCitizen citizen)
        {
            ushort homeBuilding = CitizenProxy.GetHomeBuilding(ref citizen);
            if (homeBuilding == 0 || CitizenProxy.GetVehicle(ref citizen) != 0)
            {
                return false;
            }

            ushort visitBuilding = CitizenProxy.GetVisitBuilding(ref citizen);
            if (BuildingManager.GetBuildingService(visitBuilding) == ItemClass.Service.Disaster)
            {
                if ((BuildingManager.GetBuildingFlags(visitBuilding) & Building.Flags.Downgrading) != 0)
                {
                    Log.Debug(TimeInfo.Now, $"{GetCitizenDesc(citizenId, ref citizen)} returning from evacuation place back home");
                    ReturnFromVisit(instance, citizenId, ref citizen, homeBuilding);
                    return true;
                }

                return false;
            }

            ushort eventId = BuildingManager.GetEvent(visitBuilding);
            if (eventId != 0)
            {
                if ((EventManager.GetEventFlags(eventId) & (EventData.Flags.Preparing | EventData.Flags.Active | EventData.Flags.Ready)) == 0)
                {
                    Log.Debug(TimeInfo.Now, $"{GetCitizenDesc(citizenId, ref citizen)} returning from an event {eventId} at {visitBuilding} back home to {homeBuilding}");
                    ReturnFromVisit(instance, citizenId, ref citizen, homeBuilding);
                }

                return true;
            }

            if (IsChance(ReturnFromVisitChance))
            {
                Log.Debug(TimeInfo.Now, $"{GetCitizenDesc(citizenId, ref citizen)} returning from visit back home");
                ReturnFromVisit(instance, citizenId, ref citizen, homeBuilding);
                return true;
            }

            return false;
        }

        private void ReturnFromVisit(TAI instance, uint citizenId, ref TCitizen citizen, ushort targetBuilding)
        {
            if (targetBuilding != 0 && CitizenProxy.GetVehicle(ref citizen) == 0)
            {
                CitizenProxy.RemoveFlags(ref citizen, Citizen.Flags.Evacuating);
                residentAI.StartMoving(instance, citizenId, ref citizen, CitizenProxy.GetVisitBuilding(ref citizen), targetBuilding);
                CitizenProxy.SetVisitPlace(ref citizen, citizenId, 0);
            }
        }

        private bool CitizenGoesShopping(TAI instance, uint citizenId, ref TCitizen citizen)
        {
            if ((CitizenProxy.GetFlags(ref citizen) & Citizen.Flags.NeedGoods) == 0)
            {
                return false;
            }

            if (TimeInfo.IsNightTime)
            {
                if (IsChance(GetGoOutChance(CitizenProxy.GetAge(ref citizen))))
                {
                    ushort localVisitPlace = MoveToCommercialBuilding(instance, citizenId, ref citizen, LocalSearchDistance);
                    Log.Debug(TimeInfo.Now, $"{GetCitizenDesc(citizenId, ref citizen)} wanna go shopping at night, trying local shop '{localVisitPlace}'");
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
                    localVisitPlace = MoveToCommercialBuilding(instance, citizenId, ref citizen, LocalSearchDistance);
                    Log.Debug(TimeInfo.Now, $"{GetCitizenDesc(citizenId, ref citizen)} wanna go shopping, tries a local shop '{localVisitPlace}'");
                }

                if (localVisitPlace == 0)
                {
                    if (localOnly)
                    {
                        Log.Debug(TimeInfo.Now, $"{GetCitizenDesc(citizenId, ref citizen)} wanna go shopping, but didn't find a local shop");
                        return false;
                    }

                    Log.Debug(TimeInfo.Now, $"{GetCitizenDesc(citizenId, ref citizen)} wanna go shopping, heading to a random shop");
                    residentAI.FindVisitPlace(instance, citizenId, CitizenProxy.GetHomeBuilding(ref citizen), residentAI.GetShoppingReason(instance));
                }

                return true;
            }

            return false;
        }

        private bool CitizenGoesRelaxing(TAI instance, uint citizenId, ref TCitizen citizen)
        {
            // TODO: add events here
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
                ushort leisure = MoveToLeisure(instance, citizenId, ref citizen, buildingId);
                Log.Debug(TimeInfo.Now, $"{GetCitizenDesc(citizenId, ref citizen)} wanna relax at night, trying leisure area '{leisure}'");
                return leisure != 0;
            }

            if (CitizenProxy.GetWorkBuilding(ref citizen) != 0 && IsWorkDayMorning(citizenAge))
            {
                return false;
            }

            Log.Debug(TimeInfo.Now, $"{GetCitizenDesc(citizenId, ref citizen)} wanna relax, heading to an entertainment place");
            residentAI.FindVisitPlace(instance, citizenId, buildingId, residentAI.GetEntertainmentReason(instance));
            return true;
        }

        private ushort MoveToCommercialBuilding(TAI instance, uint citizenId, ref TCitizen citizen, float distance)
        {
            ushort buildingId = CitizenProxy.GetCurrentBuilding(ref citizen);
            if (buildingId == 0)
            {
                return 0;
            }

            ushort foundBuilding = BuildingManager.FindActiveBuilding(buildingId, distance, ItemClass.Service.Commercial);

            if (foundBuilding == CitizenProxy.GetWorkBuilding(ref citizen))
            {
                return 0;
            }

            if (foundBuilding != 0)
            {
                residentAI.StartMoving(instance, citizenId, ref citizen, buildingId, foundBuilding);
                CitizenProxy.SetVisitPlace(ref citizen, citizenId, foundBuilding);
                CitizenProxy.SetVisitBuilding(ref citizen, foundBuilding);
            }

            return foundBuilding;
        }

        private ushort MoveToLeisure(TAI instance, uint citizenId, ref TCitizen citizen, ushort buildingId)
        {
            ushort leisureBuilding = BuildingManager.FindActiveBuilding(
                buildingId,
                FullSearchDistance,
                ItemClass.Service.Commercial,
                ItemClass.SubService.CommercialLeisure);

            if (leisureBuilding == CitizenProxy.GetWorkBuilding(ref citizen))
            {
                return 0;
            }

            if (leisureBuilding != 0)
            {
                residentAI.StartMoving(instance, citizenId, ref citizen, buildingId, leisureBuilding);
                CitizenProxy.SetVisitPlace(ref citizen, citizenId, leisureBuilding);
                CitizenProxy.SetVisitBuilding(ref citizen, leisureBuilding);
            }

            return leisureBuilding;
        }
    }
}
