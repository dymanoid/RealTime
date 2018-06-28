// <copyright file="RealTimeStorage.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.Core
{
    using System;
    using System.IO;
    using ICities;
    using RealTime.Tools;

    /// <summary>
    /// A game component that helps to load and store the mod's private data in a save game.
    /// </summary>
    public sealed class RealTimeStorage : SerializableDataExtensionBase
    {
        private static readonly string StorageDataPrefix = typeof(RealTimeStorage).Assembly.GetName().Name + ".";

        /// <summary>
        /// Occurs when the current game level is about to be saved to a save game.
        /// </summary>
        internal event EventHandler GameSaving;

        /// <summary>
        /// Gets an instance of the <see cref="RealTimeStorage"/> class that is used with the current game level.
        /// This is not a singleton instance, and is allowed to be null.
        /// </summary>
        internal static RealTimeStorage CurrentLevelStorage { get; private set; }

        /// <summary>
        /// Called when the level is being saved.
        /// </summary>
        public override void OnSaveData()
        {
            GameSaving?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Called when an instance of this class is being initialized by the game engine.
        /// </summary>
        ///
        /// <param name="serializableData">An instance of the <see cref="ISerializableData"/> service.</param>
        public override void OnCreated(ISerializableData serializableData)
        {
            base.OnCreated(serializableData);
            CurrentLevelStorage = this;
        }

        /// <summary>
        /// Called when this game object is released by the game engine.
        /// </summary>
        public override void OnReleased()
        {
            base.OnReleased();
            CurrentLevelStorage = null;
        }

        /// <summary>
        /// Serializes the data described by the provided <paramref name="data"/> to this level's storage.
        /// </summary>
        ///
        /// <exception cref="ArgumentNullException">Thrown when the argument is null.</exception>
        ///
        /// <param name="data">An <see cref="IStorageData"/> instance that describes the data to serialize.</param>
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

        /// <summary>
        /// Deserializes the data described by the provided <paramref name="data"/> from this level's storage.
        /// </summary>
        ///
        /// <exception cref="ArgumentNullException">Thrown when the argument is null.</exception>
        ///
        /// <param name="data">An <see cref="IStorageData"/> instance that describes the data to deserialize.</param>
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
