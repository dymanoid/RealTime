﻿// <copyright file="RealTimeConfig.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.Config
{
    using RealTime.Tools;
    using RealTime.UI;

    /// <summary>
    /// The mod's configuration.
    /// </summary>
    public sealed class RealTimeConfig
    {
        /// <summary>Initializes a new instance of the <see cref="RealTimeConfig"/> class.</summary>
        public RealTimeConfig()
        {
            ResetToDefaults();
        }

        /// <summary>Gets or sets the version number of this configuration.</summary>
        public int Version { get; set; }

        /// <summary>
        /// Gets or sets the daytime hour when the city wakes up.
        /// </summary>
        [ConfigItem("1General", "0Time", 0)]
        [ConfigItemSlider(4f, 8f, 0.25f, SliderValueType.Time)]
        public float WakeupHour { get; set; }

        /// <summary>
        /// Gets or sets the daytime hour when the city goes to sleep.
        /// </summary>
        [ConfigItem("1General", "0Time", 1)]
        [ConfigItemSlider(20f, 23.75f, 0.25f, SliderValueType.Time)]
        public float GoToSleepUpHour { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the dynamic day length is enabled.
        /// The dynamic day length depends on map's location and day of the year.
        /// </summary>
        [ConfigItem("1General", "0Time", 4)]
        [ConfigItemCheckBox]
        public bool IsDynamicDayLengthEnabled { get; set; }

        /// <summary>
        /// Gets or sets the speed of the time flow on daytime. Valid values are 1..7.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "DayTime", Justification = "Reviewed")]
        [ConfigItem("1General", "0Time", 2)]
        [ConfigItemSlider(1, 7, ValueType = SliderValueType.Default)]
        public uint DayTimeSpeed { get; set; }

        /// <summary>
        /// Gets or sets the speed of the time flow on night time. Valid values are 1..7.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "NightTime", Justification = "Reviewed")]
        [ConfigItem("1General", "0Time", 3)]
        [ConfigItemSlider(1, 7, ValueType = SliderValueType.Default)]
        public uint NightTimeSpeed { get; set; }

        /// <summary>
        /// Gets or sets the virtual citizens mode.
        /// </summary>
        [ConfigItem("1General", "1Other", 0)]
        [ConfigItemComboBox]
        public VirtualCitizensLevel VirtualCitizens { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the weekends are enabled. Cims don't go to work on weekends.
        /// </summary>
        [ConfigItem("1General", "0Time", 5)]
        [ConfigItemCheckBox]
        public bool IsWeekendEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether Cims should go out at lunch for food.
        /// </summary>
        [ConfigItem("1General", "0Time", 6)]
        [ConfigItemCheckBox]
        public bool IsLunchtimeEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the construction sites should pause at night time.
        /// </summary>
        [ConfigItem("1General", "1Other", 1)]
        [ConfigItemCheckBox]
        public bool StopConstructionAtNight { get; set; }

        /// <summary>
        /// Gets or sets the percentage value of the building construction speed. Valid values are 1..100.
        /// </summary>
        [ConfigItem("1General", "1Other", 2)]
        [ConfigItemSlider(1, 100)]
        public uint ConstructionSpeed { get; set; }

        /// <summary>
        /// Gets or sets a value that determines the percentage of the Cims that will work second shift.
        /// Valid values are 1..8.
        /// </summary>
        [ConfigItem("2Quotas", 0)]
        [ConfigItemSlider(1, 25)]
        public uint SecondShiftQuota { get; set; }

        /// <summary>
        /// Gets or sets a value that determines the percentage of the Cims that will work night shift.
        /// Valid values are 1..8.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "NightShift", Justification = "Reviewed")]
        [ConfigItem("2Quotas", 1)]
        [ConfigItemSlider(1, 25)]
        public uint NightShiftQuota { get; set; }

        /// <summary>
        /// Gets or sets the percentage of the Cims that will go out for lunch.
        /// Valid values are 0..100.
        /// </summary>
        [ConfigItem("2Quotas", 2)]
        [ConfigItemSlider(0, 100)]
        public uint LunchQuota { get; set; }

        /// <summary>
        /// Gets or sets the percentage of the population that will search locally for buildings.
        /// Valid values are 0..100.
        /// </summary>
        [ConfigItem("2Quotas", 3)]
        [ConfigItemSlider(0, 100)]
        public uint LocalBuildingSearchQuota { get; set; }

        /// <summary>
        /// Gets or sets the percentage of the Cims that will go to and leave their work or school
        /// on time (no overtime!).
        /// Valid values are 0..100.
        /// </summary>
        [ConfigItem("2Quotas", 4)]
        [ConfigItemSlider(0, 100)]
        public uint OnTimeQuota { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the custom events are enabled.
        /// </summary>
        [ConfigItem("3Events", 0)]
        [ConfigItemCheckBox]
        public bool AreEventsEnabled { get; set; }

        /// <summary>
        /// Gets or sets the daytime hour when the earliest event on a week day can start.
        /// </summary>
        [ConfigItem("3Events", 1)]
        [ConfigItemSlider(0, 23.75f, 0.25f, SliderValueType.Time)]
        public float EarliestHourEventStartWeekday { get; set; }

        /// <summary>
        /// Gets or sets the daytime hour when the latest event on a week day can start.
        /// </summary>
        [ConfigItem("3Events", 2)]
        [ConfigItemSlider(0, 23.75f, 0.25f, SliderValueType.Time)]
        public float LatestHourEventStartWeekday { get; set; }

        /// <summary>
        /// Gets or sets the daytime hour when the earliest event on a Weekend day can start.
        /// </summary>
        [ConfigItem("3Events", 3)]
        [ConfigItemSlider(0, 23.75f, 0.25f, SliderValueType.Time)]
        public float EarliestHourEventStartWeekend { get; set; }

        /// <summary>
        /// Gets or sets the daytime hour when the latest event on a Weekend day can start.
        /// </summary>
        [ConfigItem("3Events", 4)]
        [ConfigItemSlider(0, 23.75f, 0.25f, SliderValueType.Time)]
        public float LatestHourEventStartWeekend { get; set; }

        /// <summary>
        /// Gets or sets the work start daytime hour. The adult Cims must be at work.
        /// </summary>
        [ConfigItem("4Time", 0)]
        [ConfigItemSlider(4, 11, 0.25f, SliderValueType.Time)]
        public float WorkBegin { get; set; }

        /// <summary>
        /// Gets or sets the daytime hour when the adult Cims return from work.
        /// </summary>
        [ConfigItem("4Time", 1)]
        [ConfigItemSlider(12, 20, 0.25f, SliderValueType.Time)]
        public float WorkEnd { get; set; }

        /// <summary>
        /// Gets or sets the daytime hour when the Cims go out for lunch.
        /// </summary>
        [ConfigItem("4Time", 2)]
        [ConfigItemSlider(11, 13, 0.25f, SliderValueType.Time)]
        public float LunchBegin { get; set; }

        /// <summary>
        /// Gets or sets the daytime hour when the Cims return from lunch back to work.
        /// </summary>
        [ConfigItem("4Time", 3)]
        [ConfigItemSlider(13, 15, 0.25f, SliderValueType.Time)]
        public float LunchEnd { get; set; }

        /// <summary>
        /// Gets or sets the maximum overtime for the Cims. They come to work earlier or stay at work longer for at most this
        /// amount of hours. This applies only for those Cims that are not on time, see <see cref="OnTimeQuota"/>.
        /// The young Cims (school and university) don't do overtime.
        /// </summary>
        [ConfigItem("4Time", 4)]
        [ConfigItemSlider(0, 4, 0.25f, SliderValueType.Duration)]
        public float MaxOvertime { get; set; }

        /// <summary>
        /// Gets or sets the school start daytime hour. The young Cims must be at school or university.
        /// </summary>
        [ConfigItem("4Time", 5)]
        [ConfigItemSlider(4, 10, 0.25f, SliderValueType.Time)]
        public float SchoolBegin { get; set; }

        /// <summary>
        /// Gets or sets the daytime hour when the young Cims return from school or university.
        /// </summary>
        [ConfigItem("4Time", 6)]
        [ConfigItemSlider(11, 16, 0.25f, SliderValueType.Time)]
        public float SchoolEnd { get; set; }

        /// <summary>Checks the version of the deserialized object and migrates it to the latest version when necessary.</summary>
        /// <returns>This instance.</returns>
        public RealTimeConfig MigrateWhenNecessary()
        {
            if (Version == 0)
            {
                SecondShiftQuota = (uint)(SecondShiftQuota * 3.125f);
                NightShiftQuota = (uint)(NightShiftQuota * 3.125f);
            }

            Version = 1;
            return this;
        }

        /// <summary>Validates this instance and corrects possible invalid property values.</summary>
        /// <returns>This instance.</returns>
        public RealTimeConfig Validate()
        {
            WakeupHour = RealTimeMath.Clamp(WakeupHour, 4f, 8f);
            GoToSleepUpHour = RealTimeMath.Clamp(GoToSleepUpHour, 20f, 23.75f);

            DayTimeSpeed = RealTimeMath.Clamp(DayTimeSpeed, 1u, 7u);
            NightTimeSpeed = RealTimeMath.Clamp(NightTimeSpeed, 1u, 7u);

            VirtualCitizens = (VirtualCitizensLevel)RealTimeMath.Clamp((int)VirtualCitizens, (int)VirtualCitizensLevel.None, (int)VirtualCitizensLevel.Many);
            ConstructionSpeed = RealTimeMath.Clamp(ConstructionSpeed, 0u, 100u);

            SecondShiftQuota = RealTimeMath.Clamp(SecondShiftQuota, 1u, 25u);
            NightShiftQuota = RealTimeMath.Clamp(NightShiftQuota, 1u, 25u);
            LunchQuota = RealTimeMath.Clamp(LunchQuota, 0u, 100u);
            LocalBuildingSearchQuota = RealTimeMath.Clamp(LocalBuildingSearchQuota, 0u, 100u);
            OnTimeQuota = RealTimeMath.Clamp(OnTimeQuota, 0u, 100u);

            EarliestHourEventStartWeekday = RealTimeMath.Clamp(EarliestHourEventStartWeekday, 0f, 23.75f);
            LatestHourEventStartWeekday = RealTimeMath.Clamp(LatestHourEventStartWeekday, 0f, 23.75f);
            if (LatestHourEventStartWeekday < EarliestHourEventStartWeekday)
            {
                LatestHourEventStartWeekday = EarliestHourEventStartWeekday;
            }

            EarliestHourEventStartWeekend = RealTimeMath.Clamp(EarliestHourEventStartWeekend, 0f, 23.75f);
            LatestHourEventStartWeekend = RealTimeMath.Clamp(LatestHourEventStartWeekend, 0f, 23.75f);
            if (LatestHourEventStartWeekend < EarliestHourEventStartWeekend)
            {
                LatestHourEventStartWeekend = EarliestHourEventStartWeekend;
            }

            WorkBegin = RealTimeMath.Clamp(WorkBegin, 4f, 11f);
            WorkEnd = RealTimeMath.Clamp(WorkEnd, 12f, 20f);
            LunchBegin = RealTimeMath.Clamp(LunchBegin, 11f, 13f);
            LunchEnd = RealTimeMath.Clamp(LunchEnd, 13f, 15f);
            SchoolBegin = RealTimeMath.Clamp(SchoolBegin, 4f, 10f);
            SchoolEnd = RealTimeMath.Clamp(SchoolEnd, 11f, 16f);
            MaxOvertime = RealTimeMath.Clamp(MaxOvertime, 0f, 4f);
            return this;
        }

        /// <summary>Resets all values to their defaults.</summary>
        public void ResetToDefaults()
        {
            WakeupHour = 6f;
            GoToSleepUpHour = 22f;

            IsDynamicDayLengthEnabled = true;
            DayTimeSpeed = 5;
            NightTimeSpeed = 5;

            VirtualCitizens = VirtualCitizensLevel.Few;

            IsWeekendEnabled = true;
            IsLunchtimeEnabled = true;

            StopConstructionAtNight = true;
            ConstructionSpeed = 50;

            SecondShiftQuota = 13;
            NightShiftQuota = 6;

            LunchQuota = 80;
            LocalBuildingSearchQuota = 60;
            OnTimeQuota = 80;

            AreEventsEnabled = true;
            EarliestHourEventStartWeekday = 16f;
            LatestHourEventStartWeekday = 20f;
            EarliestHourEventStartWeekend = 8f;
            LatestHourEventStartWeekend = 22f;

            WorkBegin = 9f;
            WorkEnd = 18f;
            LunchBegin = 12f;
            LunchEnd = 13f;
            MaxOvertime = 2f;
            SchoolBegin = 8f;
            SchoolEnd = 14f;
        }
    }
}
