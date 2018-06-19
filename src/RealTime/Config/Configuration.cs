// <copyright file="Configuration.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.Config
{
    using RealTime.UI;

    /// <summary>
    /// The mod's configuration.
    /// </summary>
    internal sealed class Configuration
    {
        /// <summary>
        /// Gets or sets a value indicating whether the weekends are enabled. Cims don't go to work on weekends.
        /// </summary>
        [ConfigItem("General", 0, 0)]
        [ConfigItemCheckBox]
        public bool IsWeekendEnabled { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether Cims should go out at lunch for food.
        /// </summary>
        [ConfigItem("General", 1, 0)]
        [ConfigItemCheckBox]
        public bool IsLunchTimeEnabled { get; set; } = true;

        /// <summary>
        /// Gets or sets the percentage of the Cims that will go out for lunch.
        /// Valid values are 0..100.
        /// </summary>
        [ConfigItem("General", 1, 1)]
        [ConfigItemSlider(0, 100)]
        public uint LunchQuota { get; set; } = 80;

        /// <summary>
        /// Gets or sets the percentage of the population that will search locally for buildings.
        /// Valid values are 0..100.
        /// </summary>
        [ConfigItem("General", 2, 0)]
        [ConfigItemSlider(0, 100)]
        public uint LocalBuildingSearchQuota { get; set; } = 60;

        /// <summary>
        /// Gets or sets the percentage of the Cims that will go to and leave their work or school
        /// on time (no overtime!).
        /// Valid values are 0..100.
        /// </summary>
        [ConfigItem("General", 3, 0)]
        [ConfigItemSlider(0, 100)]
        public uint OnTimeQuota { get; set; } = 80;

        /// <summary>
        /// Gets or sets the maximum overtime for the Cims. They come to work earlier or stay at work longer for at most this
        /// amout of hours. This applies only for those Cims that are not on time, see <see cref="OnTimeQuota"/>.
        /// The young Cims (school and university) don't do overtime.
        /// </summary>
        [ConfigItem("Time", 0, 4)]
        [ConfigItemSlider(0, 4, 0.25f)]
        public float MaxOvertime { get; set; } = 2f;

        /// <summary>
        /// Gets or sets the the school start daytime hour. The young Cims must be at school or university.
        /// </summary>
        [ConfigItem("Time", 1, 0)]
        [ConfigItemSlider(4, 10, 0.25f)]
        public float SchoolBegin { get; set; } = 8f;

        /// <summary>
        /// Gets or sets the daytime hour when the young Cims return from school or university.
        /// </summary>
        [ConfigItem("Time", 1, 1)]
        [ConfigItemSlider(11, 16, 0.25f)]
        public float SchoolEnd { get; set; } = 14f;

        /// <summary>
        /// Gets or sets the work start daytime hour. The adult Cims must be at work.
        /// </summary>
        [ConfigItem("Time", 0, 0)]
        [ConfigItemSlider(4, 11, 0.25f)]
        public float WorkBegin { get; set; } = 9f;

        /// <summary>
        /// Gets or sets the daytime hour when the adult Cims return from work.
        /// </summary>
        [ConfigItem("Time", 0, 1)]
        [ConfigItemSlider(12, 20, 0.25f)]
        public float WorkEnd { get; set; } = 18f;

        /// <summary>
        /// Gets or sets the daytime hour when the Cims go out for lunch.
        /// </summary>
        [ConfigItem("Time", 0, 2)]
        [ConfigItemSlider(11, 13, 0.25f)]
        public float LunchBegin { get; set; } = 12f;

        /// <summary>
        /// Gets or sets the lunch time duration.
        /// </summary>
        [ConfigItem("Time", 0, 3)]
        [ConfigItemSlider(13, 15, 0.25f)]
        public float LunchEnd { get; set; } = 13f;
    }
}
