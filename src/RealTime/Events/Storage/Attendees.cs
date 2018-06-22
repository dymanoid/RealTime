// <copyright file="Attendees.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.Events.Storage
{
    using System.Xml.Serialization;

    public sealed class Attendees
    {
        [XmlElement("Males")]
        public int Males { get; set; } = 100;

        [XmlElement("Females")]
        public int Females { get; set; } = 100;

        [XmlElement("Children")]
        public int Children { get; set; } = 100;

        [XmlElement("Teens")]
        public int Teens { get; set; } = 100;

        [XmlElement("YoungAdults")]
        public int YoungAdults { get; set; } = 100;

        [XmlElement("Adults")]
        public int Adults { get; set; } = 100;

        [XmlElement("Seniors")]
        public int Seniors { get; set; } = 100;

        [XmlElement("LowWealth")]
        public int LowWealth { get; set; } = 60;

        [XmlElement("MediumWealth")]
        public int MediumWealth { get; set; } = 100;

        [XmlElement("HighWealth")]
        public int HighWealth { get; set; } = 100;

        [XmlElement("Uneducated")]
        public int Uneducated { get; set; } = 100;

        [XmlElement("OneSchool")]
        public int OneSchool { get; set; } = 100;

        [XmlElement("TwoSchools")]
        public int TwoSchools { get; set; } = 100;

        [XmlElement("ThreeSchools")]
        public int ThreeSchools { get; set; } = 100;

        [XmlElement("BadHappiness")]
        public int BadHappiness { get; set; } = 40;

        [XmlElement("PoorHappiness")]
        public int PoorHappiness { get; set; } = 60;

        [XmlElement("GoodHappiness")]
        public int GoodHappiness { get; set; } = 100;

        [XmlElement("ExcellentHappiness")]
        public int ExcellentHappiness { get; set; } = 100;

        [XmlElement("SuperbHappiness")]
        public int SuperbHappiness { get; set; } = 100;

        [XmlElement("VeryUnhappyWellbeing")]
        public int VeryUnhappyWellbeing { get; set; } = 20;

        [XmlElement("UnhappyWellbeing")]
        public int UnhappyWellbeing { get; set; } = 50;

        [XmlElement("SatisfiedWellbeing")]
        public int SatisfiedWellbeing { get; set; } = 100;

        [XmlElement("HappyWellbeing")]
        public int HappyWellbeing { get; set; } = 100;

        [XmlElement("VeryHappyWellbeing")]
        public int VeryHappyWellbeing { get; set; } = 100;
    }
}
