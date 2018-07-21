// <copyright file="RealTimeHumanAIBase.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.CustomAI
{
    using System;
    using RealTime.Config;
    using RealTime.Events;
    using RealTime.GameConnection;
    using RealTime.Simulation;
    using RealTime.Tools;
    using static Constants;

    /// <summary>
    /// A base class for the custom logic of a human in the game.
    /// </summary>
    ///
    /// <typeparam name="TCitizen">The type of the human object to process.</typeparam>
    internal abstract class RealTimeHumanAIBase<TCitizen>
        where TCitizen : struct
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RealTimeHumanAIBase{TCitizen}"/> class.
        /// </summary>
        ///
        /// <exception cref="ArgumentNullException">Thrown when any argument is null.</exception>
        ///
        /// <param name="config">A configuration to use with this custom logic.</param>
        /// <param name="connections">An object providing the proxies that connect the method calls to the game methods.</param>
        /// <param name="eventManager">A reference an <see cref="RealTimeEventManager"/> instance.</param>
        protected RealTimeHumanAIBase(RealTimeConfig config, GameConnections<TCitizen> connections, RealTimeEventManager eventManager)
        {
            Config = config ?? throw new ArgumentNullException(nameof(config));
            EventMgr = eventManager ?? throw new ArgumentNullException(nameof(eventManager));

            if (connections == null)
            {
                throw new ArgumentNullException(nameof(connections));
            }

            CitizenMgr = connections.CitizenManager;
            BuildingMgr = connections.BuildingManager;
            TransferMgr = connections.TransferManager;
            CitizenProxy = connections.CitizenConnection;
            TimeInfo = connections.TimeInfo;
            Random = connections.Random;
            WeatherInfo = connections.WeatherInfo;

            CitizenInstancesMaxCount = CitizenMgr.GetMaxInstancesCount();
        }

        /// <summary>
        /// Gets a value indicating whether the current date represents a Weekend day.
        /// </summary>
        protected bool IsWeekend => Config.IsWeekendEnabled && TimeInfo.Now.IsWeekend();

        /// <summary>
        /// Gets a value indicating whether the current date represents a work day.
        /// </summary>
        protected bool IsWorkDay => !Config.IsWeekendEnabled || !TimeInfo.Now.IsWeekend();

        /// <summary>
        /// Gets the current game configuration.
        /// </summary>
        protected RealTimeConfig Config { get; }

        /// <summary>
        /// Gets a reference to the event manager.
        /// </summary>
        protected RealTimeEventManager EventMgr { get; }

        /// <summary>
        /// Gets a reference to the proxy class that provides access to citizen's methods and fields.
        /// </summary>
        protected ICitizenConnection<TCitizen> CitizenProxy { get; }

        /// <summary>
        /// Gets a reference to the citizen manager proxy object.
        /// </summary>
        protected ICitizenManagerConnection CitizenMgr { get; }

        /// <summary>
        /// Gets a reference to the building manager proxy object.
        /// </summary>
        protected IBuildingManagerConnection BuildingMgr { get; }

        /// <summary>
        /// Gets a reference to the transfer manager proxy object.
        /// </summary>
        protected ITransferManagerConnection TransferMgr { get; }

        /// <summary>
        /// Gets the current time information.
        /// </summary>
        protected ITimeInfo TimeInfo { get; }

        /// <summary>
        /// Gets a reference to the game's randomizer.
        /// </summary>
        protected IRandomizer Random { get; }

        /// <summary>Gets the current weather info.</summary>
        protected IWeatherInfo WeatherInfo { get; }

        /// <summary>Gets the maximum count of the citizen instances.</summary>
        protected uint CitizenInstancesMaxCount { get; }

        /// <summary>
        /// Ensures that the provided citizen is in a valid state and can be processed.
        /// </summary>
        ///
        /// <param name="citizenId">The citizen ID to check.</param>
        /// <param name="citizen">The citizen data reference.</param>
        ///
        /// <returns><c>true</c> if the provided citizen is in a valid state; otherwise, <c>false</c>.</returns>
        protected bool EnsureCitizenCanBeProcessed(uint citizenId, ref TCitizen citizen)
        {
            if ((CitizenProxy.GetHomeBuilding(ref citizen) == 0
                && CitizenProxy.GetWorkBuilding(ref citizen) == 0
                && CitizenProxy.GetVisitBuilding(ref citizen) == 0
                && CitizenProxy.GetInstance(ref citizen) == 0
                && CitizenProxy.GetVehicle(ref citizen) == 0)
                ||
                (CitizenProxy.HasFlags(ref citizen, Citizen.Flags.MovingIn) && CitizenProxy.GetLocation(ref citizen) == Citizen.Location.Home))
            {
                CitizenMgr.ReleaseCitizen(citizenId);
                return false;
            }

            if (CitizenProxy.IsCollapsed(ref citizen))
            {
                Log.Debug($"{GetCitizenDesc(citizenId, ref citizen)} is collapsed, doing nothing...");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Searches for an upcoming event and checks whether the specified citizen ca attend it.
        /// Returns null if no matching events found.
        /// </summary>
        ///
        /// <param name="citizenId">The ID of the citizen to check.</param>
        /// <param name="citizen">The citizen data reference.</param>
        ///
        /// <returns>The city event or null if none found.</returns>
        protected ICityEvent GetUpcomingEventToAttend(uint citizenId, ref TCitizen citizen)
        {
            ushort currentBuilding = CitizenProxy.GetCurrentBuilding(ref citizen);
            if (EventMgr.GetEventState(currentBuilding, DateTime.MaxValue) == CityEventState.Ongoing)
            {
                return null;
            }

            DateTime earliestStart = TimeInfo.Now.AddHours(MinTravelTime);
            DateTime latestStart = TimeInfo.Now.AddHours(MaxTravelTime);

            ICityEvent upcomingEvent = EventMgr.GetUpcomingCityEvent(earliestStart, latestStart);
            if (upcomingEvent != null && CanAttendEvent(citizenId, ref citizen, upcomingEvent))
            {
                return upcomingEvent;
            }

            return null;
        }

        /// <summary>
        /// Finds an evacuation place for the provided citizen.
        /// </summary>
        ///
        /// <param name="citizenId">The citizen ID to find an evacuation place for.</param>
        ///
        /// <param name="reason">The evacuation reason.</param>
        protected void FindEvacuationPlace(uint citizenId, TransferManager.TransferReason reason)
        {
            TransferMgr.AddOutgoingOfferFromCurrentPosition(citizenId, reason);
        }

        /// <summary>
        /// Gets a string that describes the provided citizen.
        /// </summary>
        ///
        /// <param name="citizenId">The citizen ID.</param>
        /// <param name="citizen">The citizen data reference.</param>
        ///
        /// <returns>A short string describing the provided citizen.</returns>
        protected string GetCitizenDesc(uint citizenId, ref TCitizen citizen)
        {
            ushort homeBuilding = CitizenProxy.GetHomeBuilding(ref citizen);
            string home = homeBuilding == 0 ? "homeless" : "lives at " + homeBuilding;
            ushort workBuilding = CitizenProxy.GetWorkBuilding(ref citizen);
            string employment = workBuilding == 0 ? "unemployed" : "works at " + workBuilding;
            Citizen.Location location = CitizenProxy.GetLocation(ref citizen);
            return $"Citizen {citizenId} ({CitizenProxy.GetAge(ref citizen)}, {home}, {employment}, currently {location} at {CitizenProxy.GetCurrentBuilding(ref citizen)}) / instance {CitizenProxy.GetInstance(ref citizen)}";
        }

        /// <summary>Determines whether the specified citizen must be processed as a virtual citizen.</summary>
        /// <typeparam name="TAI">The type of the citizen's AI.</typeparam>
        /// <param name="humanAI">The citizen AI reference.</param>
        /// <param name="citizen">The citizen to check.</param>
        /// <param name="realizeCitizen">A callback to determine whether a virtual citizen should be realized.</param>
        /// <returns><c>true</c> if the citizen must be processed as a virtual citizen; otherwise, <c>false</c>.</returns>
        protected bool IsCitizenVirtual<TAI>(TAI humanAI, ref TCitizen citizen, Func<TAI, bool> realizeCitizen)
        {
            if (CitizenProxy.GetInstance(ref citizen) != 0)
            {
                return false;
            }

            uint virtualChance;
            switch (Config.VirtualCitizens)
            {
                case VirtualCitizensLevel.None:
                    return false;

                case VirtualCitizensLevel.Few:
                    virtualChance = FewVirtualCitizensChance;
                    break;

                case VirtualCitizensLevel.Vanilla:
                    return !realizeCitizen(humanAI);

                case VirtualCitizensLevel.Many:
                    virtualChance = ManyVirtualCitizensChance;
                    break;

                default:
                    return false;
            }

            return CitizenMgr.GetInstancesCount() * 100 / CitizenInstancesMaxCount < virtualChance
                ? !realizeCitizen(humanAI)
                : Random.ShouldOccur(virtualChance);
        }

        /// <summary>Determines whether the weather is currently so bad that the citizen would like to stay inside a building.</summary>
        /// <returns>
        ///   <c>true</c> if the weather is bad; otherwise, <c>false</c>.</returns>
        protected bool IsBadWeather()
        {
            if (WeatherInfo.IsDisasterHazardActive)
            {
                return true;
            }

            return WeatherInfo.StayInsideChance != 0 && Random.ShouldOccur(WeatherInfo.StayInsideChance);
        }

        /// <summary>Gets an estimated travel time (in hours) between two specified buildings.</summary>
        /// <param name="building1">The ID of the first building.</param>
        /// <param name="building2">The ID of the second building.</param>
        /// <returns>An estimated travel time in hours.</returns>
        protected float GetEstimatedTravelTime(ushort building1, ushort building2)
        {
            if (building1 == 0 || building2 == 0)
            {
                return 0;
            }

            float distance = BuildingMgr.GetDistanceBetweenBuildings(building1, building2);
            return RealTimeMath.Clamp(distance / OnTheWayDistancePerHour, MinTravelTime, MaxTravelTime);
        }

        private bool CanAttendEvent(uint citizenId, ref TCitizen citizen, ICityEvent cityEvent)
        {
            Citizen.AgeGroup age = CitizenProxy.GetAge(ref citizen);
            Citizen.Gender gender = CitizenProxy.GetGender(citizenId);
            Citizen.Education education = CitizenProxy.GetEducationLevel(ref citizen);
            Citizen.Wealth wealth = CitizenProxy.GetWealthLevel(ref citizen);
            Citizen.Wellbeing wellbeing = CitizenProxy.GetWellbeingLevel(ref citizen);
            Citizen.Happiness happiness = CitizenProxy.GetHappinessLevel(ref citizen);

            return cityEvent.TryAcceptAttendee(age, gender, education, wealth, wellbeing, happiness, Random);
        }
    }
}
