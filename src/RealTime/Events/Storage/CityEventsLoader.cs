// <copyright file="CityEventsLoader.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.Events.Storage
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Xml.Serialization;
    using RealTime.Tools;

    internal sealed class CityEventsLoader : IEventProvider
    {
        private const string EventsFolder = "Events";
        private const string EventFileSearchPattern = "*.xml";

        private readonly List<CityEvent> events = new List<CityEvent>();

        private CityEventsLoader()
        {
        }

        public static CityEventsLoader Istance { get; } = new CityEventsLoader();

        public void ReloadEvents(string rootPath)
        {
            events.Clear();
            string searchPath = Path.Combine(rootPath, EventsFolder);
            if (!Directory.Exists(searchPath))
            {
                return;
            }

            try
            {
                LoadEvents(Directory.GetFiles(searchPath, EventFileSearchPattern));
            }
            catch (Exception ex)
            {
                Log.Error("The 'Real Time' mod was unable to load the events, error message: " + ex.Message);
                events.Clear();
            }
        }

        IRealTimeEvent IEventProvider.GetRandomEvent(string buildingClass)
        {
            var buildingEvents = events.Where(e => e.BuildingClassName == buildingClass).ToList();
            if (buildingEvents.Count == 0)
            {
                Log.Debug("No events found for the building class " + buildingClass);
                return null;
            }

            int eventNumber = SimulationManager.instance.m_randomizer.Int32((uint)buildingEvents.Count);
            Log.Debug($"EVENTS! {buildingEvents.Count} events found for the building class {buildingClass}, choosing a random event number {eventNumber}");
            return new RealTimeEvent(buildingEvents[eventNumber]);
        }

        private void LoadEvents(IEnumerable<string> files)
        {
            var serializer = new XmlSerializer(typeof(CityEventContainer));

            foreach (string file in files)
            {
                using (var sr = new StreamReader(file))
                {
                    var container = (CityEventContainer)serializer.Deserialize(sr);
                    foreach (CityEvent @event in container.Events.Where(e => !events.Any(ev => ev.Name == e.Name)))
                    {
                        events.Add(@event);
                    }
                }
            }

            Log.Debug($"Successfully loaded {events.Count} events");
        }
    }
}
