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
        /// <summary>Gets or sets the ticket price for this event.</summary>
        [XmlElement("EntryCost")]
        public float Entry { get; set; } = 10;
    }
}
