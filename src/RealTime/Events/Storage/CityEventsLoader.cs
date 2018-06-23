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

    internal sealed class CityEventsLoader : ICityEventsProvider
    {
        private const string EventsFolder = "Events";
        private const string EventFileSearchPattern = "*.xml";

        private readonly List<CityEventTemplate> events = new List<CityEventTemplate>();

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
                Log.Warning($"The 'Real Time' mod did not found any event templates, the directory '{searchPath}' doesn't exist");
                return;
            }

            LoadEvents(Directory.GetFiles(searchPath, EventFileSearchPattern));
        }

        public void Clear()
        {
            events.Clear();
        }

        ICityEvent ICityEventsProvider.GetRandomEvent(string buildingClass)
        {
            if (buildingClass == null)
            {
                throw new ArgumentNullException(nameof(buildingClass));
            }

            var buildingEvents = events.Where(e => e.BuildingClassName == buildingClass).ToList();
            if (buildingEvents.Count == 0)
            {
                return null;
            }

            int eventNumber = SimulationManager.instance.m_randomizer.Int32((uint)buildingEvents.Count);
            return new RealTimeCityEvent(buildingEvents[eventNumber]);
        }

        CityEventTemplate ICityEventsProvider.GetEventTemplate(string eventName, string buildingClassName)
        {
            if (eventName == null)
            {
                throw new ArgumentNullException(nameof(eventName));
            }

            if (buildingClassName == null)
            {
                throw new ArgumentNullException(nameof(buildingClassName));
            }

            return events.FirstOrDefault(e => e.EventName == eventName && e.BuildingClassName == buildingClassName);
        }

        private void LoadEvents(IEnumerable<string> files)
        {
            var serializer = new XmlSerializer(typeof(CityEventContainer));

            foreach (string file in files)
            {
                try
                {
                    using (var sr = new StreamReader(file))
                    {
                        var container = (CityEventContainer)serializer.Deserialize(sr);
                        foreach (CityEventTemplate @event in container.Templates.Where(e => !events.Any(ev => ev.EventName == e.EventName)))
                        {
                            events.Add(@event);
                            Log.Debug($"Loaded event template '{@event.EventName}' for '{@event.BuildingClassName}'");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Error($"The 'Real Time' mod was unable to load an event template from file '{file}', error message: '{ex.Message}'");
                }
            }

            Log.Debug($"Successfully loaded {events.Count} event templates");
        }
    }
}
