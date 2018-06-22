// <copyright file="Event.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.Events.Storage
{
    using System.Collections.Generic;
    using System.Xml.Serialization;

    public sealed class Event
    {
        [XmlAttribute("EventName")]
        public string Name { get; set; } = string.Empty;

        [XmlAttribute("BuildingName")]
        public string BuildingClassName { get; set; } = string.Empty;

        [XmlAttribute("UserEventName")]
        public string UserEventName { get; set; } = string.Empty;

        [XmlAttribute("Capacity")]
        public int Capacity { get; set; } = 1000;

        [XmlAttribute("LengthInHours")]
        public double Length { get; set; } = 1.5;

        [XmlAttribute("SupportsRandomEvents")]
        public bool SupportsRandomEvents { get; set; } = true;

        [XmlAttribute("SupportsUserEvents")]
        public bool SupportUserEvents { get; set; }

        [XmlAttribute("CanBeWatchedOnTV")]
        public bool CanBeWatchedOnTV { get; set; }

        [XmlElement("ChanceOfAttendingPercentage", IsNullable = false)]
        public Attendees Attendees { get; set; } = new Attendees();

        [XmlElement("Costs", IsNullable = false)]
        public Costs Costs { get; set; } = new Costs();

        [XmlArray("Incentives", IsNullable = false)]
        [XmlArrayItem("Incentive")]
        public List<Incentive> Incentives { get; set; } = new List<Incentive>();
    }
}
