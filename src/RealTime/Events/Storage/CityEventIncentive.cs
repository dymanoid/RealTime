// <copyright file="CityEventIncentive.cs" company="dymanoid">
//     Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.Events.Storage
{
    using System.Xml.Serialization;

    /// <summary>
    /// A storage class for the city event incentives settings.
    /// </summary>
    public sealed class CityEventIncentive
    {
        /// <summary>
        /// Gets or sets the incentive name.
        /// </summary>
        [XmlAttribute("Name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the incentive cost.
        /// </summary>
        [XmlAttribute("Cost")]
        public float Cost { get; set; } = 3;

        /// <summary>
        /// Gets or sets the incentive return cost.
        /// </summary>
        [XmlAttribute("ReturnCost")]
        public float ReturnCost { get; set; } = 10;

        /// <summary>
        /// Gets or sets a value indicating whether this incentive can be active in an auto-generated
        /// city event.
        /// </summary>
        [XmlAttribute("ActiveWhenRandomEvent")]
        public bool ActiveWhenRandomEvent { get; set; }

        /// <summary>
        /// Gets or sets the incentive description.
        /// </summary>
        [XmlElement("Description")]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a percentage value that describes the increase of the city event attendees
        /// if this incentive is active. Valid values are greater than 0.
        /// </summary>
        [XmlElement("PositiveEffect")]
        public int PositiveEffect { get; set; } = 10;

        /// <summary>
        /// Gets or sets a percentage value that describes the decrease of the city event attendees
        /// if this incentive is active. Valid values are greater than 0.
        /// </summary>
        [XmlElement("NegativeEffect")]
        public int NegativeEffect { get; set; } = 10;
    }
}
