// <copyright file="CityEventContainer.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.Events.Storage
{
    using System.Collections.Generic;
    using System.Xml.Serialization;

    [XmlRoot("EventContainer", IsNullable = false)]
    public sealed class CityEventContainer
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "XML serialization")]
        [XmlArray("Events", IsNullable = false)]
        [XmlArrayItem("Event")]
        public List<CityEvent> Events { get; } = new List<CityEvent>();
    }
}
