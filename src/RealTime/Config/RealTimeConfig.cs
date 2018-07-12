// <copyright file="RealTimeConfig.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.Config
{
    using System.Collections.Generic;
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
        [ConfigItemSlider(1, 8, DisplayMultiplier = 3.125f)]
        public uint SecondShiftQuota { get; set; }

        /// <summary>
        /// Gets or sets a value that determines the percentage of the Cims that will work night shift.
        /// Valid values are 1..8.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "NightShift", Justification = "Reviewed")]
        [ConfigItem("2Quotas", 0)]
        [ConfigItemSlider(1, 8, DisplayMultiplier = 3.125f)]
        public uint NightShiftQuota { get; set; }

        /// <summary>
        /// Gets or sets the percentage of the Cims that will go out for lunch.
        /// Valid values are 0..100.
        /// </summary>
        [ConfigItem("2Quotas", 0)]
        [ConfigItemSlider(0, 100)]
        public uint LunchQuota { get; set; }

        /// <summary>
        /// Gets or sets the percentage of the population that will search locally for buildings.
        /// Valid values are 0..100.
        /// </summary>
        [ConfigItem("2Quotas", 1)]
        [ConfigItemSlider(0, 100)]
        public uint LocalBuildingSearchQuota { get; set; }

        /// <summary>
        /// Gets or sets the percentage of the Cims that will go to and leave their work or school
        /// on time (no overtime!).
        /// Valid values are 0..100.
        /// </summary>
        [ConfigItem("2Quotas", 2)]
        [ConfigItemSlider(0, 100)]
        public uint OnTimeQuota { get; set; }

        /// <summary>
        /// Gets or sets the daytime hour when the earliest event on a week day can start.
        /// </summary>
        [ConfigItem("3Events", 0)]
        [ConfigItemSlider(0, 23.75f, 0.25f, SliderValueType.Time)]
        public float EarliestHourEventStartWeekday { get; set; }

        /// <summary>
        /// Gets or sets the daytime hour when the latest event on a week day can start.
        /// </summary>
        [ConfigItem("3Events", 1)]
        [ConfigItemSlider(0, 23.75f, 0.25f, SliderValueType.Time)]
        public float LatestHourEventStartWeekday { get; set; }

        /// <summary>
        /// Gets or sets the daytime hour when the earliest event on a Weekend day can start.
        /// </summary>
        [ConfigItem("3Events", 2)]
        [ConfigItemSlider(0, 23.75f, 0.25f, SliderValueType.Time)]
        public float EarliestHourEventStartWeekend { get; set; }

        /// <summary>
        /// Gets or sets the daytime hour when the latest event on a Weekend day can start.
        /// </summary>
        [ConfigItem("3Events", 3)]
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

        /// <summary>Validates this instance and corrects possible invalid property values.</summary>
        /// <returns>This instance.</returns>
        public RealTimeConfig Validate()
        {
            WakeupHour = Clamp(WakeupHour, 4f, 8f);
            GoToSleepUpHour = Clamp(GoToSleepUpHour, 20f, 23.75f);

            DayTimeSpeed = Clamp(DayTimeSpeed, 1u, 7u);
            NightTimeSpeed = Clamp(NightTimeSpeed, 1u, 7u);

            VirtualCitizens = (VirtualCitizensLevel)Clamp((int)VirtualCitizens, (int)VirtualCitizensLevel.None, (int)VirtualCitizensLevel.Many);
            ConstructionSpeed = Clamp(ConstructionSpeed, 0u, 100u);

            SecondShiftQuota = Clamp(SecondShiftQuota, 1u, 8u);
            NightShiftQuota = Clamp(NightShiftQuota, 1u, 8u);
            LunchQuota = Clamp(LunchQuota, 0u, 100u);
            LocalBuildingSearchQuota = Clamp(LocalBuildingSearchQuota, 0u, 100u);
            OnTimeQuota = Clamp(OnTimeQuota, 0u, 100u);

            EarliestHourEventStartWeekday = Clamp(EarliestHourEventStartWeekday, 0f, 23.75f);
            LatestHourEventStartWeekday = Clamp(LatestHourEventStartWeekday, 0f, 23.75f);
            if (LatestHourEventStartWeekday < EarliestHourEventStartWeekday)
            {
                LatestHourEventStartWeekday = EarliestHourEventStartWeekday;
            }

            EarliestHourEventStartWeekend = Clamp(EarliestHourEventStartWeekend, 0f, 23.75f);
            LatestHourEventStartWeekend = Clamp(LatestHourEventStartWeekend, 0f, 23.75f);
            if (LatestHourEventStartWeekend < EarliestHourEventStartWeekend)
            {
                LatestHourEventStartWeekend = EarliestHourEventStartWeekend;
            }

            WorkBegin = Clamp(WorkBegin, 4f, 11f);
            WorkEnd = Clamp(WorkEnd, 12f, 20f);
            LunchBegin = Clamp(LunchBegin, 11f, 13f);
            LunchEnd = Clamp(LunchEnd, 13f, 15f);
            SchoolBegin = Clamp(SchoolBegin, 4f, 10f);
            SchoolEnd = Clamp(SchoolEnd, 11f, 16f);
            MaxOvertime = Clamp(MaxOvertime, 0f, 4f);
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

            SecondShiftQuota = 4;
            NightShiftQuota = 2;

            LunchQuota = 80;
            LocalBuildingSearchQuota = 60;
            OnTimeQuota = 80;

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

        private static T Clamp<T>(T value, T min, T max)
            where T : struct
        {
            Comparer<T> comparer = Comparer<T>.Default;
            if (comparer.Compare(value, min) < 0)
            {
                return min;
            }

            if (comparer.Compare(value, max) > 0)
            {
                return max;
            }

            return value;
        }
    }
}
