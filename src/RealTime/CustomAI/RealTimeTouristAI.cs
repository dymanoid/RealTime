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

    /// <summary>
    /// A class incorporating the custom logic for the tourists that visit the city.
    /// </summary>
    /// <typeparam name="TAI">The type of the tourist AI.</typeparam>
    /// <typeparam name="TCitizen">The type of the citizen objects.</typeparam>
    /// <seealso cref="RealTimeHumanAIBase{TCitizen}" />
    internal sealed class RealTimeTouristAI<TAI, TCitizen> : RealTimeHumanAIBase<TCitizen>
        where TAI : class
        where TCitizen : struct
    {
        private readonly TouristAIConnection<TAI, TCitizen> touristAI;

        /// <summary>
        /// Initializes a new instance of the <see cref="RealTimeTouristAI{TAI, TCitizen}"/> class.
        /// </summary>
        ///
        /// <param name="config">The configuration to run with.</param>
        /// <param name="connections">A <see cref="GameConnections{T}"/> instance that provides the game connection implementation.</param>
        /// <param name="touristAI">A connection to game's tourist AI.</param>
        /// <param name="eventManager">The custom event manager.</param>
        ///
        /// <exception cref="ArgumentNullException">Thrown when any argument is null.</exception>
        public RealTimeTouristAI(
            RealTimeConfig config,
            GameConnections<TCitizen> connections,
            TouristAIConnection<TAI, TCitizen> touristAI,
            RealTimeEventManager eventManager)
            : base(config, connections, eventManager)
        {
            this.touristAI = touristAI ?? throw new ArgumentNullException(nameof(touristAI));
        }

        /// <summary>
        /// The entry method of the custom AI.
        /// </summary>
        ///
        /// <param name="instance">A reference to an object instance of the original AI.</param>
        /// <param name="citizenId">The ID of the citizen to process.</param>
        /// <param name="citizen">A <typeparamref name="TCitizen"/> reference to process.</param>
        public void UpdateLocation(TAI instance, uint citizenId, ref TCitizen citizen)
        {
            if (!EnsureCitizenCanBeProcessed(citizenId, ref citizen))
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
                    ProcessVisit(instance, citizenId, ref citizen, IsCitizenVirtual(instance, ref citizen, ShouldRealizeCitizen));
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

            if (vehicleId == 0 && CitizenMgr.IsAreaEvacuating(instanceId) && !CitizenProxy.HasFlags(ref citizen, Citizen.Flags.Evacuating))
            {
                Log.Debug(TimeInfo.Now, $"Tourist {GetCitizenDesc(citizenId, ref citizen, false)} was on the way, but the area evacuates. Leaving the city.");
                touristAI.FindVisitPlace(instance, citizenId, CitizenProxy.GetCurrentBuilding(ref citizen), touristAI.GetLeavingReason(instance, citizenId, ref citizen));
                return;
            }

            if (CitizenMgr.InstanceHasFlags(instanceId, CitizenInstance.Flags.TargetIsNode | CitizenInstance.Flags.OnTour, true))
            {
                Log.Debug(TimeInfo.Now, $"Tourist {GetCitizenDesc(citizenId, ref citizen, false)} exits the guided tour.");
                FindRandomVisitPlace(instance, citizenId, ref citizen, TouristDoNothingProbability, 0, false);
            }
        }

        private void ProcessVisit(TAI instance, uint citizenId, ref TCitizen citizen, bool isVirtual)
        {
            ushort visitBuilding = CitizenProxy.GetVisitBuilding(ref citizen);
            if (visitBuilding == 0)
            {
                CitizenMgr.ReleaseCitizen(citizenId);
                return;
            }

            if (BuildingMgr.BuildingHasFlags(visitBuilding, Building.Flags.Evacuating))
            {
                touristAI.FindEvacuationPlace(instance, citizenId, visitBuilding, touristAI.GetEvacuationReason(instance, visitBuilding));
                return;
            }

            switch (BuildingMgr.GetBuildingService(visitBuilding))
            {
                case ItemClass.Service.Disaster:
                    if (BuildingMgr.BuildingHasFlags(visitBuilding, Building.Flags.Downgrading))
                    {
                        FindRandomVisitPlace(instance, citizenId, ref citizen, 0, visitBuilding, false);
                    }

                    return;

                // Tourist is sleeping in a hotel
                case ItemClass.Service.Commercial
                    when TimeInfo.IsNightTime && BuildingMgr.GetBuildingSubService(visitBuilding) == ItemClass.SubService.CommercialTourist:
                    return;
            }

            if (IsChance(TouristEventChance) && AttendUpcomingEvent(citizenId, ref citizen, out ushort eventBuilding))
            {
                StartMovingToVisitBuilding(instance, citizenId, ref citizen, CitizenProxy.GetCurrentBuilding(ref citizen), eventBuilding, isVirtual);
                Log.Debug(TimeInfo.Now, $"Tourist {GetCitizenDesc(citizenId, ref citizen, isVirtual)} attending an event at {eventBuilding}");
                return;
            }

            int doNothingChance;
            switch (EventMgr.GetEventState(visitBuilding, DateTime.MaxValue))
            {
                case CityEventState.Ongoing:
                    if (IsChance(TouristShoppingChance))
                    {
                        BuildingMgr.ModifyMaterialBuffer(visitBuilding, TransferManager.TransferReason.Shopping, -ShoppingGoodsAmount);
                    }

                    return;

                case CityEventState.Finished:
                    doNothingChance = 0;
                    break;

                default:
                    doNothingChance = TouristDoNothingProbability;
                    break;
            }

            FindRandomVisitPlace(instance, citizenId, ref citizen, doNothingChance, visitBuilding, isVirtual);
        }

        private void FindRandomVisitPlace(TAI instance, uint citizenId, ref TCitizen citizen, int doNothingProbability, ushort currentBuilding, bool isVirtual)
        {
            int targetType = touristAI.GetRandomTargetType(instance, doNothingProbability);
            if (targetType == 1)
            {
                Log.Debug(TimeInfo.Now, $"Tourist {GetCitizenDesc(citizenId, ref citizen, isVirtual)} decides to leave the city");
                touristAI.FindVisitPlace(instance, citizenId, currentBuilding, touristAI.GetLeavingReason(instance, citizenId, ref citizen));
                return;
            }

            if (!IsChance(GetGoOutChance(CitizenProxy.GetAge(ref citizen))))
            {
                FindHotel(instance, citizenId, ref citizen, isVirtual);
                return;
            }

            if (isVirtual)
            {
                if (IsChance(TouristShoppingChance) && BuildingMgr.GetBuildingService(currentBuilding) == ItemClass.Service.Commercial)
                {
                    BuildingMgr.ModifyMaterialBuffer(currentBuilding, TransferManager.TransferReason.Shopping, -ShoppingGoodsAmount);
                }

                touristAI.AddTouristVisit(instance, citizenId, currentBuilding);

                return;
            }

            switch (targetType)
            {
                case 2:
                    touristAI.FindVisitPlace(instance, citizenId, currentBuilding, touristAI.GetShoppingReason(instance));
                    Log.Debug(TimeInfo.Now, $"Tourist {GetCitizenDesc(citizenId, ref citizen, isVirtual)} stays in the city, goes shopping");
                    break;

                case 3:
                    Log.Debug(TimeInfo.Now, $"Tourist {GetCitizenDesc(citizenId, ref citizen, isVirtual)} stays in the city, goes relaxing");
                    touristAI.FindVisitPlace(instance, citizenId, currentBuilding, touristAI.GetEntertainmentReason(instance));
                    break;
            }
        }

        private void FindHotel(TAI instance, uint citizenId, ref TCitizen citizen, bool isVirtual)
        {
            ushort currentBuilding = CitizenProxy.GetCurrentBuilding(ref citizen);
            if (!IsChance(FindHotelChance))
            {
                Log.Debug(TimeInfo.Now, $"Tourist {GetCitizenDesc(citizenId, ref citizen, isVirtual)} didn't want to stay in a hotel, leaving the city");
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
                Log.Debug(TimeInfo.Now, $"Tourist {GetCitizenDesc(citizenId, ref citizen, isVirtual)} didn't find a hotel, leaving the city");
                touristAI.FindVisitPlace(instance, citizenId, currentBuilding, touristAI.GetLeavingReason(instance, citizenId, ref citizen));
                return;
            }

            StartMovingToVisitBuilding(instance, citizenId, ref citizen, currentBuilding, hotel, isVirtual);
            Log.Debug(TimeInfo.Now, $"Tourist {GetCitizenDesc(citizenId, ref citizen, isVirtual)} stays in a hotel {hotel}");
        }

        private void StartMovingToVisitBuilding(TAI instance, uint citizenId, ref TCitizen citizen, ushort currentBuilding, ushort visitBuilding, bool isVirtual)
        {
            CitizenProxy.SetVisitPlace(ref citizen, citizenId, visitBuilding);
            CitizenProxy.SetVisitBuilding(ref citizen, visitBuilding);

            if (isVirtual)
            {
                CitizenProxy.SetLocation(ref citizen, Citizen.Location.Visit);
                touristAI.AddTouristVisit(instance, citizenId, visitBuilding);
            }
            else
            {
                touristAI.StartMoving(instance, citizenId, ref citizen, currentBuilding, visitBuilding);
            }
        }

        private bool ShouldRealizeCitizen(TAI ai)
        {
            return touristAI.DoRandomMove(ai);
        }
    }
}
