// <copyright file="CityEventIncentive.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.Events.Storage
{
    using System.Xml.Serialization;

    public sealed class CityEventIncentive
    {
        [XmlAttribute("Name")]
        public string Name { get; set; } = string.Empty;

        [XmlAttribute("Cost")]
        public float Cost { get; set; } = 3;

        [XmlAttribute("ReturnCost")]
        public float ReturnCost { get; set; } = 10;

        [XmlAttribute("ActiveWhenRandomEvent")]
        public bool ActiveWhenRandomEvent { get; set; }

        [XmlElement("Description")]
        public string Description { get; set; } = string.Empty;

        [XmlElement("PositiveEffect")]
        public int PositiveEffect { get; set; } = 10;

        [XmlElement("NegativeEffect")]
        public int NegativeEffect { get; set; } = 10;
    }
}
