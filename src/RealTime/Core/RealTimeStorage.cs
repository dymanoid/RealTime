// <copyright file="RealTimeStorage.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.Core
{
    using System;
    using System.IO;
    using ICities;
    using RealTime.Tools;

    public sealed class RealTimeStorage : SerializableDataExtensionBase
    {
        private static readonly string StorageDataPrefix = typeof(RealTimeStorage).Assembly.GetName().Name + ".";

        internal event EventHandler GameSaving;

        /// <summary>
        /// Gets an instance of the <see cref="RealTimeStorage"/> class that is used with the current game level.
        /// This is not a singleton instance, and is allowed to be null.
        /// </summary>
        internal static RealTimeStorage CurrentLevelStorage { get; private set; }

        public override void OnSaveData()
        {
            GameSaving?.Invoke(this, EventArgs.Empty);
        }

        public override void OnCreated(ISerializableData serializableData)
        {
            base.OnCreated(serializableData);
            CurrentLevelStorage = this;
        }

        public override void OnReleased()
        {
            base.OnReleased();
            CurrentLevelStorage = null;
        }

        internal void Serialize(IStorageData data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            string dataKey = StorageDataPrefix + data.StorageDataId;

            try
            {
                using (var stream = new MemoryStream())
                {
                    data.StoreData(stream);
                    serializableDataManager.SaveData(dataKey, stream.ToArray());
                }
            }
            catch (Exception ex)
            {
                Log.Error($"The 'Real Time' mod failed to save its data (key {dataKey}), error message: {ex}");
            }
        }

        internal void Deserialize(IStorageData data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            string dataKey = StorageDataPrefix + data.StorageDataId;

            try
            {
                byte[] rawData = serializableDataManager.LoadData(dataKey);
                if (rawData == null)
                {
                    return;
                }

                using (var stream = new MemoryStream(rawData))
                {
                    data.ReadData(stream);
                }
            }
            catch (Exception ex)
            {
                Log.Error($"The 'Real Time' mod failed to load its data (key {dataKey}), error message: {ex}");
            }
        }
    }
}
