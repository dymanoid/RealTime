// <copyright file="RealTimeEventStorageContainer.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.Events.Storage
{
    using System.Collections.Generic;
    using System.Xml.Serialization;

    [XmlRoot("RealTimeEventStorage")]
    public sealed class RealTimeEventStorageContainer
    {
        [XmlAttribute]
        public int Version { get; set; } = 1;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "XML serialization")]
        [XmlArray("Events")]
        [XmlArrayItem("RealTimeEventStorage")]
        public List<RealTimeEventStorage> Events { get; set; } = new List<RealTimeEventStorage>();
    }
}
