// <copyright file="CityEventAttendees.cs" company="dymanoid">
//     Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.Events.Storage
{
    using System.Xml.Serialization;

    /// <summary>
    /// A storage class for the city event attendees settings.
    /// </summary>
    public sealed class CityEventAttendees
    {
        /// <summary>
        /// Gets or sets the percentage of male citizens that can attend the city event. Valid values
        /// are 0..100.
        /// </summary>
        [XmlElement("Males")]
        public int Males { get; set; } = 100;

        /// <summary>
        /// Gets or sets the percentage of female citizens that can attend the city event. Valid
        /// values are 0..100.
        /// </summary>
        [XmlElement("Females")]
        public int Females { get; set; } = 100;

        /// <summary>
        /// Gets or sets the percentage of children that can attend the city event. Valid values are 0..100.
        /// </summary>
        [XmlElement("Children")]
        public int Children { get; set; } = 100;

        /// <summary>
        /// Gets or sets the percentage of teens that can attend the city event. Valid values are 0..100.
        /// </summary>
        [XmlElement("Teens")]
        public int Teens { get; set; } = 100;

        /// <summary>
        /// Gets or sets the percentage of young adults that can attend the city event. Valid values
        /// are 0..100.
        /// </summary>
        [XmlElement("YoungAdults")]
        public int YoungAdults { get; set; } = 100;

        /// <summary>
        /// Gets or sets the percentage of adults that can attend the city event. Valid values are 0..100.
        /// </summary>
        [XmlElement("Adults")]
        public int Adults { get; set; } = 100;

        /// <summary>
        /// Gets or sets the percentage of seniors citizens that can attend the city event. Valid
        /// values are 0..100.
        /// </summary>
        [XmlElement("Seniors")]
        public int Seniors { get; set; } = 100;

        /// <summary>
        /// Gets or sets the percentage of low wealth citizens that can attend the city event. Valid
        /// values are 0..100.
        /// </summary>
        [XmlElement("LowWealth")]
        public int LowWealth { get; set; } = 60;

        /// <summary>
        /// Gets or sets the percentage of medium wealth citizens that can attend the city event.
        /// Valid values are 0..100.
        /// </summary>
        [XmlElement("MediumWealth")]
        public int MediumWealth { get; set; } = 100;

        /// <summary>
        /// Gets or sets the percentage of high wealth citizens that can attend the city event. Valid
        /// values are 0..100.
        /// </summary>
        [XmlElement("HighWealth")]
        public int HighWealth { get; set; } = 100;

        /// <summary>
        /// Gets or sets the percentage of uneducated citizens that can attend the city event. Valid
        /// values are 0..100.
        /// </summary>
        [XmlElement("Uneducated")]
        public int Uneducated { get; set; } = 100;

        /// <summary>
        /// Gets or sets the percentage of citizens with only primary school education that can
        /// attend the city event. Valid values are 0..100.
        /// </summary>
        [XmlElement("OneSchool")]
        public int OneSchool { get; set; } = 100;

        /// <summary>
        /// Gets or sets the percentage of citizens with high school education that can attend the
        /// city event. Valid values are 0..100.
        /// </summary>
        [XmlElement("TwoSchools")]
        public int TwoSchools { get; set; } = 100;

        /// <summary>
        /// Gets or sets the percentage of citizens with university education that can attend the
        /// city event. Valid values are 0..100.
        /// </summary>
        [XmlElement("ThreeSchools")]
        public int ThreeSchools { get; set; } = 100;

        /// <summary>
        /// Gets or sets the percentage of very unhappy citizens that can attend the city event.
        /// Valid values are 0..100.
        /// </summary>
        [XmlElement("BadHappiness")]
        public int BadHappiness { get; set; } = 40;

        /// <summary>
        /// Gets or sets the percentage of unhappy citizens that can attend the city event. Valid
        /// values are 0..100.
        /// </summary>
        [XmlElement("PoorHappiness")]
        public int PoorHappiness { get; set; } = 60;

        /// <summary>
        /// Gets or sets the percentage of happy citizens that can attend the city event. Valid
        /// values are 0..100.
        /// </summary>
        [XmlElement("GoodHappiness")]
        public int GoodHappiness { get; set; } = 100;

        /// <summary>
        /// Gets or sets the percentage of very happy citizens that can attend the city event. Valid
        /// values are 0..100.
        /// </summary>
        [XmlElement("ExcellentHappiness")]
        public int ExcellentHappiness { get; set; } = 100;

        /// <summary>
        /// Gets or sets the percentage of extremely happy citizens that can attend the city event.
        /// Valid values are 0..100.
        /// </summary>
        [XmlElement("SuperbHappiness")]
        public int SuperbHappiness { get; set; } = 100;

        /// <summary>
        /// Gets or sets the percentage of citizens with very unhappy wellbeing that can attend the
        /// city event. Valid values are 0..100.
        /// </summary>
        [XmlElement("VeryUnhappyWellbeing")]
        public int VeryUnhappyWellbeing { get; set; } = 20;

        /// <summary>
        /// Gets or sets the percentage of citizens with unhappy wellbeing that can attend the city
        /// event. Valid values are 0..100.
        /// </summary>
        [XmlElement("UnhappyWellbeing")]
        public int UnhappyWellbeing { get; set; } = 50;

        /// <summary>
        /// Gets or sets the percentage of citizens with satisfied wellbeing that can attend the city
        /// event. Valid values are 0..100.
        /// </summary>
        [XmlElement("SatisfiedWellbeing")]
        public int SatisfiedWellbeing { get; set; } = 100;

        /// <summary>
        /// Gets or sets the percentage of citizens with happy wellbeing that can attend the city
        /// event. Valid values are 0..100.
        /// </summary>
        [XmlElement("HappyWellbeing")]
        public int HappyWellbeing { get; set; } = 100;

        /// <summary>
        /// Gets or sets the percentage of citizens with very happy wellbeing that can attend the
        /// city event. Valid values are 0..100.
        /// </summary>
        [XmlElement("VeryHappyWellbeing")]
        public int VeryHappyWellbeing { get; set; } = 100;
    }
}