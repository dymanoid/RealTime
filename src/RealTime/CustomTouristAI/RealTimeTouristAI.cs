// <copyright file="RealTimeTouristAI.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.CustomAI
{
    using RealTime.Config;
    using RealTime.GameConnection;
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

            ItemClass.Service service = BuildingManager.GetBuildingService(visitBuilding);
            Building.Flags buildingFlags = BuildingManager.GetBuildingFlags(visitBuilding);

            if (service == ItemClass.Service.Disaster)
            {
                if ((buildingFlags & Building.Flags.Downgrading) != 0)
                {
                    FindRandomVisitPlace(instance, citizenId, ref citizen, 0, visitBuilding);
                }

                return;
            }

            if ((buildingFlags & Building.Flags.Evacuating) != 0)
            {
                touristAI.FindEvacuationPlace(instance, citizenId, visitBuilding, touristAI.GetEvacuationReason(instance, visitBuilding));
                return;
            }

            ushort eventIndex = BuildingManager.GetEvent(visitBuilding);
            if (eventIndex != 0)
            {
                EventData.Flags eventFlags = EventData.Flags.Preparing | EventData.Flags.Active | EventData.Flags.Ready;
                if ((EventManager.GetEventFlags(eventIndex) & eventFlags) == 0)
                {
                    FindRandomVisitPlace(instance, citizenId, ref citizen, 0, visitBuilding);
                }
                else if (IsChance(TouristShoppingQuota))
                {
                    BuildingManager.ModifyMaterialBuffer(visitBuilding, TransferManager.TransferReason.Shopping, -ShoppingGoodsAmount);
                    touristAI.AddTouristVisit(instance, citizenId, visitBuilding);
                }

                return;
            }

            TransferManager.TransferReason transferReason;
            switch (touristAI.GetRandomTargetType(instance, TouristDoNothingProbability))
            {
                case 1:
                    transferReason = touristAI.GetLeavingReason(instance, citizenId, ref citizen);
                    touristAI.FindVisitPlace(instance, citizenId, visitBuilding, transferReason);
                    return;

                case 2:
                    transferReason = touristAI.GetShoppingReason(instance);
                    break;

                case 3:
                    transferReason = touristAI.GetEntertainmentReason(instance);
                    break;

                default:
                    return;
            }

            if (CitizenProxy.GetInstance(ref citizen) != 0 || touristAI.DoRandomMove(instance))
            {
                touristAI.FindVisitPlace(instance, citizenId, visitBuilding, transferReason);
            }
            else
            {
                BuildingManager.ModifyMaterialBuffer(visitBuilding, TransferManager.TransferReason.Shopping, -ShoppingGoodsAmount);
                touristAI.AddTouristVisit(instance, citizenId, visitBuilding);
            }
        }

        private void FindRandomVisitPlace(TAI instance, uint citizenId, ref TCitizen citizen, int doNothingProbability, ushort visitBuilding)
        {
            switch (touristAI.GetRandomTargetType(instance, doNothingProbability))
            {
                case 1:
                    touristAI.FindVisitPlace(instance, citizenId, visitBuilding, touristAI.GetLeavingReason(instance, citizenId, ref citizen));
                    break;
                case 2:
                    touristAI.FindVisitPlace(instance, citizenId, visitBuilding, touristAI.GetShoppingReason(instance));
                    break;
                case 3:
                    touristAI.FindVisitPlace(instance, citizenId, visitBuilding, touristAI.GetEntertainmentReason(instance));
                    break;
            }
        }
    }
}
