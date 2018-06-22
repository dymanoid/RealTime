// <copyright file="EventContainer.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.Events.Storage
{
    using System.Collections.Generic;
    using System.Xml.Serialization;

    [XmlRoot("EventContainer", IsNullable = false)]
    public sealed class EventContainer
    {
        [XmlArray("Events", IsNullable = false)]
        [XmlArrayItem("Event")]
        public List<Event> Events { get; } = new List<Event>();
    }
}
