// <copyright file="IStorageData.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.Core
{
    using System.IO;

    /// <summary>
    /// An interface describing an abstract data set that can be persisted.
    /// </summary>
    internal interface IStorageData
    {
        /// <summary>
        /// Gets an unique ID of this storage data set.
        /// </summary>
        string StorageDataId { get; }

        /// <summary>
        /// Reads the data set from the provided <see cref="Stream"/>.
        /// </summary>
        ///
        /// <exception cref="System.ArgumentNullException">Thrown when the argument is null.</exception>
        ///
        /// <param name="source">A <see cref="Stream"/> to read the data set from.</param>
        void ReadData(Stream source);

        /// <summary>
        /// Reads the data set to the provided <see cref="Stream"/>.
        /// </summary>
        ///
        /// <exception cref="System.ArgumentNullException">Thrown when the argument is null.</exception>
        ///
        /// <param name="target">A <see cref="Stream"/> to write the data set to.</param>
        void StoreData(Stream target);
    }
}
