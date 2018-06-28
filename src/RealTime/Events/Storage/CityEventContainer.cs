// <copyright file="CityEventContainer.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.Events.Storage
{
    using System.Collections.Generic;
    using System.Xml.Serialization;

    /// <summary>
    /// A storage container class for the city event templates.
    /// </summary>
    [XmlRoot("EventContainer")]
    public sealed class CityEventContainer
    {
        /// <summary>Gets the event templates stored in this container.</summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "XML serialization")]
        [XmlArray("Events", IsNullable = false)]
        [XmlArrayItem("Event")]
        public List<CityEventTemplate> Templates { get; } = new List<CityEventTemplate>();
    }
}
