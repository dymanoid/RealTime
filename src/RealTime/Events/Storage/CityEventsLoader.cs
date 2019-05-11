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
    using ColossalFramework.Packaging;
    using SkyTools.Tools;

    /// <summary>
    /// A city event template management class that can load the city events templates from a file
    /// storage and create <see cref="RealTimeCityEvent"/> instances from templates for particular
    /// building classes.
    /// </summary>
    /// <seealso cref="ICityEventsProvider"/>
    internal sealed class CityEventsLoader : ICityEventsProvider
    {
        private const string RealTimeEventsDirectoryName = "Events";
        private const string RushHourEventsDirectoryName = "RushHour Events";
        private const string EventFileSearchPattern = "*.xml";

        private readonly List<CityEventTemplate> events = new List<CityEventTemplate>();

        private CityEventsLoader()
        {
        }

        /// <summary>Gets the one and only instance of this class.</summary>
        public static CityEventsLoader Instance { get; } = new CityEventsLoader();

        /// <summary>
        /// Reloads the event templates from the storage file that is located in a subdirectory of
        /// the specified path.
        /// </summary>
        /// <param name="modPath">The path where the mod's custom data files are stored.</param>
        /// <exception cref="ArgumentException">
        /// Thrown when the <paramref name="modPath"/> is null or an empty string.
        /// </exception>
        public void ReloadEvents(string modPath)
        {
            if (string.IsNullOrEmpty(modPath))
            {
                throw new ArgumentException("The data path cannot be null or empty string", nameof(modPath));
            }

            events.Clear();

            var loadedEvents = new HashSet<string>();
            string eventsPath = Path.Combine(modPath, RealTimeEventsDirectoryName);
            if (Directory.Exists(eventsPath))
            {
                LoadEventsFromDirectory(eventsPath, loadedEvents);
            }
            else
            {
                Log.Warning($"The 'Real Time' mod did not find any event templates, the directory '{eventsPath}' doesn't exist");
            }

            LoadRushHourEvents(loadedEvents);
            Log.Debug(LogCategory.Generic, $"Loaded {events.Count} event templates");
        }

        /// <summary>Clears the currently loaded city events templates collection.</summary>
        public void Clear() => events.Clear();

        /// <summary>
        /// Gets a randomly created city event for a building of specified class. If no city event
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
        /// Gets the event template that has the specified name and is configured for the specified
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

            return events.Find(e => e.EventName == eventName && e.BuildingClassName == buildingClassName);
        }

        private void LoadRushHourEvents(HashSet<string> loadedEvents)
        {
            IEnumerable<string> buildingPaths = PackageManager.FilterAssets(UserAssetType.CustomAssetMetaData)
                .Where(a => a.isEnabled)
                .Select(a => a.Instantiate<CustomAssetMetaData>())
                .Where(m => m?.service == ItemClass.Service.Monument && !string.IsNullOrEmpty(m.assetRef.package?.packagePath))
                .Select(m => Path.GetDirectoryName(m.assetRef.package.packagePath))
                .Distinct();

            try
            {
                foreach (string path in buildingPaths)
                {
                    string eventsPath = Path.Combine(path, RushHourEventsDirectoryName);
                    Log.Debug(LogCategory.Generic, $"Checking directory '{eventsPath}' for Rush Hour events...");
                    if (Directory.Exists(eventsPath))
                    {
                        LoadEventsFromDirectory(eventsPath, loadedEvents);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Warning($"The 'Real Time' mod could not load Rush Hour events, error message: {ex}");
            }
        }

        private void LoadEventsFromDirectory(string eventsPath, HashSet<string> loadedEvents)
        {
            try
            {
                string[] files = Directory.GetFiles(eventsPath, EventFileSearchPattern);
                LoadEvents(files, loadedEvents);
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
            {
                Log.Warning($"The 'Real Time' mod could not load event templates from {eventsPath}, error message: {ex}");
            }
        }

        private void LoadEvents(IEnumerable<string> files, HashSet<string> loadedEvents)
        {
            var serializer = new XmlSerializer(typeof(CityEventContainer));

            foreach (string file in files)
            {
                try
                {
                    using (var sr = new StreamReader(file))
                    {
                        var container = (CityEventContainer)serializer.Deserialize(sr);
                        foreach (var cityEvent in container.Templates.Where(e => loadedEvents.Add(e.EventName)))
                        {
                            events.Add(cityEvent);
                            Log.Debug(LogCategory.Generic, $"Loaded event template '{cityEvent.EventName}' for '{cityEvent.BuildingClassName}'");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Error($"The 'Real Time' mod was unable to load an event template from file '{file}', error message: {ex}");
                }
            }
        }
    }
}