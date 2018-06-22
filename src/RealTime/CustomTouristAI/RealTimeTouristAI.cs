// <copyright file="RealTimeTouristAI.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.CustomAI
{
    using RealTime.Config;
    using RealTime.GameConnection;
    using RealTime.Tools;
    using static Constants;

    internal sealed class RealTimeTouristAI<TAI, TCitizen> : RealTimeHumanAIBase<TCitizen>
        where TAI : class
        where TCitizen : struct
    {
        private readonly TouristAIConnection<TAI, TCitizen> touristAI;

        public RealTimeTouristAI(Configuration config, GameConnections<TCitizen> connections, TouristAIConnection<TAI, TCitizen> touristAI)
            : base(config, connections)
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
                CitizenManager.ReleaseCitizen(citizenId);
                return;
            }

            switch (CitizenProxy.GetLocation(ref citizen))
            {
                case Citizen.Location.Home:
                case Citizen.Location.Work:
                    CitizenManager.ReleaseCitizen(citizenId);
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
            if (instanceId == 0)
            {
                if (CitizenProxy.GetVehicle(ref citizen) == 0)
                {
                    CitizenManager.ReleaseCitizen(citizenId);
                }

                return;
            }

            CitizenInstance.Flags flags = CitizenInstance.Flags.TargetIsNode | CitizenInstance.Flags.OnTour;
            if ((CitizenManager.GetInstanceFlags(instanceId) & flags) == flags)
            {
                FindRandomVisitPlace(instance, citizenId, ref citizen, TouristDoNothingProbability, 0);
            }
        }

        private void ProcessVisit(TAI instance, uint citizenId, ref TCitizen citizen)
        {
            ushort visitBuilding = CitizenProxy.GetVisitBuilding(ref citizen);
            if (visitBuilding == 0)
            {
                CitizenManager.ReleaseCitizen(citizenId);
                return;
            }

            Building.Flags buildingFlags = BuildingManager.GetBuildingFlags(visitBuilding);
            if ((buildingFlags & Building.Flags.Evacuating) != 0)
            {
                touristAI.FindEvacuationPlace(instance, citizenId, visitBuilding, touristAI.GetEvacuationReason(instance, visitBuilding));
                return;
            }

            switch (BuildingManager.GetBuildingService(visitBuilding))
            {
                case ItemClass.Service.Disaster:
                    if ((buildingFlags & Building.Flags.Downgrading) != 0)
                    {
                        FindRandomVisitPlace(instance, citizenId, ref citizen, 0, visitBuilding);
                    }

                    return;

                // Tourist is sleeping in a hotel
                case ItemClass.Service.Commercial
                    when TimeInfo.IsNightTime && BuildingManager.GetBuildingSubService(visitBuilding) == ItemClass.SubService.CommercialTourist:
                    return;
            }

            // TODO: add events here
            bool doShopping;
            ushort eventIndex = BuildingManager.GetEvent(visitBuilding);
            if (eventIndex != 0)
            {
                EventData.Flags eventFlags = EventData.Flags.Preparing | EventData.Flags.Active | EventData.Flags.Ready;
                doShopping = (EventManager.GetEventFlags(eventIndex) & eventFlags) == 0
                    ? !FindRandomVisitPlace(instance, citizenId, ref citizen, 0, visitBuilding)
                    : IsChance(TouristShoppingChance);

                if (!doShopping)
                {
                    return;
                }
            }
            else
            {
                doShopping = false;
            }

            if (doShopping || !FindRandomVisitPlace(instance, citizenId, ref citizen, TouristDoNothingProbability, visitBuilding))
            {
                BuildingManager.ModifyMaterialBuffer(visitBuilding, TransferManager.TransferReason.Shopping, -ShoppingGoodsAmount);
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

            ushort hotel = BuildingManager.FindActiveBuilding(
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

            touristAI.StartMoving(instance, citizenId, ref citizen, currentBuilding, hotel);
            CitizenProxy.SetVisitPlace(ref citizen, citizenId, hotel);
            CitizenProxy.SetVisitBuilding(ref citizen, hotel);
            touristAI.AddTouristVisit(instance, citizenId, hotel);
            Log.Debug(TimeInfo.Now, $"Tourist {GetCitizenDesc(citizenId, ref citizen)} stays in a hotel {hotel}");
        }
    }
}
