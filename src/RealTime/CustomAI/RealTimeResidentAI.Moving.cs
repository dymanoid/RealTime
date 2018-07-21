// <copyright file="RealTimeResidentAI.Moving.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.CustomAI
{
    using RealTime.Tools;
    using static Constants;

    internal sealed partial class RealTimeResidentAI<TAI, TCitizen>
    {
        private void ProcessCitizenMoving(TAI instance, uint citizenId, ref TCitizen citizen)
        {
            ushort instanceId = CitizenProxy.GetInstance(ref citizen);
            ushort vehicleId = CitizenProxy.GetVehicle(ref citizen);

            if (vehicleId == 0 && instanceId == 0)
            {
                if (CitizenProxy.GetVisitBuilding(ref citizen) != 0)
                {
                    CitizenProxy.SetVisitPlace(ref citizen, citizenId, 0);
                }

                if (CitizenProxy.HasFlags(ref citizen, Citizen.Flags.MovingIn))
                {
                    CitizenMgr.ReleaseCitizen(citizenId);
                    residentSchedules[citizenId] = default;
                }
                else
                {
                    CitizenProxy.SetLocation(ref citizen, Citizen.Location.Home);
                    CitizenProxy.SetArrested(ref citizen, false);
                }

                return;
            }

            if (vehicleId == 0 && CitizenMgr.IsAreaEvacuating(instanceId) && !CitizenProxy.HasFlags(ref citizen, Citizen.Flags.Evacuating))
            {
                Log.Debug(TimeInfo.Now, $"{GetCitizenDesc(citizenId, ref citizen)} was on the way, but the area evacuates. Finding an evacuation place.");
                TransferMgr.AddOutgoingOfferFromCurrentPosition(citizenId, residentAI.GetEvacuationReason(instance, 0));
                return;
            }

            ushort targetBuilding = CitizenMgr.GetTargetBuilding(instanceId);
            if (targetBuilding == CitizenProxy.GetWorkBuilding(ref citizen))
            {
                return;
            }

            ItemClass.Service targetService = BuildingMgr.GetBuildingService(targetBuilding);
            if (targetService == ItemClass.Service.Beautification && IsBadWeather())
            {
                Log.Debug(TimeInfo.Now, $"{GetCitizenDesc(citizenId, ref citizen)} cancels the trip to a park due to bad weather");
                ushort home = CitizenProxy.GetHomeBuilding(ref citizen);
                if (home != 0)
                {
                    residentAI.StartMoving(instance, citizenId, ref citizen, 0, home);
                }
            }
        }

        private ushort MoveToCommercialBuilding(TAI instance, uint citizenId, ref TCitizen citizen, float distance)
        {
            ushort buildingId = CitizenProxy.GetCurrentBuilding(ref citizen);
            if (buildingId == 0)
            {
                return 0;
            }

            ushort foundBuilding = BuildingMgr.FindActiveBuilding(buildingId, distance, ItemClass.Service.Commercial);
            if (IsBuildingNoiseRestricted(foundBuilding))
            {
                Log.Debug($"Citizen {citizenId} won't go to the commercial building {foundBuilding}, it has a NIMBY policy");
                return 0;
            }

            if (StartMovingToVisitBuilding(instance, citizenId, ref citizen, foundBuilding))
            {
                ushort homeBuilding = CitizenProxy.GetHomeBuilding(ref citizen);
                uint homeUnit = BuildingMgr.GetCitizenUnit(homeBuilding);
                uint citizenUnit = CitizenProxy.GetContainingUnit(ref citizen, citizenId, homeUnit, CitizenUnit.Flags.Home);
                if (citizenUnit != 0)
                {
                    CitizenMgr.ModifyUnitGoods(citizenUnit, ShoppingGoodsAmount);
                }
            }

            return foundBuilding;
        }

        private ushort MoveToLeisureBuilding(TAI instance, uint citizenId, ref TCitizen citizen, ushort buildingId)
        {
            ushort leisureBuilding = BuildingMgr.FindActiveBuilding(
                buildingId,
                LeisureSearchDistance,
                ItemClass.Service.Commercial,
                ItemClass.SubService.CommercialLeisure);

            if (IsBuildingNoiseRestricted(leisureBuilding))
            {
                Log.Debug($"Citizen {citizenId} won't go to the leisure building {leisureBuilding}, it has a NIMBY policy");
                return 0;
            }

            StartMovingToVisitBuilding(instance, citizenId, ref citizen, leisureBuilding);
            return leisureBuilding;
        }

        private bool StartMovingToVisitBuilding(TAI instance, uint citizenId, ref TCitizen citizen, ushort visitBuilding)
        {
            if (visitBuilding == 0)
            {
                return false;
            }

            ushort currentBuilding = CitizenProxy.GetCurrentBuilding(ref citizen);

            if (currentBuilding == visitBuilding)
            {
                CitizenProxy.SetVisitPlace(ref citizen, citizenId, visitBuilding);
                CitizenProxy.SetVisitBuilding(ref citizen, visitBuilding);
                CitizenProxy.SetLocation(ref citizen, Citizen.Location.Visit);
                return true;
            }
            else if (residentAI.StartMoving(instance, citizenId, ref citizen, currentBuilding, visitBuilding))
            {
                CitizenProxy.SetVisitPlace(ref citizen, citizenId, visitBuilding);
                CitizenProxy.SetVisitBuilding(ref citizen, visitBuilding);
                return true;
            }

            return false;
        }
    }
}
