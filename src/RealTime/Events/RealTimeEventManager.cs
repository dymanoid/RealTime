// <copyright file="RealTimeEventManager.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.Events
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using RealTime.Config;
    using RealTime.GameConnection;
    using RealTime.Simulation;
    using RealTime.Tools;

    internal sealed class RealTimeEventManager
    {
        private const int MaximumEventsCount = 5;
        private const float EarliestHourEventStartWeekday = 16f;
        private const float LatestHourEventStartWeekday = 20f;
        private const float EarliestHourEventStartWeekend = 8f;
        private const float LatestHourEventStartWeekend = 22f;

        private static readonly TimeSpan MinimumIntervalBetweenEvents = TimeSpan.FromHours(3);
        private static readonly TimeSpan EventStartTimeGranularity = TimeSpan.FromMinutes(30);
        private static readonly TimeSpan EventProcessInterval = TimeSpan.FromMinutes(15);

        private static readonly ItemClass.Service[] EventBuildingServices = new[] { ItemClass.Service.Monument, ItemClass.Service.Beautification };

        private readonly LinkedList<IRealTimeEvent> upcomingEvents;
        private readonly RealTimeConfig config;
        private readonly IEventProvider eventProvider;
        private readonly IEventManagerConnection eventManager;
        private readonly IBuildingManagerConnection buildingManager;
        private readonly ISimulationManagerConnection simulationManager;
        private readonly ITimeInfo timeInfo;

        private IRealTimeEvent lastActiveEvent;
        private IRealTimeEvent activeEvent;
        private DateTime lastProcessed;

        public RealTimeEventManager(
            RealTimeConfig config,
            IEventProvider eventProvider,
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
            upcomingEvents = new LinkedList<IRealTimeEvent>();
        }

        public EventState GetEventState(ushort buildingId, DateTime latestStart)
        {
            if (buildingId == 0)
            {
                return EventState.None;
            }

            ushort eventId = buildingManager.GetEvent(buildingId);
            if (eventId != 0)
            {
                EventData.Flags vanillaEventState = eventManager.GetEventFlags(eventId);
                if ((vanillaEventState & (EventData.Flags.Preparing | EventData.Flags.Ready)) != 0)
                {
                    if (eventManager.TryGetEventInfo(eventId, out _, out DateTime startTime, out _) && startTime <= latestStart)
                    {
                        return EventState.Upcoming;
                    }

                    return EventState.None;
                }
                else if ((vanillaEventState & EventData.Flags.Active) != 0)
                {
                    return EventState.OnGoing;
                }
                else if (vanillaEventState != EventData.Flags.None)
                {
                    return EventState.Finished;
                }
            }

            if (activeEvent != null && activeEvent.BuildingId == buildingId)
            {
                return EventState.OnGoing;
            }
            else if (lastActiveEvent != null && lastActiveEvent.BuildingId == buildingId)
            {
                return EventState.Finished;
            }

            if (upcomingEvents.FirstOrDefaultNode(e => e.BuildingId == buildingId && e.StartTime <= latestStart) != null)
            {
                return EventState.Upcoming;
            }

            return EventState.None;
        }

        public bool TryAttendEvent(DateTime earliestStartTime, DateTime latestStartTime, out ushort buildingId)
        {
            buildingId = default;

            if (upcomingEvents.Count == 0)
            {
                return false;
            }

            IRealTimeEvent upcomingEvent = upcomingEvents.First.Value;
            if (upcomingEvent.StartTime >= earliestStartTime && upcomingEvent.StartTime <= latestStartTime && upcomingEvent.CanAttend())
            {
                upcomingEvent.Attend();
                buildingId = upcomingEvent.BuildingId;
                return true;
            }

            return false;
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
            if ((buildingManager.GetBuildingFlags(building) & Building.Flags.Active) == 0)
            {
                return;
            }

            CreateRandomEvent(building);
        }

        private void Update()
        {
            if (activeEvent != null && activeEvent.EndTime <= timeInfo.Now)
            {
                Log.Debug(timeInfo.Now, $"Event finished in {activeEvent.BuildingId}, started at {activeEvent.StartTime}, end time {activeEvent.EndTime}");
                lastActiveEvent = activeEvent;
                activeEvent = null;
            }

            foreach (ushort eventId in eventManager.GetUpcomingEvents(timeInfo.Now.AddDays(1)))
            {
                eventManager.TryGetEventInfo(eventId, out ushort buildingId, out DateTime startTime, out float duration);
                if (upcomingEvents
                    .OfType<VanillaEvent>()
                    .Where(e => e.EventId == eventId && e.BuildingId == buildingId)
                    .Any())
                {
                    continue;
                }

                var newEvent = new VanillaEvent(duration);
                newEvent.Configure(buildingId, startTime);
                Log.Debug(timeInfo.Now, $"Vanilla event registered for {newEvent.BuildingId}, start time {newEvent.StartTime}, end time {newEvent.EndTime}");

                LinkedListNode<IRealTimeEvent> existingEvent = upcomingEvents.FirstOrDefaultNode(e => e.StartTime > startTime);
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

            IRealTimeEvent upcomingEvent = upcomingEvents.First.Value;
            if (upcomingEvent.StartTime <= timeInfo.Now)
            {
                activeEvent = upcomingEvent;
                upcomingEvents.RemoveFirst();
                Log.Debug(timeInfo.Now, $"Event started! Building {activeEvent.BuildingId}, ends on {activeEvent.EndTime}");
            }
        }

        private void CreateRandomEvent(ushort buildingId)
        {
            string buildingClass = buildingManager.GetBuildingClassName(buildingId);
            if (string.IsNullOrEmpty(buildingClass))
            {
                return;
            }

            IRealTimeEvent newEvent = eventProvider.GetRandomEvent(buildingClass);
            if (newEvent == null)
            {
                return;
            }

            DateTime startTime = GetRandomEventStartTime();
            newEvent.Configure(buildingId, buildingManager.GetBuildingName(buildingId), startTime);
            upcomingEvents.AddLast(newEvent);
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
                earliestHour = EarliestHourEventStartWeekend;
                latestHour = LatestHourEventStartWeekend;
            }
            else
            {
                earliestHour = EarliestHourEventStartWeekday;
                latestHour = LatestHourEventStartWeekday;
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
    }
}
