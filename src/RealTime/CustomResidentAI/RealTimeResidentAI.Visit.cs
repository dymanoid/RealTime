// <copyright file="RealTimeResidentAI.Visit.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.CustomResidentAI
{
    using RealTime.Tools;

    internal sealed partial class RealTimeResidentAI<TAI, TCitizen>
    {
        private void ProcessCitizenVisit(TAI instance, CitizenState citizenState, uint citizenId, ref TCitizen citizen)
        {
            if (citizenProxy.GetVisitBuilding(ref citizen) == 0)
            {
                Log.Debug($"WARNING: {GetCitizenDesc(citizenId, ref citizen)} is in corrupt state: visiting with no visit building. Teleporting home.");
                citizenProxy.SetLocation(ref citizen, Citizen.Location.Home);
                return;
            }

            switch (citizenState)
            {
                case CitizenState.AtLunch:
                    CitizenReturnsFromLunch(instance, citizenId, ref citizen);

                    return;

                case CitizenState.AtLeisureArea:
                case CitizenState.Visiting:
                    if (!CitizenGoesWorking(instance, citizenId, ref citizen))
                    {
                        CitizenReturnsHomeFromVisit(instance, citizenId, ref citizen);
                    }

                    return;

                case CitizenState.Shopping:
                    if (CitizenGoesWorking(instance, citizenId, ref citizen))
                    {
                        return;
                    }

                    if (IsChance(ReturnFromShoppingChance) || IsWorkDayMorning())
                    {
                        Log.Debug(timeInfo.Now, $"{GetCitizenDesc(citizenId, ref citizen)} returning from shopping back home");
                        ReturnFromVisit(instance, citizenId, ref citizen, citizenProxy.GetHomeBuilding(ref citizen));
                        return;
                    }

                    break;

                default:
                    return;
            }

            if ((citizenProxy.GetFlags(ref citizen) & Citizen.Flags.NeedGoods) != 0)
            {
                buildingManager.ModifyMaterialBuffer(citizenProxy.GetVisitBuilding(ref citizen), TransferManager.TransferReason.Shopping, -ShoppingGoodsAmount);
                citizenProxy.RemoveFlags(ref citizen, Citizen.Flags.NeedGoods);
            }
        }

        private bool CitizenReturnsFromLunch(TAI instance, uint citizenId, ref TCitizen citizen)
        {
            if (IsLunchHour)
            {
                return false;
            }

            ushort workBuilding = citizenProxy.GetWorkBuilding(ref citizen);
            if (workBuilding != 0)
            {
                Log.Debug(timeInfo.Now, $"{GetCitizenDesc(citizenId, ref citizen)} returning from lunch to {workBuilding}");
                ReturnFromVisit(instance, citizenId, ref citizen, workBuilding);
            }
            else
            {
                Log.Debug($"WARNING: {GetCitizenDesc(citizenId, ref citizen)} is at lunch but no work building. Teleporting home.");
                citizenProxy.SetLocation(ref citizen, Citizen.Location.Home);
            }

            return true;
        }

        private bool CitizenReturnsHomeFromVisit(TAI instance, uint citizenId, ref TCitizen citizen)
        {
            ushort homeBuilding = citizenProxy.GetHomeBuilding(ref citizen);
            if (homeBuilding == 0 || citizenProxy.GetVehicle(ref citizen) != 0)
            {
                return false;
            }

            ushort visitBuilding = citizenProxy.GetVisitBuilding(ref citizen);
            if (buildingManager.GetBuildingService(visitBuilding) == ItemClass.Service.Disaster)
            {
                if ((buildingManager.GetBuildingFlags(visitBuilding) & Building.Flags.Downgrading) != 0)
                {
                    Log.Debug(timeInfo.Now, $"{GetCitizenDesc(citizenId, ref citizen)} returning from evacuation place back home");
                    ReturnFromVisit(instance, citizenId, ref citizen, homeBuilding);
                    return true;
                }

                return false;
            }

            ushort eventId = buildingManager.GetEvent(visitBuilding);
            if (eventId != 0)
            {
                if ((eventManager.GetEventFlags(eventId) & (EventData.Flags.Preparing | EventData.Flags.Active | EventData.Flags.Ready)) == 0)
                {
                    Log.Debug(timeInfo.Now, $"{GetCitizenDesc(citizenId, ref citizen)} returning from an event back home");
                    ReturnFromVisit(instance, citizenId, ref citizen, homeBuilding);
                }

                return true;
            }

            if (IsChance(ReturnFromVisitChance))
            {
                Log.Debug(timeInfo.Now, $"{GetCitizenDesc(citizenId, ref citizen)} returning from visit back home");
                ReturnFromVisit(instance, citizenId, ref citizen, homeBuilding);
                return true;
            }

            return false;
        }

        private void ReturnFromVisit(TAI instance, uint citizenId, ref TCitizen citizen, ushort targetBuilding)
        {
            if (targetBuilding != 0 && citizenProxy.GetVehicle(ref citizen) == 0)
            {
                citizenProxy.RemoveFlags(ref citizen, Citizen.Flags.Evacuating);
                residentAI.StartMoving(instance, citizenId, ref citizen, citizenProxy.GetVisitBuilding(ref citizen), targetBuilding);
                citizenProxy.SetVisitPlace(ref citizen, citizenId, 0);
            }
        }

        private bool CitizenGoesShopping(TAI instance, uint citizenId, ref TCitizen citizen)
        {
            if ((citizenProxy.GetFlags(ref citizen) & Citizen.Flags.NeedGoods) == 0)
            {
                return false;
            }

            if (timeInfo.IsNightTime)
            {
                if (IsChance(GetGoOutChance(citizenProxy.GetAge(ref citizen), false)))
                {
                    ushort localVisitPlace = MoveToCommercialBuilding(instance, citizenId, ref citizen, LocalSearchDistance);
                    Log.Debug(timeInfo.Now, $"{GetCitizenDesc(citizenId, ref citizen)} wanna go shopping at night, trying local shop '{localVisitPlace}'");
                    return localVisitPlace > 0;
                }

                return false;
            }

            if (IsChance(GoShoppingChance))
            {
                bool localOnly = citizenProxy.GetWorkBuilding(ref citizen) != 0 && IsWorkDayMorning();
                ushort localVisitPlace = 0;

                if (IsChance(config.LocalBuildingSearchQuota))
                {
                    localVisitPlace = MoveToCommercialBuilding(instance, citizenId, ref citizen, LocalSearchDistance);
                    Log.Debug(timeInfo.Now, $"{GetCitizenDesc(citizenId, ref citizen)} wanna go shopping, tries a local shop '{localVisitPlace}'");
                }

                if (localVisitPlace == 0)
                {
                    if (localOnly)
                    {
                        Log.Debug(timeInfo.Now, $"{GetCitizenDesc(citizenId, ref citizen)} wanna go shopping, but didn't found a local shop");
                        return false;
                    }

                    Log.Debug(timeInfo.Now, $"{GetCitizenDesc(citizenId, ref citizen)} wanna go shopping, heading to a random shop");
                    residentAI.FindVisitPlace(instance, citizenId, citizenProxy.GetHomeBuilding(ref citizen), residentAI.GetShoppingReason(instance));
                }

                return true;
            }

            return false;
        }

        private bool CitizenGoesRelaxing(TAI instance, uint citizenId, ref TCitizen citizen)
        {
            // TODO: add events here
            if (!ShouldFindEntertainment(citizenProxy.GetAge(ref citizen)))
            {
                return false;
            }

            ushort buildingId = citizenProxy.GetCurrentBuilding(ref citizen);
            if (buildingId == 0)
            {
                return false;
            }

            if (timeInfo.IsNightTime)
            {
                ushort leisure = MoveToLeisure(instance, citizenId, ref citizen, buildingId);
                Log.Debug(timeInfo.Now, $"{GetCitizenDesc(citizenId, ref citizen)} wanna relax at night, trying leisure area '{leisure}'");
            }
            else
            {
                Log.Debug(timeInfo.Now, $"{GetCitizenDesc(citizenId, ref citizen)} wanna relax, heading to an entertainment place");
                residentAI.FindVisitPlace(instance, citizenId, buildingId, residentAI.GetEntertainmentReason(instance));
            }

            return true;
        }

        private ushort MoveToCommercialBuilding(TAI instance, uint citizenId, ref TCitizen citizen, float distance)
        {
            ushort buildingId = citizenProxy.GetCurrentBuilding(ref citizen);
            if (buildingId == 0)
            {
                return 0;
            }

            ushort foundBuilding = buildingManager.FindActiveBuilding(buildingId, distance, ItemClass.Service.Commercial);

            if (foundBuilding != 0)
            {
                residentAI.StartMoving(instance, citizenId, ref citizen, buildingId, foundBuilding);
                citizenProxy.SetVisitPlace(ref citizen, citizenId, foundBuilding);
                citizenProxy.SetVisitBuilding(ref citizen, foundBuilding);
            }

            return foundBuilding;
        }

        private ushort MoveToLeisure(TAI instance, uint citizenId, ref TCitizen citizen, ushort buildingId)
        {
            ushort leisureBuilding = buildingManager.FindActiveBuilding(
                buildingId,
                FullSearchDistance,
                ItemClass.Service.Commercial,
                ItemClass.SubService.CommercialLeisure);

            if (leisureBuilding != 0)
            {
                residentAI.StartMoving(instance, citizenId, ref citizen, buildingId, leisureBuilding);
                citizenProxy.SetVisitPlace(ref citizen, citizenId, leisureBuilding);
                citizenProxy.SetVisitBuilding(ref citizen, leisureBuilding);
            }

            return leisureBuilding;
        }
    }
}
