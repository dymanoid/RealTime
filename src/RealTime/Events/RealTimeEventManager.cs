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
        private const int IntervalBetweenEvents = 3;
        private const float EarliestHourEventStartWeekday = 16f;
        private const float LatestHourEventStartWeekday = 20f;
        private const float EarliestHourEventStartWeekend = 8f;
        private const float LatestHourEventStartWeekend = 22f;

        private static readonly TimeSpan EventProcessInterval = TimeSpan.FromMinutes(15);

        private static readonly ItemClass.Service[] EventBuildingServices = new[] { ItemClass.Service.Monument, ItemClass.Service.Beautification };

        private readonly LinkedList<IRealTimeEvent> upcomingEvents;
        private readonly RealTimeConfig config;
        private readonly IEventProvider eventProvider;
        private readonly IEventManagerConnection eventManager;
        private readonly IBuildingManagerConnection buildingManager;
        private readonly ITimeInfo timeInfo;

        private IRealTimeEvent lastActiveEvent;
        private IRealTimeEvent activeEvent;
        private DateTime lastProcessed;

        public RealTimeEventManager(
            RealTimeConfig config,
            IEventProvider eventProvider,
            IEventManagerConnection eventManager,
            IBuildingManagerConnection buildingManager,
            ITimeInfo timeInfo)
        {
            this.config = config ?? throw new ArgumentNullException(nameof(config));
            this.eventProvider = eventProvider ?? throw new ArgumentNullException(nameof(eventProvider));
            this.eventManager = eventManager ?? throw new ArgumentNullException(nameof(eventManager));
            this.buildingManager = buildingManager ?? throw new ArgumentNullException(nameof(buildingManager));
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

            DateTime start = upcomingEvents.Count == 0 ? timeInfo.Now : upcomingEvents.Last.Value.EndTime;
            start = start.AddHours(IntervalBetweenEvents);

            float earliestHour;
            float latestHour;
            if (config.IsWeekendEnabled && timeInfo.Now.IsWeekend())
            {
                earliestHour = EarliestHourEventStartWeekend;
                latestHour = LatestHourEventStartWeekend;
            }
            else
            {
                earliestHour = EarliestHourEventStartWeekday;
                latestHour = LatestHourEventStartWeekday;
            }

            if (start.Hour >= latestHour)
            {
                start = start.Date.AddHours(24 + earliestHour);
            }
            else if (start.Hour < earliestHour)
            {
                start = start.AddHours(earliestHour - start.Hour);
            }

            newEvent.Configure(buildingId, start);
            upcomingEvents.AddLast(newEvent);
            Log.Debug(timeInfo.Now, $"New event created for building {newEvent.BuildingId}, starts on {newEvent.StartTime}, ends on {newEvent.EndTime}");
        }
    }
}
