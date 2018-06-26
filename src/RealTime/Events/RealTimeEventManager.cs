// <copyright file="RealTimeEventManager.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.Events
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Xml.Serialization;
    using RealTime.Config;
    using RealTime.Core;
    using RealTime.Events.Storage;
    using RealTime.GameConnection;
    using RealTime.Simulation;
    using RealTime.Tools;

    internal sealed class RealTimeEventManager : IStorageData
    {
        private const int MaximumEventsCount = 5;
        private const string StorageDataId = "RealTimeEvents";
        private const uint EventIntervalVariance = 48u;

        private static readonly TimeSpan MinimumIntervalBetweenEvents = TimeSpan.FromHours(3);
        private static readonly TimeSpan EventStartTimeGranularity = TimeSpan.FromMinutes(30);
        private static readonly TimeSpan EventProcessInterval = TimeSpan.FromMinutes(15);

        private static readonly ItemClass.Service[] EventBuildingServices = new[] { ItemClass.Service.Monument, ItemClass.Service.Beautification };

        private readonly LinkedList<ICityEvent> upcomingEvents;
        private readonly RealTimeConfig config;
        private readonly ICityEventsProvider eventProvider;
        private readonly IEventManagerConnection eventManager;
        private readonly IBuildingManagerConnection buildingManager;
        private readonly ISimulationManagerConnection simulationManager;
        private readonly ITimeInfo timeInfo;

        private ICityEvent lastActiveEvent;
        private ICityEvent activeEvent;
        private DateTime lastProcessed;
        private DateTime earliestEvent;

        public RealTimeEventManager(
            RealTimeConfig config,
            ICityEventsProvider eventProvider,
            IEventManagerConnection eventManager,
            IBuildingManagerConnection buildingManager,
            ISimulationManagerConnection simulationManager,
            ITimeInfo timeInfo)
        {
            this.config = config ?? throw new ArgumentNullException(nameof(config));
            this.eventProvider = eventProvider ?? throw new ArgumentNullException(nameof(eventProvider));
            this.eventManager = eventManager ?? throw new ArgumentNullException(nameof(eventManager));
            this.buildingManager = buildingManager ?? throw new ArgumentNullException(nameof(buildingManager));
            this.simulationManager = simulationManager ?? throw new ArgumentNullException(nameof(simulationManager));
            this.timeInfo = timeInfo ?? throw new ArgumentNullException(nameof(timeInfo));
            upcomingEvents = new LinkedList<ICityEvent>();
        }

        public event EventHandler EventsChanged;

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

        string IStorageData.StorageDataId => StorageDataId;

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
                else if ((vanillaEventState & EventData.Flags.Active) != 0)
                {
                    return CityEventState.OnGoing;
                }
                else if (vanillaEventState != EventData.Flags.None)
                {
                    return CityEventState.Finished;
                }
            }

            if (activeEvent != null && activeEvent.BuildingId == buildingId)
            {
                return CityEventState.OnGoing;
            }
            else if (lastActiveEvent != null && lastActiveEvent.BuildingId == buildingId)
            {
                return CityEventState.Finished;
            }

            if (upcomingEvents.FirstOrDefaultNode(e => e.BuildingId == buildingId && e.StartTime <= latestStart) != null)
            {
                return CityEventState.Upcoming;
            }

            return CityEventState.None;
        }

        public ICityEvent GetUpcomingCityEvent(DateTime earliestStartTime, DateTime latestStartTime)
        {
            if (upcomingEvents.Count == 0)
            {
                return null;
            }

            ICityEvent upcomingEvent = upcomingEvents.First.Value;
            return upcomingEvent.StartTime >= earliestStartTime && upcomingEvent.StartTime <= latestStartTime
                ? upcomingEvent
                : null;
        }

        public void ProcessEvents()
        {
            if ((timeInfo.Now - lastProcessed) < EventProcessInterval)
            {
                return;
            }

            lastProcessed = timeInfo.Now;

            Update();
            if (upcomingEvents.Count >= MaximumEventsCount)
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

        void IStorageData.ReadData(Stream source)
        {
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

        void IStorageData.StoreData(Stream target)
        {
            var serializer = new XmlSerializer(typeof(RealTimeEventStorageContainer));
            var data = new RealTimeEventStorageContainer();

            data.EarliestEvent = earliestEvent.Ticks;

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
                Log.Debug(timeInfo.Now, $"Event finished in {activeEvent.BuildingId}, started at {activeEvent.StartTime}, end time {activeEvent.EndTime}");
                lastActiveEvent = activeEvent;
                activeEvent = null;
            }

            bool eventsChanged = false;
            foreach (ushort eventId in eventManager.GetUpcomingEvents(timeInfo.Now, timeInfo.Now.AddDays(1)))
            {
                eventManager.TryGetEventInfo(eventId, out ushort buildingId, out DateTime startTime, out float duration, out float ticketPrice);

                if (upcomingEvents.Concat(new[] { activeEvent })
                    .OfType<VanillaEvent>()
                    .Any(e => e.BuildingId == buildingId && e.StartTime == startTime))
                {
                    continue;
                }

                var newEvent = new VanillaEvent(duration, ticketPrice);
                newEvent.Configure(buildingId, buildingManager.GetBuildingName(buildingId), startTime);
                eventsChanged = true;
                Log.Debug(timeInfo.Now, $"Vanilla event registered for {newEvent.BuildingId}, start time {newEvent.StartTime}, end time {newEvent.EndTime}");

                LinkedListNode<ICityEvent> existingEvent = upcomingEvents.FirstOrDefaultNode(e => e.StartTime > startTime);
                if (existingEvent == null)
                {
                    upcomingEvents.AddLast(newEvent);
                }
                else
                {
                    upcomingEvents.AddBefore(existingEvent, newEvent);
                }
            }

            if (upcomingEvents.Count == 0)
            {
                return;
            }

            ICityEvent upcomingEvent = upcomingEvents.First.Value;
            if (upcomingEvent.StartTime <= timeInfo.Now)
            {
                activeEvent = upcomingEvent;
                upcomingEvents.RemoveFirst();
                eventsChanged = true;
                Log.Debug(timeInfo.Now, $"Event started! Building {activeEvent.BuildingId}, ends on {activeEvent.EndTime}");
            }

            if (eventsChanged)
            {
                OnEventsChanged();
            }
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

            DateTime startTime = GetRandomEventStartTime();
            if (startTime < earliestEvent)
            {
                return;
            }

            earliestEvent = startTime.AddHours(simulationManager.GetRandomizer().Int32(EventIntervalVariance));

            newEvent.Configure(buildingId, buildingManager.GetBuildingName(buildingId), startTime);
            upcomingEvents.AddLast(newEvent);
            OnEventsChanged();
            Log.Debug(timeInfo.Now, $"New event created for building {newEvent.BuildingId}, starts on {newEvent.StartTime}, ends on {newEvent.EndTime}");
        }

        private DateTime GetRandomEventStartTime()
        {
            DateTime result = upcomingEvents.Count == 0
                ? timeInfo.Now
                : upcomingEvents.Last.Value.EndTime.Add(MinimumIntervalBetweenEvents);

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

            float randomOffset = simulationManager.GetRandomizer().Int32((uint)((latestHour - earliestHour) * 60f)) / 60f;
            result = result.AddHours(randomOffset).RoundCeil(EventStartTimeGranularity);

            if (result.Hour >= latestHour)
            {
                result = result.Date.AddHours(24 + earliestHour + randomOffset).RoundCeil(EventStartTimeGranularity);
            }
            else if (result.Hour < earliestHour)
            {
                result = result.AddHours(earliestHour - result.Hour + randomOffset).RoundCeil(EventStartTimeGranularity);
            }

            return result;
        }

        private void OnEventsChanged()
        {
            EventsChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
