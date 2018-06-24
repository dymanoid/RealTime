// <copyright file="RealTimeEventStorage.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.Events.Storage
{
    using System.Xml.Serialization;

    public sealed class RealTimeEventStorage
    {
        [XmlAttribute]
        public string EventName { get; set; }

        [XmlAttribute]
        public string BuildingClassName { get; set; }

        [XmlElement]
        public long StartTime { get; set; }

        [XmlElement]
        public ushort BuildingId { get; set; }

        [XmlElement]
        public string BuildingName { get; set; }

        [XmlElement]
        public int AttendeesCount { get; set; }
    }
}
