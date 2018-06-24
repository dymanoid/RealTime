// <copyright file="CityEventTemplate.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.Events.Storage
{
    using System.Collections.Generic;
    using System.Xml.Serialization;

    public sealed class CityEventTemplate
    {
        [XmlAttribute("EventName")]
        public string EventName { get; set; } = string.Empty;

        [XmlAttribute("BuildingName")]
        public string BuildingClassName { get; set; } = string.Empty;

        [XmlAttribute("UserEventName")]
        public string UserEventName { get; set; } = string.Empty;

        [XmlAttribute("Capacity")]
        public int Capacity { get; set; } = 1000;

        [XmlAttribute("LengthInHours")]
        public double Duration { get; set; } = 1.5;

        [XmlAttribute("SupportsRandomEvents")]
        public bool SupportsRandomEvents { get; set; } = true;

        [XmlAttribute("SupportsUserEvents")]
        public bool SupportsUserEvents { get; set; }

        [XmlAttribute("CanBeWatchedOnTV")]
        public bool CanBeWatchedOnTV { get; set; }

        [XmlElement("ChanceOfAttendingPercentage", IsNullable = false)]
        public CityEventAttendees Attendees { get; set; } = new CityEventAttendees();

        [XmlElement("Costs", IsNullable = false)]
        public CityEventCosts Costs { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "XML serialization")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "XML serialization")]
        [XmlArray("Incentives", IsNullable = false)]
        [XmlArrayItem("Incentive")]
        public List<CityEventIncentive> Incentives { get; set; } = new List<CityEventIncentive>();
    }
}
