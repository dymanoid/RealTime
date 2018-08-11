// <copyright file="RealTimeResidentAI.Moving.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.CustomAI
{
    using SkyTools.Tools;
    using static Constants;

    internal sealed partial class RealTimeResidentAI<TAI, TCitizen>
    {
        private bool ProcessCitizenMoving(ref CitizenSchedule schedule, TAI instance, uint citizenId, ref TCitizen citizen)
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
                    schedule = default;
                }
                else
                {
                    CitizenProxy.SetLocation(ref citizen, Citizen.Location.Home);
                    CitizenProxy.SetArrested(ref citizen, false);
                    schedule.Schedule(ResidentState.Unknown);
                }

                return true;
            }

            if (vehicleId == 0 && CitizenMgr.IsAreaEvacuating(instanceId) && !CitizenProxy.HasFlags(ref citizen, Citizen.Flags.Evacuating))
            {
                Log.Debug(LogCategory.Movement, TimeInfo.Now, $"{GetCitizenDesc(citizenId, ref citizen)} was on the way, but the area evacuates. Finding an evacuation place.");
                schedule.Schedule(ResidentState.Unknown);
                TransferMgr.AddOutgoingOfferFromCurrentPosition(citizenId, residentAI.GetEvacuationReason(instance, 0));
                return true;
            }

            ushort targetBuilding = CitizenMgr.GetTargetBuilding(instanceId);
            if (targetBuilding == CitizenProxy.GetWorkBuilding(ref citizen))
            {
                return true;
            }

            ItemClass.Service targetService = BuildingMgr.GetBuildingService(targetBuilding);
            if (targetService == ItemClass.Service.Beautification && WeatherInfo.IsBadWeather)
            {
                Log.Debug(LogCategory.Movement, TimeInfo.Now, $"{GetCitizenDesc(citizenId, ref citizen)} cancels the trip to a park due to bad weather");
                schedule.Schedule(ResidentState.AtHome);
                return false;
            }

            return true;
        }

        private ushort MoveToCommercialBuilding(TAI instance, uint citizenId, ref TCitizen citizen, float distance)
        {
            ushort currentBuilding = CitizenProxy.GetCurrentBuilding(ref citizen);
            if (currentBuilding == 0)
            {
                return 0;
            }

            ushort foundBuilding = BuildingMgr.FindActiveBuilding(currentBuilding, distance, ItemClass.Service.Commercial);
            if (foundBuilding == 0)
            {
                Log.Debug(LogCategory.Movement, $"Citizen {citizenId} didn't find any visitable commercial buildings nearby");
                return 0;
            }

            if (buildingAI.IsNoiseRestricted(foundBuilding, currentBuilding))
            {
                Log.Debug(LogCategory.Movement, $"Citizen {citizenId} won't go to the commercial building {foundBuilding}, it has a NIMBY policy");
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

                return foundBuilding;
            }

            return 0;
        }

        private ushort MoveToLeisureBuilding(TAI instance, uint citizenId, ref TCitizen citizen, ushort currentBuilding)
        {
            ushort leisureBuilding = BuildingMgr.FindActiveBuilding(
                currentBuilding,
                LeisureSearchDistance,
                ItemClass.Service.Commercial,
                ItemClass.SubService.CommercialLeisure);

            if (buildingAI.IsNoiseRestricted(leisureBuilding, currentBuilding))
            {
                Log.Debug(LogCategory.Movement, $"Citizen {citizenId} won't go to the leisure building {leisureBuilding}, it has a NIMBY policy");
                return 0;
            }

            return StartMovingToVisitBuilding(instance, citizenId, ref citizen, leisureBuilding)
                ? leisureBuilding
                : (ushort)0;
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
                CitizenProxy.SetLocation(ref citizen, Citizen.Location.Visit);
                return true;
            }

            CitizenProxy.SetVisitPlace(ref citizen, citizenId, visitBuilding);
            if (CitizenProxy.GetVisitBuilding(ref citizen) == 0)
            {
                // Building is full and doesn't accept visitors anymore
                return false;
            }

            if (!residentAI.StartMoving(instance, citizenId, ref citizen, currentBuilding, visitBuilding))
            {
                CitizenProxy.SetVisitPlace(ref citizen, citizenId, 0);
                return false;
            }

            return true;
        }
    }
}
