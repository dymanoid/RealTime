// <copyright file="Costs.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.Events.Storage
{
    using System.Xml.Serialization;

    public class Costs
    {
        [XmlElement("Creation")]
        public float Creation { get; set; } = 100;

        [XmlElement("PerHead")]
        public float PerHead { get; set; } = 5;

        [XmlElement("AdvertisingSigns")]
        public float AdvertisingSigns { get; set; } = 20000;

        [XmlElement("AdvertisingTV")]
        public float AdvertisingTV { get; set; } = 5000;

        [XmlElement("EntryCost")]
        public float Entry { get; set; } = 10;
    }
}
