// <copyright file="CityEventTemplate.cs" company="dymanoid">
//     Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.Events.Storage
{
    using System.Collections.Generic;
    using System.Xml.Serialization;

    /// <summary>A storage class for the city event template.</summary>
    public sealed class CityEventTemplate
    {
        /// <summary>Gets or sets the unique name of the city event.</summary>
        [XmlAttribute("EventName")]
        public string EventName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the name of the building class where this city event can take place.
        /// </summary>
        [XmlAttribute("BuildingName")]
        public string BuildingClassName { get; set; } = string.Empty;

        /// <summary>Gets or sets a customizable user-provided name of the city event.</summary>
        [XmlAttribute("UserEventName")]
        public string UserEventName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the maximum event capacity. This is the maximum number of the event attendees.
        /// </summary>
        [XmlAttribute("Capacity")]
        public int Capacity { get; set; } = 1000;

        /// <summary>Gets or sets the duration of the event in hours.</summary>
        [XmlAttribute("LengthInHours")]
        public double Duration { get; set; } = 1.5;

        /// <summary>
        /// Gets or sets a value indicating whether this event template can be used for
        /// auto-generated city events.
        /// </summary>
        [XmlAttribute("SupportsRandomEvents")]
        public bool SupportsRandomEvents { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether this event template can be used for user-created
        /// custom city events.
        /// </summary>
        [XmlAttribute("SupportsUserEvents")]
        public bool SupportsUserEvents { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this city event can be watched on TV.
        /// </summary>
        [XmlAttribute("CanBeWatchedOnTV")]
        public bool CanBeWatchedOnTV { get; set; }

        /// <summary>Gets or sets the city event attendees configuration.</summary>
        [XmlElement("ChanceOfAttendingPercentage", IsNullable = false)]
        public CityEventAttendees Attendees { get; set; } = new CityEventAttendees();

        /// <summary>Gets or sets the city event costs configuration.</summary>
        [XmlElement("Costs", IsNullable = false)]
        public CityEventCosts Costs { get; set; }

        /// <summary>Gets or sets the city event incentives configuration.</summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "XML serialization")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "XML serialization")]
        [XmlArray("Incentives", IsNullable = false)]
        [XmlArrayItem("Incentive")]
        public List<CityEventIncentive> Incentives { get; set; } = new List<CityEventIncentive>();
    }
}