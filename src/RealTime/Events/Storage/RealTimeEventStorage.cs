// <copyright file="RealTimeEventStorage.cs" company="dymanoid">
//     Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.Events.Storage
{
    using System.Xml.Serialization;

    /// <summary>
    /// A storage class for a city event instance. This information can be stored in the save game to
    /// allow the created city events to be restored after game load.
    /// </summary>
    public sealed class RealTimeEventStorage
    {
        /// <summary>
        /// Gets or sets the unique name of the event. Equals the <see
        /// cref="CityEventTemplate.EventName"/> from the template this event was generated from.
        /// </summary>
        [XmlAttribute]
        public string EventName { get; set; }

        /// <summary>
        /// Gets or sets the name of the building class this city event is taking place in. Equals
        /// the <see cref="CityEventTemplate.BuildingClassName"/> of the template this event was
        /// generated from.
        /// </summary>
        [XmlAttribute]
        public string BuildingClassName { get; set; }

        /// <summary>Gets or sets the start time of this city event.</summary>
        [XmlElement]
        public long StartTime { get; set; }

        /// <summary>Gets or sets the building ID this city event is taking place in.</summary>
        [XmlElement]
        public ushort BuildingId { get; set; }

        /// <summary>
        /// Gets or sets the localized building name this city event is taking place in.
        /// </summary>
        [XmlElement]
        public string BuildingName { get; set; }

        /// <summary>Gets or sets the current attendees count of this city event.</summary>
        [XmlElement]
        public int AttendeesCount { get; set; }
    }
}