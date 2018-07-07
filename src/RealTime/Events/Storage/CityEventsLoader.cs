// <copyright file="CityEventsLoader.cs" company="dymanoid">
//     Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.Events.Storage
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Xml.Serialization;
    using RealTime.Tools;

    /// <summary>
    /// A city event template management class that can load the city events templates from a file
    /// storage and create <see cref="RealTimeCityEvent"/> instances from templates for particular
    /// building classes.
    /// </summary>
    /// <seealso cref="ICityEventsProvider"/>
    internal sealed class CityEventsLoader : ICityEventsProvider
    {
        private const string EventsFolder = "Events";
        private const string EventFileSearchPattern = "*.xml";

        private readonly List<CityEventTemplate> events = new List<CityEventTemplate>();

        private CityEventsLoader()
        {
        }

        /// <summary>Gets the one and only instance of this class.</summary>
        public static CityEventsLoader Instance { get; } = new CityEventsLoader();

        /// <summary>
        /// Reloads the event templates from the storage file that is located in a subdirectory of
        /// the provided path.
        /// </summary>
        /// <param name="dataPath">The path where the mod's custom data files are stored.</param>
        /// <exception cref="ArgumentException">
        /// Thrown when the <paramref name="dataPath"/> is null or an empty string.
        /// </exception>
        public void ReloadEvents(string dataPath)
        {
            if (string.IsNullOrEmpty(dataPath))
            {
                throw new ArgumentException("The data path cannot be null or empty string", nameof(dataPath));
            }

            events.Clear();
            string searchPath = Path.Combine(dataPath, EventsFolder);
            if (!Directory.Exists(searchPath))
            {
                Log.Warning($"The 'Real Time' mod did not found any event templates, the directory '{searchPath}' doesn't exist");
                return;
            }

            LoadEvents(Directory.GetFiles(searchPath, EventFileSearchPattern));
        }

        /// <summary>Clears the currently loaded city events templates collection.</summary>
        public void Clear()
        {
            events.Clear();
        }

        /// <summary>
        /// Gets a randomly created city event for a building of provided class. If no city event
        /// could be created, returns <c>null</c>.
        /// </summary>
        /// <param name="buildingClass">The building class to create a city event for.</param>
        /// <returns>An instance of <see cref="ICityEvent"/> or null.</returns>
        /// <exception cref="ArgumentException">
        /// Thrown when the argument is null or an empty string.
        /// </exception>
        ICityEvent ICityEventsProvider.GetRandomEvent(string buildingClass)
        {
            if (string.IsNullOrEmpty(buildingClass))
            {
                throw new ArgumentException("The building class cannot be null or empty string", nameof(buildingClass));
            }

            var buildingEvents = events.Where(e => e.BuildingClassName == buildingClass).ToList();
            if (buildingEvents.Count == 0)
            {
                return null;
            }

            int eventNumber = SimulationManager.instance.m_randomizer.Int32((uint)buildingEvents.Count);
            return new RealTimeCityEvent(buildingEvents[eventNumber]);
        }

        /// <summary>
        /// Gets the event template that has the provided name and is configured for the provided
        /// building class.
        /// </summary>
        /// <param name="eventName">The unique name of the city event template.</param>
        /// <param name="buildingClassName">
        /// The name of the building class the searched template is configured for.
        /// </param>
        /// <returns>An instance of <see cref="CityEventTemplate"/> or null of none found.</returns>
        /// <exception cref="ArgumentException">
        /// Thrown when any argument is null or an empty string.
        /// </exception>
        CityEventTemplate ICityEventsProvider.GetEventTemplate(string eventName, string buildingClassName)
        {
            if (string.IsNullOrEmpty(eventName))
            {
                throw new ArgumentException("The event name cannot be null or empty string", nameof(eventName));
            }

            if (string.IsNullOrEmpty(buildingClassName))
            {
                throw new ArgumentException("The building class name cannot be null or empty string", nameof(buildingClassName));
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
                    Log.Error($"The 'Real Time' mod was unable to load an event template from file '{file}', error message: {ex}");
                }
            }

            Log.Debug($"Successfully loaded {events.Count} event templates");
        }
    }
}