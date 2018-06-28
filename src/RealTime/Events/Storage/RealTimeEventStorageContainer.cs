// <copyright file="RealTimeEventStorageContainer.cs" company="dymanoid">
//     Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.Events.Storage
{
    using System.Collections.Generic;
    using System.Xml.Serialization;

    /// <summary>
    /// A storage container class for the custom data of this mod. A serialized instance of this
    /// class can be stored in the save game.
    /// </summary>
    [XmlRoot("RealTimeEventStorage")]
    public sealed class RealTimeEventStorageContainer
    {
        /// <summary>Gets or sets the version of this class. Used for compatibility checks.</summary>
        [XmlAttribute]
        public int Version { get; set; } = 1;

        /// <summary>
        /// Gets or sets a date and time (in ticks) when the next city event can take place.
        /// </summary>
        [XmlElement]
        public long EarliestEvent { get; set; }

        /// <summary>Gets or sets the currently created city events.</summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "XML serialization")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "XML serialization")]
        [XmlArray("Events")]
        [XmlArrayItem("RealTimeEventStorage")]
        public List<RealTimeEventStorage> Events { get; set; } = new List<RealTimeEventStorage>();
    }
}