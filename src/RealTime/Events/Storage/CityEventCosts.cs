// <copyright file="CityEventCosts.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.Events.Storage
{
    using System.Xml.Serialization;

    /// <summary>
    /// A storage class for the city event costs settings.
    /// </summary>
    public class CityEventCosts
    {
        /// <summary>Gets or sets the cost of the event creation.</summary>
        [XmlElement("Creation")]
        public float Creation { get; set; } = 100;

        /// <summary>Gets or sets the additional cost per event attendee.</summary>
        [XmlElement("PerHead")]
        public float PerHead { get; set; } = 5;

        /// <summary>Gets or sets the costs of the advertising signs campaign.</summary>
        [XmlElement("AdvertisingSigns")]
        public float AdvertisingSigns { get; set; } = 20000;

        /// <summary>Gets or sets the costs of the advertising TV campaign.</summary>
        [XmlElement("AdvertisingTV")]
        public float AdvertisingTV { get; set; } = 5000;

        /// <summary>Gets or sets the ticket price for this event.</summary>
        [XmlElement("EntryCost")]
        public float Entry { get; set; } = 10;
    }
}
