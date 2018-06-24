// <copyright file="IStorageData.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.Core
{
    using System.IO;

    internal interface IStorageData
    {
        string StorageDataId { get; }

        void ReadData(Stream source);

        void StoreData(Stream target);
    }
}
