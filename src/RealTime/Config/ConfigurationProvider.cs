// <copyright file="ConfigurationProvider.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.Config
{
    using System;
    using System.IO;
    using System.Xml.Serialization;
    using RealTime.Tools;

    /// <summary>
    /// A static class that loads and stores the <see cref="RealTimeConfig"/> objects.
    /// An XML file 'RealTime.xml' in the default (game) directory is used as a storage.
    /// </summary>
    internal static class ConfigurationProvider
    {
        private static readonly string SettingsFileName = typeof(ConfigurationProvider).Assembly.GetName().Name + ".xml";

        /// <summary>
        /// Loads the configuration object from the serialized storage. If no storage is available,
        /// returns a new <see cref="RealTimeConfig"/> object with its values set to defaults.
        /// </summary>
        ///
        /// <returns>A <see cref="RealTimeConfig"/> object containing the configuration.</returns>
        public static RealTimeConfig LoadConfiguration()
        {
            try
            {
                if (!File.Exists(SettingsFileName))
                {
                    return new RealTimeConfig();
                }

                return Deserialize();
            }
            catch (Exception ex)
            {
                Log.Warning($"The ' Real Time' mod has encountered an error while trying to load the configuration, error message: " + ex.Message);
                return new RealTimeConfig();
            }
        }

        /// <summary>
        /// Stores the provided <paramref name="config"/> object to the storage.
        /// </summary>
        ///
        /// <exception cref="ArgumentNullException">Thrown when the argument is null.</exception>
        ///
        /// <param name="config">A <see cref="RealTimeConfig"/> object to store.</param>
        public static void SaveConfiguration(RealTimeConfig config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            try
            {
                Serialize(config);
            }
            catch (Exception ex)
            {
                Log.Error("The 'Real Time' mod cannot save its configuration, error message: " + ex.Message);
            }
        }

        private static RealTimeConfig Deserialize()
        {
            var serializer = new XmlSerializer(typeof(RealTimeConfig));
            using (var sr = new StreamReader(SettingsFileName))
            {
                return (RealTimeConfig)serializer.Deserialize(sr);
            }
        }

        private static void Serialize(RealTimeConfig config)
        {
            var serializer = new XmlSerializer(typeof(RealTimeConfig));
            using (var sw = new StreamWriter(SettingsFileName))
            {
                serializer.Serialize(sw, config);
            }
        }
    }
}
