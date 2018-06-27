// <copyright file="ConfigurationProvider.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.Config
{
    using System;
    using System.IO;
    using System.Xml.Serialization;
    using RealTime.Tools;

    internal static class ConfigurationProvider
    {
        private static readonly string SettingsFileName = typeof(ConfigurationProvider).Assembly.GetName().Name + ".xml";

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
            catch
            {
                return new RealTimeConfig();
            }
        }

        public static void SaveConfiguration(RealTimeConfig config)
        {
            try
            {
                Serialize(config);
            }
            catch (Exception ex)
            {
                Log.Error("Le mode 'Real Time' ne peut pas enregistrer sa configuration, message d'erreur : " + ex.Message);
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
