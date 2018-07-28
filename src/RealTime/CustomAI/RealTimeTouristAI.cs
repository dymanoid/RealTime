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
    // TODO: tourist AI should be unified with resident AI where possible
    internal sealed class RealTimeTouristAI<TAI, TCitizen> : RealTimeHumanAIBase<TCitizen>
        where TAI : class
        where TCitizen : struct
    {
        private readonly TouristAIConnection<TAI, TCitizen> touristAI;
        private readonly SpareTimeBehavior spareTimeBehavior;

        /// <summary>
        /// Initializes a new instance of the <see cref="RealTimeTouristAI{TAI, TCitizen}"/> class.
        /// </summary>
        ///
        /// <param name="config">The configuration to run with.</param>
        /// <param name="connections">A <see cref="GameConnections{T}"/> instance that provides the game connection implementation.</param>
        /// <param name="touristAI">A connection to game's tourist AI.</param>
        /// <param name="eventManager">The custom event manager.</param>
        /// <param name="spareTimeBehavior">A behavior that provides simulation info for the citizens spare time.</param>
        ///
        /// <exception cref="ArgumentNullException">Thrown when any argument is null.</exception>
        public RealTimeTouristAI(
            RealTimeConfig config,
            GameConnections<TCitizen> connections,
            TouristAIConnection<TAI, TCitizen> touristAI,
            RealTimeEventManager eventManager,
            SpareTimeBehavior spareTimeBehavior)
            : base(config, connections, eventManager)
        {
            this.touristAI = touristAI ?? throw new ArgumentNullException(nameof(touristAI));
            this.spareTimeBehavior = spareTimeBehavior ?? throw new ArgumentNullException(nameof(spareTimeBehavior));
        }

        private enum TouristTarget
        {
            DoNothing = 0,
            LeaveCity = 1,
            Shopping = 2,
            Relaxing = 3,
            Party,
            Hotel
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

            if (vehicleId == 0 && CitizenMgr.IsAreaEvacuating(instanceId) && !CitizenProxy.HasFlags(ref citizen, Citizen.Flags.Evacuating))
            {
                Log.Debug(TimeInfo.Now, $"Tourist {GetCitizenDesc(citizenId, ref citizen)} was on the way, but the area evacuates. Leaving the city.");
                touristAI.FindVisitPlace(instance, citizenId, CitizenProxy.GetCurrentBuilding(ref citizen), touristAI.GetLeavingReason(instance, citizenId, ref citizen));
                return;
            }

            if (CitizenMgr.InstanceHasFlags(instanceId, CitizenInstance.Flags.TargetIsNode | CitizenInstance.Flags.OnTour, true))
            {
                Log.Debug(TimeInfo.Now, $"Tourist {GetCitizenDesc(citizenId, ref citizen)} exits the guided tour.");
                FindRandomVisitPlace(instance, citizenId, ref citizen, TouristDoNothingProbability, 0);
                return;
            }

            ushort targetBuildingId = CitizenProxy.GetVisitBuilding(ref citizen);

            TouristTarget target;
            if (CitizenMgr.InstanceHasFlags(instanceId, CitizenInstance.Flags.TargetIsNode))
            {
                if (CitizenMgr.GetTargetNode(instanceId) != 0)
                {
                    Log.Debug($"Tourist {citizenId} moves to a target node {CitizenMgr.GetTargetNode(instanceId)}");
                    target = TouristTarget.Relaxing;
                }
                else
                {
                    return;
                }
            }
            else
            {
                BuildingMgr.GetBuildingService(targetBuildingId, out ItemClass.Service targetService, out ItemClass.SubService targetSubService);
                switch (targetService)
                {
                    // Heading to a hotel, no need to change anything
                    case ItemClass.Service.Commercial when targetSubService == ItemClass.SubService.CommercialTourist:
                        return;

                    case ItemClass.Service.Commercial when targetSubService == ItemClass.SubService.CommercialLeisure:
                        target = TouristTarget.Party;
                        break;

                    case ItemClass.Service.Tourism:
                    case ItemClass.Service.Beautification:
                    case ItemClass.Service.Monument:
                        target = TouristTarget.Relaxing;
                        break;

                    case ItemClass.Service.Commercial:
                        target = TouristTarget.Shopping;
                        break;

                    default:
                        return;
                }
            }

            if (GetTouristGoingOutChance(ref citizen, target) > 0)
            {
                return;
            }

            ushort hotel = FindHotel(ref citizen, targetBuildingId);
            if (hotel != 0)
            {
                Log.Debug(TimeInfo.Now, $"Tourist {GetCitizenDesc(citizenId, ref citizen)} changes the target and moves to a hotel {hotel} because of time or weather");
                StartMovingToVisitBuilding(instance, citizenId, ref citizen, 0, hotel);
            }
            else
            {
                Log.Debug(TimeInfo.Now, $"Tourist {GetCitizenDesc(citizenId, ref citizen)} leaves the city because of time or weather");
                touristAI.FindVisitPlace(instance, citizenId, 0, touristAI.GetLeavingReason(instance, citizenId, ref citizen));
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
                        FindRandomVisitPlace(instance, citizenId, ref citizen, 0, visitBuilding);
                    }

                    return;

                // Tourist is sleeping in a hotel
                // TODO: tourist hotel leave time
                case ItemClass.Service.Commercial
                    when TimeInfo.IsNightTime && BuildingMgr.GetBuildingSubService(visitBuilding) == ItemClass.SubService.CommercialTourist:
                    return;
            }

            if (Random.ShouldOccur(TouristEventChance) && !IsBadWeather())
            {
                ICityEvent cityEvent = GetUpcomingEventToAttend(citizenId, ref citizen);
                if (cityEvent != null)
                {
                    StartMovingToVisitBuilding(instance, citizenId, ref citizen, CitizenProxy.GetCurrentBuilding(ref citizen), cityEvent.BuildingId);
                    Log.Debug(TimeInfo.Now, $"Tourist {GetCitizenDesc(citizenId, ref citizen)} attending an event at {cityEvent.BuildingId}");
                    return;
                }
            }

            int doNothingChance;
            switch (EventMgr.GetEventState(visitBuilding, DateTime.MaxValue))
            {
                case CityEventState.Ongoing:
                    if (Random.ShouldOccur(TouristShoppingChance))
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

            FindRandomVisitPlace(instance, citizenId, ref citizen, doNothingChance, visitBuilding);
        }

        private void FindRandomVisitPlace(TAI instance, uint citizenId, ref TCitizen citizen, int doNothingProbability, ushort currentBuilding)
        {
            var target = (TouristTarget)touristAI.GetRandomTargetType(instance, doNothingProbability);
            target = AdjustTargetToTimeAndWeather(ref citizen, target);

            switch (target)
            {
                case TouristTarget.LeaveCity:
                    Log.Debug(TimeInfo.Now, $"Tourist {GetCitizenDesc(citizenId, ref citizen)} decides to leave the city");
                    touristAI.FindVisitPlace(instance, citizenId, currentBuilding, touristAI.GetLeavingReason(instance, citizenId, ref citizen));
                    break;

                case TouristTarget.Shopping:
                    touristAI.FindVisitPlace(instance, citizenId, currentBuilding, touristAI.GetShoppingReason(instance));
                    Log.Debug(TimeInfo.Now, $"Tourist {GetCitizenDesc(citizenId, ref citizen)} stays in the city, goes shopping");
                    break;

                case TouristTarget.Relaxing:
                    Log.Debug(TimeInfo.Now, $"Tourist {GetCitizenDesc(citizenId, ref citizen)} stays in the city, goes relaxing");
                    touristAI.FindVisitPlace(instance, citizenId, currentBuilding, touristAI.GetEntertainmentReason(instance));
                    break;

                case TouristTarget.Party:
                    ushort leisureBuilding = BuildingMgr.FindActiveBuilding(
                        currentBuilding,
                        LeisureSearchDistance,
                        ItemClass.Service.Commercial,
                        ItemClass.SubService.CommercialLeisure);
                    if (leisureBuilding == 0)
                    {
                        goto case TouristTarget.Hotel;
                    }

                    Log.Debug(TimeInfo.Now, $"Tourist {GetCitizenDesc(citizenId, ref citizen)} want to party in {leisureBuilding}");
                    StartMovingToVisitBuilding(instance, citizenId, ref citizen, currentBuilding, leisureBuilding);
                    break;

                case TouristTarget.Hotel:
                    ushort hotel = FindHotel(ref citizen, currentBuilding);
                    if (hotel == 0)
                    {
                        goto case TouristTarget.LeaveCity;
                    }

                    Log.Debug(TimeInfo.Now, $"Tourist {GetCitizenDesc(citizenId, ref citizen)} want to stay in a hotel {hotel}");
                    StartMovingToVisitBuilding(instance, citizenId, ref citizen, currentBuilding, hotel);
                    break;
            }
        }

        private TouristTarget AdjustTargetToTimeAndWeather(ref TCitizen citizen, TouristTarget target)
        {
            switch (target)
            {
                case TouristTarget.Shopping:
                case TouristTarget.Relaxing:
                case TouristTarget.Party:
                    uint goingOutChance = GetTouristGoingOutChance(ref citizen, target);
                    if (!Random.ShouldOccur(goingOutChance))
                    {
                        return TouristTarget.Hotel;
                    }

                    if (target == TouristTarget.Relaxing && TimeInfo.IsNightTime)
                    {
                        return TouristTarget.Party;
                    }

                    goto default;

                default:
                    return target;
            }
        }

        private uint GetTouristGoingOutChance(ref TCitizen citizen, TouristTarget target)
        {
            Citizen.AgeGroup age = CitizenProxy.GetAge(ref citizen);
            switch (target)
            {
                case TouristTarget.Shopping:
                    return spareTimeBehavior.GetShoppingChance(age);

                case TouristTarget.Relaxing when TimeInfo.IsNightTime || IsBadWeather():
                    return 0u;

                case TouristTarget.Party:
                case TouristTarget.Relaxing:
                    return spareTimeBehavior.GetRelaxingChance(age);

                default:
                    return 100u;
            }
        }

        private ushort FindHotel(ref TCitizen citizen, ushort currentBuilding)
        {
            if (!Random.ShouldOccur(FindHotelChance))
            {
                return 0;
            }

            return BuildingMgr.FindActiveBuilding(
                currentBuilding,
                HotelSearchDistance,
                ItemClass.Service.Commercial,
                ItemClass.SubService.CommercialTourist);
        }

        private void StartMovingToVisitBuilding(TAI instance, uint citizenId, ref TCitizen citizen, ushort currentBuilding, ushort visitBuilding)
        {
            if (touristAI.StartMoving(instance, citizenId, ref citizen, currentBuilding, visitBuilding))
            {
                CitizenProxy.SetVisitPlace(ref citizen, citizenId, visitBuilding);
            }
        }
    }
}
