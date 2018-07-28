// <copyright file="ConfigurationProvider.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.Config
{
    using System;
    using System.IO;
    using System.Xml.Serialization;
    using RealTime.Core;
    using RealTime.Tools;

    /// <summary>
    /// A static class that loads and stores the <see cref="RealTimeConfig"/> objects.
    /// An XML file 'RealTime.xml' in the default (game) directory is used as a storage.
    /// </summary>
    internal sealed class ConfigurationProvider : IStorageData
    {
        private const string StorageId = "RealTimeConfiguration";
        private static readonly string SettingsFileName = typeof(ConfigurationProvider).Assembly.GetName().Name + ".xml";

        /// <summary>Occurs when the <see cref="Configuration"/> instance changes.</summary>
        public event EventHandler Changed;

        /// <summary>Gets the current configuration.</summary>
        public RealTimeConfig Configuration { get; private set; }

        /// <summary>Gets a value indicating whether the <see cref="Configuration"/> instance is a default configuration.</summary>
        public bool IsDefault { get; private set; }

        /// <summary>Gets an unique ID of this storage data set.</summary>
        string IStorageData.StorageDataId => StorageId;

        /// <summary>
        /// Loads the default configuration object from the serialized storage. If no storage is available,
        /// returns a new <see cref="RealTimeConfig"/> object with its values set to defaults.
        /// </summary>
        public void LoadDefaultConfiguration()
        {
            try
            {
                if (File.Exists(SettingsFileName))
                {
                    using (var stream = new FileStream(SettingsFileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        Configuration = Deserialize(stream);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error("The 'Real Time' mod cannot load its default configuration, error message: " + ex);
            }

            IsDefault = true;
            if (Configuration == null)
            {
                Configuration = new RealTimeConfig(true);
            }

            OnChanged();
        }

        /// <summary>
        /// Stores the current configuration object to the storage as a default configuration.
        /// </summary>
        public void SaveDefaultConfiguration()
        {
            if (Configuration == null)
            {
                Configuration = new RealTimeConfig(true);
            }

            try
            {
                using (var stream = new FileStream(SettingsFileName, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
                {
                    Serialize(Configuration, stream);
                }
            }
            catch (Exception ex)
            {
                Log.Error("The 'Real Time' mod cannot save its default configuration, error message: " + ex);
            }
        }

        /// <summary>Reads the data set from the specified <see cref="Stream" />.</summary>
        /// <param name="source">A <see cref="Stream" /> to read the data set from.</param>
        void IStorageData.ReadData(Stream source)
        {
            Configuration = Deserialize(source);
            if (Configuration == null)
            {
                LoadDefaultConfiguration();
            }
            else
            {
                OnChanged();
            }

            IsDefault = false;
        }

        /// <summary>Stores the data set to the specified <see cref="Stream" />.</summary>
        /// <param name="target">A <see cref="Stream" /> to write the data set to.</param>
        void IStorageData.StoreData(Stream target)
        {
            if (Configuration != null)
            {
                Serialize(Configuration, target);
            }
        }

        private static RealTimeConfig Deserialize(Stream source)
        {
            try
            {
                var serializer = new XmlSerializer(typeof(RealTimeConfig));
                using (var sr = new StreamReader(source))
                {
                    return ((RealTimeConfig)serializer.Deserialize(sr)).MigrateWhenNecessary().Validate();
                }
            }
            catch (Exception ex)
            {
                Log.Warning($"The 'Real Time' mod has encountered an error while trying to load the configuration, error message: " + ex);
                return null;
            }
        }

        private static void Serialize(RealTimeConfig config, Stream target)
        {
            try
            {
                var serializer = new XmlSerializer(typeof(RealTimeConfig), SerializationTools.IgnoreObsoleteProperties(config));
                using (var sw = new StreamWriter(target))
                {
                    serializer.Serialize(sw, config);
                }
            }
            catch (Exception ex)
            {
                Log.Error("The 'Real Time' mod cannot save its configuration, error message: " + ex);
            }
        }

        private void OnChanged()
        {
            Changed?.Invoke(this, EventArgs.Empty);
        }
    }
}
