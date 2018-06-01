// <copyright file="Configuration.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.Config
{
    /// <summary>
    /// The mod's configuration.
    /// </summary>
    internal sealed class Configuration
    {
        /// <summary>
        /// Gets or sets the current mod configuration.
        /// </summary>
        public static Configuration Current { get; set; } = new Configuration();

        /// <summary>
        /// Gets or sets a value indicating whether the Weekends are enabled. Cims don't go to work on Weekends.
        /// </summary>
        public bool EnableWeekends { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether Cims should go out at lunch for food.
        /// </summary>
        public bool SimulateLunchTime { get; set; } = true;

        /// <summary>
        /// Gets or sets the percentage of the Cims that will go out for lunch.
        /// </summary>
        public float LunchQuota { get; set; } = 0.6f;

        /// <summary>
        /// Gets or sets a value indicating whether Cims will search locally for buildings to visit,
        /// rather than heading to a random building.
        /// </summary>
        public bool AllowLocalBuildingSearch { get; set; } = true;

        /// <summary>
        /// Gets or sets the percentage of the population that will search locally for buildings.
        /// Valid values are 0.0 to 1.0.
        /// </summary>
        public float LocalBuildingSearchQuota { get; set; } = 0.5f;

        /// <summary>
        /// Gets or sets the percentage of the Cims that will attend their way to work or to school
        /// as soon as possible.
        /// </summary>
        public float EarlyStartQuota { get; set; } = 0.4f;

        /// <summary>
        /// Gets or sets the percentage of the Cims that will leave their work or school
        /// on time (no overtime!).
        /// </summary>
        public float LeaveOnTimeQuota { get; set; } = 0.8f;

        /// <summary>
        /// Gets or sets the earliest daytime hour when the young Cims head to school or university.
        /// </summary>
        public float MinSchoolHour { get; set; } = 7.5f;

        /// <summary>
        /// Gets or sets the latest daytime hour when the young Cims head to school or university.
        /// </summary>
        public float StartSchoolHour { get; set; } = 8f;

        /// <summary>
        /// Gets or sets the earliest daytime hour when the young Cims return from school or university.
        /// </summary>
        public float EndSchoolHour { get; set; } = 13.5f;

        /// <summary>
        /// Gets or sets the latest daytime hour when the young Cims return from school or university.
        /// </summary>
        public float MaxSchoolHour { get; set; } = 14f;

        /// <summary>
        /// Gets or sets the earliest daytime hour when the adult Cims head to work.
        /// </summary>
        public float MinWorkHour { get; set; } = 6f;

        /// <summary>
        /// Gets or sets the latest daytime hour when the adult Cims head to work.
        /// </summary>
        public float StartWorkHour { get; set; } = 9f;

        /// <summary>
        /// Gets or sets the earliest daytime hour when the adult Cims return from work.
        /// </summary>
        public float EndWorkHour { get; set; } = 15f;

        /// <summary>
        /// Gets or sets the latest daytime hour when the adult Cims return from work.
        /// </summary>
        public float MaxWorkHour { get; set; } = 18f;

        /// <summary>
        /// Gets or sets the daytime hour when the Cims go out for lunch.
        /// </summary>
        public float LunchBegin { get; set; } = 12f;

        /// <summary>
        /// Gets or sets the daytime hour when the Cims return for lunch.
        /// </summary>
        public float LunchEnd { get; set; } = 12.75f;

        /// <summary>
        /// Gets or sets the minimum school duration (in hours).
        /// Cims will not travel to school or university if the time spent there will not reach at least this value.
        /// </summary>
        public float MinSchoolDuration { get; set; } = 1f;

        /// <summary>
        /// Gets or sets the minimum work duration (in hours).
        /// Cims will not travel to work if the time spent there will not reach at least this value.
        /// </summary>
        public float MinWorkDuration { get; set; } = 1f;

        /// <summary>
        /// Gets or sets the assumed maximum travel time (in hours) to work or to school or to university.
        /// </summary>
        public float WorkOrSchoolTravelTime { get; set; } = 2f;
    }
}
