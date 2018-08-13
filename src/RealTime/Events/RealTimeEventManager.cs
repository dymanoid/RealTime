// <copyright file="RealTimeEventManager.cs" company="dymanoid">Copyright (c) dymanoid. All rights reserved.</copyright>

namespace RealTime.Events
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Xml.Serialization;
    using RealTime.Config;
    using RealTime.Events.Storage;
    using RealTime.GameConnection;
    using RealTime.Simulation;
    using SkyTools.Storage;
    using SkyTools.Tools;

    /// <summary>The central class for the custom city events logic.</summary>
    /// <seealso cref="IStorageData"/>
    internal sealed class RealTimeEventManager : IStorageData, IRealTimeEventManager
    {
        private const int MaximumEventsCount = 5;
        private const string StorageDataId = "RealTimeEvents";
        private const uint EventIntervalVariance = 48u;

        private static readonly TimeSpan MinimumIntervalBetweenEvents = TimeSpan.FromHours(3);
        private static readonly TimeSpan EventStartTimeGranularity = TimeSpan.FromMinutes(30);
        private static readonly TimeSpan EventProcessInterval = TimeSpan.FromMinutes(15);

        private static readonly ItemClass.Service[] EventBuildingServices = { ItemClass.Service.Monument, ItemClass.Service.Beautification };

        private readonly LinkedList<ICityEvent> upcomingEvents;
        private readonly RealTimeConfig config;
        private readonly ICityEventsProvider eventProvider;
        private readonly IEventManagerConnection eventManager;
        private readonly IBuildingManagerConnection buildingManager;
        private readonly IRandomizer randomizer;
        private readonly ITimeInfo timeInfo;

        private ICityEvent lastActiveEvent;
        private ICityEvent activeEvent;
        private DateTime lastProcessed;
        private DateTime earliestEvent;

        /// <summary>Initializes a new instance of the <see cref="RealTimeEventManager"/> class.</summary>
        /// <param name="config">The configuration to run with.</param>
        /// <param name="eventProvider">The city event provider implementation.</param>
        /// <param name="eventManager">
        /// A proxy object that provides a way to call the game-specific methods of the <see cref="global::EventManager"/> class.
        /// </param>
        /// <param name="buildingManager">
        /// A proxy object that provides a way to call the game-specific methods of the <see cref="global::BuildingManager"/> class.
        /// </param>
        /// <param name="randomizer">
        /// An object that implements of the <see cref="IRandomizer"/> interface.
        /// </param>
        /// <param name="timeInfo">The time information source.</param>
        /// <exception cref="ArgumentNullException">Thrown when any argument is null.</exception>
        public RealTimeEventManager(
            RealTimeConfig config,
            ICityEventsProvider eventProvider,
            IEventManagerConnection eventManager,
            IBuildingManagerConnection buildingManager,
            IRandomizer randomizer,
            ITimeInfo timeInfo)
        {
            this.config = config ?? throw new ArgumentNullException(nameof(config));
            this.eventProvider = eventProvider ?? throw new ArgumentNullException(nameof(eventProvider));
            this.eventManager = eventManager ?? throw new ArgumentNullException(nameof(eventManager));
            this.buildingManager = buildingManager ?? throw new ArgumentNullException(nameof(buildingManager));
            this.randomizer = randomizer ?? throw new ArgumentNullException(nameof(randomizer));
            this.timeInfo = timeInfo ?? throw new ArgumentNullException(nameof(timeInfo));
            upcomingEvents = new LinkedList<ICityEvent>();
        }

        /// <summary>Occurs when currently preparing, ready, ongoing, or recently finished events change.</summary>
        public event EventHandler EventsChanged;

        /// <summary>Gets the currently preparing, ready, ongoing, or recently finished city events.</summary>
        public IEnumerable<ICityEvent> CityEvents
        {
            get
            {
                if (lastActiveEvent != null)
                {
                    yield return lastActiveEvent;
                }

                if (activeEvent != null)
                {
                    yield return activeEvent;
                }

                foreach (ICityEvent upcomingEvent in upcomingEvents)
                {
                    yield return upcomingEvent;
                }
            }
        }

        /// <summary>Gets an unique ID of this storage data set.</summary>
        string IStorageData.StorageDataId => StorageDataId;

        /// <summary>Gets the state of a city event in the specified building.</summary>
        /// <param name="buildingId">The building ID to check events in.</param>
        /// <param name="latestStart">The latest start time of events to consider.</param>
        /// <returns>
        /// The state of an event that meets the specified criteria, or <see cref="CityEventState.None"/> if none found.
        /// </returns>
        public CityEventState GetEventState(ushort buildingId, DateTime latestStart)
        {
            if (buildingId == 0)
            {
                return CityEventState.None;
            }

            ushort eventId = buildingManager.GetEvent(buildingId);
            if (eventId != 0)
            {
                EventData.Flags vanillaEventState = eventManager.GetEventFlags(eventId);
                if ((vanillaEventState & (EventData.Flags.Preparing | EventData.Flags.Ready)) != 0)
                {
                    if (eventManager.TryGetEventInfo(eventId, out _, out DateTime startTime, out _, out _) && startTime <= latestStart)
                    {
                        return CityEventState.Upcoming;
                    }

                    return CityEventState.None;
                }

                if ((vanillaEventState & EventData.Flags.Active) != 0)
                {
                    return CityEventState.Ongoing;
                }

                if (vanillaEventState != EventData.Flags.None)
                {
                    return CityEventState.Finished;
                }
            }

            if (activeEvent != null && activeEvent.BuildingId == buildingId)
            {
                return CityEventState.Ongoing;
            }

            if (lastActiveEvent != null && lastActiveEvent.BuildingId == buildingId)
            {
                return CityEventState.Finished;
            }

            if (upcomingEvents.FirstOrDefaultNode(e => e.BuildingId == buildingId && e.StartTime <= latestStart) != null)
            {
                return CityEventState.Upcoming;
            }

            return CityEventState.None;
        }

        /// <summary>
        /// Gets the <see cref="ICityEvent"/> instance of an upcoming city event whose start time is between the specified values.
        /// </summary>
        /// <param name="earliestStartTime">The earliest city event start time to consider.</param>
        /// <param name="latestStartTime">The latest city event start time to consider.</param>
        /// <returns>An <see cref="ICityEvent"/> instance of the first matching city event, or null if none found.</returns>
        public ICityEvent GetUpcomingCityEvent(DateTime earliestStartTime, DateTime latestStartTime)
        {
            if (activeEvent != null && activeEvent.EndTime > latestStartTime)
            {
                return activeEvent;
            }

            if (upcomingEvents.Count == 0)
            {
                return null;
            }

            ICityEvent upcomingEvent = upcomingEvents.First.Value;
            return upcomingEvent.StartTime >= earliestStartTime && upcomingEvent.StartTime <= latestStartTime
                ? upcomingEvent
                : null;
        }

        /// <summary>
        /// Gets the <see cref="ICityEvent"/> instance of an ongoing or upcoming city event that takes place in a building
        /// with specified ID.
        /// </summary>
        /// <param name="buildingId">The ID of a building to search events for.</param>
        /// <returns>An <see cref="ICityEvent"/> instance of the first matching city event, or null if none found.</returns>
        public ICityEvent GetCityEvent(ushort buildingId)
        {
            if (buildingId == 0)
            {
                return null;
            }

            if (activeEvent != null && activeEvent.BuildingId == buildingId)
            {
                return activeEvent;
            }

            if (upcomingEvents.Count == 0)
            {
                return null;
            }

            LinkedListNode<ICityEvent> upcomingEvent = upcomingEvents.First;
            while (upcomingEvent != null)
            {
                if (upcomingEvent.Value.BuildingId == buildingId)
                {
                    return upcomingEvent.Value;
                }

                upcomingEvent = upcomingEvent.Next;
            }

            return null;
        }

        /// <summary>
        /// Processes the city events simulation step. The method can be called frequently, but the processing occurs periodically
        /// at an interval specified by <see cref="EventProcessInterval"/>.
        /// </summary>
        public void ProcessEvents()
        {
            if (RemoveCanceledEvents())
            {
                OnEventsChanged();
            }

            if ((timeInfo.Now - lastProcessed) < EventProcessInterval)
            {
                return;
            }

            lastProcessed = timeInfo.Now;

            Update();
            if (upcomingEvents.Count >= MaximumEventsCount || !config.AreEventsEnabled)
            {
                return;
            }

            ushort building = buildingManager.GetRandomBuilding(EventBuildingServices);
            if (!buildingManager.BuildingHasFlags(building, Building.Flags.Active))
            {
                return;
            }

            CreateRandomEvent(building);
        }

        /// <summary>Reads the data set from the specified <see cref="Stream"/>.</summary>
        /// <param name="source">A <see cref="Stream"/> to read the data set from.</param>
        void IStorageData.ReadData(Stream source)
        {
            lastActiveEvent = null;
            activeEvent = null;
            upcomingEvents.Clear();

            var serializer = new XmlSerializer(typeof(RealTimeEventStorageContainer));
            var data = (RealTimeEventStorageContainer)serializer.Deserialize(source);

            earliestEvent = new DateTime(data.EarliestEvent);

            foreach (RealTimeEventStorage storedEvent in data.Events)
            {
                if (string.IsNullOrEmpty(storedEvent.EventName) || string.IsNullOrEmpty(storedEvent.BuildingClassName))
                {
                    continue;
                }

                CityEventTemplate template = eventProvider.GetEventTemplate(storedEvent.EventName, storedEvent.BuildingClassName);
                var realTimeEvent = new RealTimeCityEvent(template, storedEvent.AttendeesCount);
                realTimeEvent.Configure(storedEvent.BuildingId, storedEvent.BuildingName, new DateTime(storedEvent.StartTime));

                if (realTimeEvent.EndTime < timeInfo.Now)
                {
                    lastActiveEvent = realTimeEvent;
                }
                else
                {
                    upcomingEvents.AddLast(realTimeEvent);
                }
            }

            OnEventsChanged();
        }

        /// <summary>Reads the data set to the specified <see cref="Stream"/>.</summary>
        /// <param name="target">A <see cref="Stream"/> to write the data set to.</param>
        void IStorageData.StoreData(Stream target)
        {
            var serializer = new XmlSerializer(typeof(RealTimeEventStorageContainer));
            var data = new RealTimeEventStorageContainer
            {
                EarliestEvent = earliestEvent.Ticks,
            };

            AddEventToStorage(lastActiveEvent);
            AddEventToStorage(activeEvent);
            foreach (ICityEvent cityEvent in upcomingEvents)
            {
                AddEventToStorage(cityEvent);
            }

            serializer.Serialize(target, data);

            void AddEventToStorage(ICityEvent cityEvent)
            {
                if (cityEvent != null && cityEvent is RealTimeCityEvent realTimeEvent)
                {
                    data.Events.Add(realTimeEvent.GetStorageData());
                }
            }
        }

        private void Update()
        {
            if (activeEvent != null && activeEvent.EndTime <= timeInfo.Now)
            {
                Log.Debug(LogCategory.Events, timeInfo.Now, $"Event finished in {activeEvent.BuildingId}, started at {activeEvent.StartTime}, end time {activeEvent.EndTime}");
                lastActiveEvent = activeEvent;
                activeEvent = null;
            }

            bool eventsChanged = SynchronizeWithVanillaEvents();

            if (upcomingEvents.Count != 0)
            {
                LinkedListNode<ICityEvent> upcomingEvent = upcomingEvents.First;
                while (upcomingEvent != null && upcomingEvent.Value.StartTime <= timeInfo.Now)
                {
                    if (activeEvent != null)
                    {
                        lastActiveEvent = activeEvent;
                    }

                    activeEvent = upcomingEvent.Value;
                    upcomingEvents.RemoveFirst();
                    eventsChanged = true;
                    upcomingEvent = upcomingEvent.Next;
                    Log.Debug(LogCategory.Events, timeInfo.Now, $"Event started! Building {activeEvent.BuildingId}, ends on {activeEvent.EndTime}");
                }
            }

            if (eventsChanged)
            {
                OnEventsChanged();
            }
        }

        private bool SynchronizeWithVanillaEvents()
        {
            bool result = false;

            DateTime today = timeInfo.Now.Date;
            foreach (ushort eventId in eventManager.GetUpcomingEvents(today, today.AddDays(1)))
            {
                if (!eventManager.TryGetEventInfo(eventId, out ushort buildingId, out DateTime startTime, out float duration, out float ticketPrice))
                {
                    continue;
                }

                if (startTime.AddHours(duration) < timeInfo.Now)
                {
                    continue;
                }

                VanillaEvent existingVanillaEvent = CityEvents
                    .OfType<VanillaEvent>()
                    .FirstOrDefault(e => e.BuildingId == buildingId && e.EventId == eventId);

                if (existingVanillaEvent != null)
                {
                    if (Math.Abs((startTime - existingVanillaEvent.StartTime).TotalMinutes) <= 5d)
                    {
                        continue;
                    }
                    else if (existingVanillaEvent == activeEvent)
                    {
                        activeEvent = null;
                    }
                    else
                    {
                        upcomingEvents.Remove(existingVanillaEvent);
                    }
                }

                DateTime adjustedStartTime = AdjustEventStartTime(startTime, false);
                if (adjustedStartTime != startTime)
                {
                    startTime = adjustedStartTime;
                    eventManager.SetStartTime(eventId, startTime);
                }

                var newEvent = new VanillaEvent(eventId, duration, ticketPrice);
                newEvent.Configure(buildingId, buildingManager.GetBuildingName(buildingId), startTime);
                result = true;
                Log.Debug(LogCategory.Events, timeInfo.Now, $"Vanilla event registered for {newEvent.BuildingId}, start time {newEvent.StartTime}, end time {newEvent.EndTime}");

                LinkedListNode<ICityEvent> existingEvent = upcomingEvents.FirstOrDefaultNode(e => e.StartTime >= startTime);
                if (existingEvent == null)
                {
                    upcomingEvents.AddLast(newEvent);
                }
                else
                {
                    upcomingEvents.AddBefore(existingEvent, newEvent);
                    if (existingEvent.Value.StartTime < newEvent.EndTime && existingEvent.Value is RealTimeCityEvent)
                    {
                        // Avoid multiple events at the same time - vanilla events have priority
                        upcomingEvents.Remove(existingEvent);
                        earliestEvent = newEvent.EndTime.AddHours(12f);
                    }
                }
            }

            return result;
        }

        private bool RemoveCanceledEvents()
        {
            if (lastActiveEvent != null && MustCancelEvent(lastActiveEvent))
            {
                lastActiveEvent = null;
            }

            bool eventsChanged = false;
            if (activeEvent != null && MustCancelEvent(activeEvent))
            {
                Log.Debug(LogCategory.Events, $"The active event in building {activeEvent.BuildingId} must be canceled");
                activeEvent = null;
                eventsChanged = true;
            }

            if (upcomingEvents.Count == 0)
            {
                return eventsChanged;
            }

            LinkedListNode<ICityEvent> cityEvent = upcomingEvents.First;
            while (cityEvent != null)
            {
                if (MustCancelEvent(cityEvent.Value))
                {
                    Log.Debug(LogCategory.Events, $"The upcoming event in building {cityEvent.Value.BuildingId} must be canceled");
                    eventsChanged = true;
                    LinkedListNode<ICityEvent> nextEvent = cityEvent.Next;
                    upcomingEvents.Remove(cityEvent);
                    cityEvent = nextEvent;
                }
                else
                {
                    cityEvent = cityEvent.Next;
                }
            }

            return eventsChanged;
        }

        private bool MustCancelEvent(ICityEvent cityEvent)
        {
            if (!config.AreEventsEnabled && cityEvent is RealTimeCityEvent)
            {
                return true;
            }

            const Building.Flags flags = Building.Flags.Abandoned | Building.Flags.BurnedDown | Building.Flags.Collapsed
                | Building.Flags.Deleted | Building.Flags.Demolishing | Building.Flags.Evacuating | Building.Flags.Flooded;

            if (buildingManager.BuildingHasFlags(cityEvent.BuildingId, flags, true))
            {
                return true;
            }

            if (cityEvent is VanillaEvent vanillaEvent)
            {
                EventData.Flags eventFlags = eventManager.GetEventFlags(vanillaEvent.EventId);
                return eventFlags == 0 || (eventFlags & (EventData.Flags.Cancelled | EventData.Flags.Deleted)) != 0;
            }

            return false;
        }

        private void CreateRandomEvent(ushort buildingId)
        {
            string buildingClass = buildingManager.GetBuildingClassName(buildingId);
            if (string.IsNullOrEmpty(buildingClass))
            {
                return;
            }

            ICityEvent newEvent = eventProvider.GetRandomEvent(buildingClass);
            if (newEvent == null)
            {
                return;
            }

            DateTime startTime = upcomingEvents.Count == 0
                ? timeInfo.Now
                : upcomingEvents.Last.Value.EndTime.Add(MinimumIntervalBetweenEvents);

            startTime = AdjustEventStartTime(startTime, true);
            if (startTime < earliestEvent)
            {
                return;
            }

            earliestEvent = startTime.AddHours(randomizer.GetRandomValue(EventIntervalVariance));

            newEvent.Configure(buildingId, buildingManager.GetBuildingName(buildingId), startTime);
            upcomingEvents.AddLast(newEvent);
            OnEventsChanged();
            Log.Debug(LogCategory.Events, timeInfo.Now, $"New event created for building {newEvent.BuildingId}, starts on {newEvent.StartTime}, ends on {newEvent.EndTime}");
        }

        private DateTime AdjustEventStartTime(DateTime eventStartTime, bool randomize)
        {
            DateTime result = eventStartTime;

            float earliestHour;
            float latestHour;
            if (config.IsWeekendEnabled && result.IsWeekend())
            {
                earliestHour = config.EarliestHourEventStartWeekend;
                latestHour = config.LatestHourEventStartWeekend;
            }
            else
            {
                earliestHour = config.EarliestHourEventStartWeekday;
                latestHour = config.LatestHourEventStartWeekday;
            }

            float randomOffset = randomize
                ? randomizer.GetRandomValue((uint)((latestHour - earliestHour) * 60f)) / 60f
                : 0;

            result = result.AddHours(randomOffset).RoundCeil(EventStartTimeGranularity);

            if (result.Hour >= latestHour)
            {
                return result.Date.AddHours(24 + earliestHour + randomOffset).RoundCeil(EventStartTimeGranularity);
            }

            if (result.Hour < earliestHour)
            {
                return result.AddHours(earliestHour - result.Hour + randomOffset).RoundCeil(EventStartTimeGranularity);
            }

            return result;
        }

        private void OnEventsChanged()
        {
            EventsChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}