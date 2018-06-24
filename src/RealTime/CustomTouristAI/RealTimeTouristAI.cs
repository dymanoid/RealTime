// <copyright file="RealTimeTouristAI.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.CustomAI
{
    using System;
    using RealTime.Config;
    using RealTime.Events;
    using RealTime.GameConnection;
    using RealTime.Tools;
    using static Constants;

    internal sealed class RealTimeTouristAI<TAI, TCitizen> : RealTimeHumanAIBase<TCitizen>
        where TAI : class
        where TCitizen : struct
    {
        private readonly TouristAIConnection<TAI, TCitizen> touristAI;

        public RealTimeTouristAI(
            RealTimeConfig config,
            GameConnections<TCitizen> connections,
            TouristAIConnection<TAI, TCitizen> touristAI,
            RealTimeEventManager eventManager)
            : base(config, connections, eventManager)
        {
            this.touristAI = touristAI ?? throw new System.ArgumentNullException(nameof(touristAI));
        }

        public void UpdateLocation(TAI instance, uint citizenId, ref TCitizen citizen)
        {
            if (!EnsureCitizenValid(citizenId, ref citizen))
            {
                return;
            }

            if (CitizenProxy.IsDead(ref citizen) || CitizenProxy.IsSick(ref citizen))
            {
                CitizenMgr.ReleaseCitizen(citizenId);
                return;
            }

            switch (CitizenProxy.GetLocation(ref citizen))
            {
                case Citizen.Location.Home:
                case Citizen.Location.Work:
                    CitizenMgr.ReleaseCitizen(citizenId);
                    break;

                case Citizen.Location.Visit:
                    ProcessVisit(instance, citizenId, ref citizen);
                    break;

                case Citizen.Location.Moving:
                    ProcessMoving(instance, citizenId, ref citizen);
                    break;
            }
        }

        private void ProcessMoving(TAI instance, uint citizenId, ref TCitizen citizen)
        {
            ushort instanceId = CitizenProxy.GetInstance(ref citizen);
            ushort vehicleId = CitizenProxy.GetVehicle(ref citizen);

            if (instanceId == 0)
            {
                if (vehicleId == 0)
                {
                    CitizenMgr.ReleaseCitizen(citizenId);
                }

                return;
            }

            if (vehicleId == 0 && CitizenMgr.IsAreaEvacuating(instanceId) && (CitizenProxy.GetFlags(ref citizen) & Citizen.Flags.Evacuating) == 0)
            {
                Log.Debug(TimeInfo.Now, $"Tourist {GetCitizenDesc(citizenId, ref citizen)} was on the way, but the area evacuates. Leaving the city.");
                touristAI.FindVisitPlace(instance, citizenId, CitizenProxy.GetCurrentBuilding(ref citizen), touristAI.GetLeavingReason(instance, citizenId, ref citizen));
                return;
            }

            CitizenInstance.Flags flags = CitizenInstance.Flags.TargetIsNode | CitizenInstance.Flags.OnTour;
            if ((CitizenMgr.GetInstanceFlags(instanceId) & flags) == flags)
            {
                FindRandomVisitPlace(instance, citizenId, ref citizen, TouristDoNothingProbability, 0);
            }
        }

        private void ProcessVisit(TAI instance, uint citizenId, ref TCitizen citizen)
        {
            ushort visitBuilding = CitizenProxy.GetVisitBuilding(ref citizen);
            if (visitBuilding == 0)
            {
                CitizenMgr.ReleaseCitizen(citizenId);
                return;
            }

            Building.Flags buildingFlags = BuildingMgr.GetBuildingFlags(visitBuilding);
            if ((buildingFlags & Building.Flags.Evacuating) != 0)
            {
                touristAI.FindEvacuationPlace(instance, citizenId, visitBuilding, touristAI.GetEvacuationReason(instance, visitBuilding));
                return;
            }

            switch (BuildingMgr.GetBuildingService(visitBuilding))
            {
                case ItemClass.Service.Disaster:
                    if ((buildingFlags & Building.Flags.Downgrading) != 0)
                    {
                        FindRandomVisitPlace(instance, citizenId, ref citizen, 0, visitBuilding);
                    }

                    return;

                // Tourist is sleeping in a hotel
                case ItemClass.Service.Commercial
                    when TimeInfo.IsNightTime && BuildingMgr.GetBuildingSubService(visitBuilding) == ItemClass.SubService.CommercialTourist:
                    return;
            }

            if (IsChance(TouristEventChance) && AttendUpcomingEvent(citizenId, ref citizen, out ushort eventBuilding))
            {
                StartMovingToVisitBuilding(instance, citizenId, ref citizen, CitizenProxy.GetCurrentBuilding(ref citizen), eventBuilding);
                touristAI.AddTouristVisit(instance, citizenId, eventBuilding);
                Log.Debug(TimeInfo.Now, $"Tourist {GetCitizenDesc(citizenId, ref citizen)} attending an event at {eventBuilding}");
                return;
            }

            bool doShopping;
            switch (EventMgr.GetEventState(visitBuilding, DateTime.MaxValue))
            {
                case CityEventState.OnGoing:
                    doShopping = IsChance(TouristShoppingChance);
                    break;

                case CityEventState.Finished:
                    doShopping = !FindRandomVisitPlace(instance, citizenId, ref citizen, 0, visitBuilding);
                    break;

                default:
                    doShopping = false;
                    break;
            }

            if (doShopping || !FindRandomVisitPlace(instance, citizenId, ref citizen, TouristDoNothingProbability, visitBuilding))
            {
                BuildingMgr.ModifyMaterialBuffer(visitBuilding, TransferManager.TransferReason.Shopping, -ShoppingGoodsAmount);
                touristAI.AddTouristVisit(instance, citizenId, visitBuilding);
            }
        }

        private bool FindRandomVisitPlace(TAI instance, uint citizenId, ref TCitizen citizen, int doNothingProbability, ushort visitBuilding)
        {
            int targetType = touristAI.GetRandomTargetType(instance, doNothingProbability);
            if (targetType == 1)
            {
                Log.Debug(TimeInfo.Now, $"Tourist {GetCitizenDesc(citizenId, ref citizen)} decides to leave the city");
                touristAI.FindVisitPlace(instance, citizenId, visitBuilding, touristAI.GetLeavingReason(instance, citizenId, ref citizen));
                return true;
            }

            if (CitizenProxy.GetInstance(ref citizen) == 0 && !touristAI.DoRandomMove(instance))
            {
                return false;
            }

            if (!IsChance(GetGoOutChance(CitizenProxy.GetAge(ref citizen))))
            {
                FindHotel(instance, citizenId, ref citizen);
                return true;
            }

            switch (targetType)
            {
                case 2:
                    touristAI.FindVisitPlace(instance, citizenId, visitBuilding, touristAI.GetShoppingReason(instance));
                    Log.Debug(TimeInfo.Now, $"Tourist {GetCitizenDesc(citizenId, ref citizen)} stays in the city, goes shopping");
                    break;

                case 3:
                    Log.Debug(TimeInfo.Now, $"Tourist {GetCitizenDesc(citizenId, ref citizen)} stays in the city, goes relaxing");
                    touristAI.FindVisitPlace(instance, citizenId, visitBuilding, touristAI.GetEntertainmentReason(instance));
                    break;
            }

            return true;
        }

        private void FindHotel(TAI instance, uint citizenId, ref TCitizen citizen)
        {
            ushort currentBuilding = CitizenProxy.GetCurrentBuilding(ref citizen);
            if (!IsChance(FindHotelChance))
            {
                Log.Debug(TimeInfo.Now, $"Tourist {GetCitizenDesc(citizenId, ref citizen)} didn't want to stay in a hotel, leaving the city");
                touristAI.FindVisitPlace(instance, citizenId, currentBuilding, touristAI.GetLeavingReason(instance, citizenId, ref citizen));
                return;
            }

            ushort hotel = BuildingMgr.FindActiveBuilding(
                currentBuilding,
                FullSearchDistance,
                ItemClass.Service.Commercial,
                ItemClass.SubService.CommercialTourist);

            if (hotel == 0)
            {
                Log.Debug(TimeInfo.Now, $"Tourist {GetCitizenDesc(citizenId, ref citizen)} didn't find a hotel, leaving the city");
                touristAI.FindVisitPlace(instance, citizenId, currentBuilding, touristAI.GetLeavingReason(instance, citizenId, ref citizen));
                return;
            }

            StartMovingToVisitBuilding(instance, citizenId, ref citizen, currentBuilding, hotel);

            touristAI.AddTouristVisit(instance, citizenId, hotel);
            Log.Debug(TimeInfo.Now, $"Tourist {GetCitizenDesc(citizenId, ref citizen)} stays in a hotel {hotel}");
        }

        private void StartMovingToVisitBuilding(TAI instance, uint citizenId, ref TCitizen citizen, ushort currentBuilding, ushort visitBuilding)
        {
            touristAI.StartMoving(instance, citizenId, ref citizen, currentBuilding, visitBuilding);
            CitizenProxy.SetVisitPlace(ref citizen, citizenId, visitBuilding);
            CitizenProxy.SetVisitBuilding(ref citizen, visitBuilding);
        }
    }
}
