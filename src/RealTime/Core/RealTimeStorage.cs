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

        internal static RealTimeStorage Instance { get; private set; }

        public override void OnSaveData()
        {
            GameSaving?.Invoke(this, EventArgs.Empty);
        }

        public override void OnCreated(ISerializableData serializableData)
        {
            base.OnCreated(serializableData);
            Instance = this;
        }

        public override void OnReleased()
        {
            base.OnReleased();
            Instance = null;
        }

        internal void Serialize(IStorageData data)
        {
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
            string dataKey = StorageDataPrefix + data.StorageDataId;

            try
            {
                byte[] rawData = serializableDataManager.LoadData(dataKey);
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
