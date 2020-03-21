// <copyright file="VanillaEvents.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.Events
{
    using System;
    using System.Collections.Generic;
    using SkyTools.Tools;

    /// <summary>
    /// A class for handling the vanilla game events data.
    /// </summary>
    internal sealed class VanillaEvents
    {
        private readonly Dictionary<EventAI, EventAIData> eventData = new Dictionary<EventAI, EventAIData>();

        private VanillaEvents()
        {
        }

        /// <summary>Customizes the vanilla events in the game by changing their time behavior.</summary>
        /// <returns>An instance of the <see cref="VanillaEvents"/> class that can be used for reverting the events
        /// to the original state.</returns>
        public static VanillaEvents Customize()
        {
            var result = new VanillaEvents();

            for (uint i = 0; i < PrefabCollection<EventInfo>.PrefabCount(); ++i)
            {
                var eventInfo = PrefabCollection<EventInfo>.GetPrefab(i);
                if (eventInfo?.m_eventAI?.m_info == null)
                {
                    continue;
                }

                var eventAI = eventInfo.m_eventAI;
                var originalData = new EventAIData(eventAI.m_eventDuration, eventAI.m_prepareDuration, eventAI.m_disorganizeDuration);
                result.eventData[eventAI] = originalData;
                Customize(eventAI);
                Log.Debug(LogCategory.Events, $"Customized event {eventAI.GetType().Name}");
            }

            return result;
        }

        /// <summary>Updates all vanilla game events so that their start and end times correspond to the
        /// currently active time flow speed.</summary>
        /// <param name="timeConverter">A method that converts the frame-based time to a real time.</param>
        /// <exception cref="ArgumentNullException">Thrown when the argument is null.</exception>
        public static void ProcessUpdatedTimeSpeed(Func<uint, DateTime> timeConverter)
        {
            if (timeConverter == null)
            {
                throw new ArgumentNullException(nameof(timeConverter));
            }

            for (int i = 0; i < EventManager.instance.m_events.m_size; ++i)
            {
                ref EventData eventData = ref EventManager.instance.m_events.m_buffer[i];
                if (eventData.m_startFrame == 0)
                {
                    continue;
                }

                Log.Debug(LogCategory.Events, $"Update event {i} time frames: original start {eventData.m_startFrame}, original end {eventData.m_expireFrame}");
                var originalTime = timeConverter(eventData.m_startFrame);
                eventData.m_startFrame = SimulationManager.instance.TimeToFrame(originalTime);

                originalTime = timeConverter(eventData.m_expireFrame);
                eventData.m_expireFrame = SimulationManager.instance.TimeToFrame(originalTime);
                Log.Debug(LogCategory.Events, $"Event updated: new start {eventData.m_startFrame}, new end {eventData.m_expireFrame}");
            }
        }

        /// <summary>Reverts the vanilla events parameters to the original state.</summary>
        public void Revert()
        {
            foreach (var item in eventData)
            {
                var eventAI = item.Key;
                eventAI.m_eventDuration = item.Value.EventDuration;
                eventAI.m_prepareDuration = item.Value.PrepareDuration;
                eventAI.m_disorganizeDuration = item.Value.DisorganizeDuration;
                Log.Debug(LogCategory.Events, $"Reverted event {eventAI.GetType().Name}");
            }

            eventData.Clear();
        }

        private static void Customize(EventAI eventAI)
        {
            float eventDuration;
            float prepareDuration;
            float disorganizeDuration;

            switch (eventAI.m_info.m_type)
            {
                case EventManager.EventType.Concert:
                    eventDuration = 4f;
                    prepareDuration = 4f;
                    disorganizeDuration = 2f;
                    break;

                case EventManager.EventType.VarsitySportsMatch:
                case EventManager.EventType.Football:
                    eventDuration = 2f;
                    prepareDuration = 4f;
                    disorganizeDuration = 2f;
                    break;

                case EventManager.EventType.RocketLaunch:
                    eventDuration = 4f;
                    prepareDuration = 12f;
                    disorganizeDuration = 4f;
                    break;

                case EventManager.EventType.AcademicYear:
                    eventDuration = 7f * 24f;
                    prepareDuration = 2f;
                    disorganizeDuration = 2f;
                    break;

                default:
                    return;
            }

            eventAI.m_eventDuration = eventDuration;
            eventAI.m_prepareDuration = prepareDuration;
            eventAI.m_disorganizeDuration = disorganizeDuration;
        }

        private readonly struct EventAIData
        {
            public EventAIData(float eventDuration, float prepareDuration, float disorganizeDuration)
            {
                EventDuration = eventDuration;
                PrepareDuration = prepareDuration;
                DisorganizeDuration = disorganizeDuration;
            }

            public float EventDuration { get; }

            public float PrepareDuration { get; }

            public float DisorganizeDuration { get; }
        }
    }
}
