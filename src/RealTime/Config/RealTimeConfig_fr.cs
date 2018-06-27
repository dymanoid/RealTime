// <copyright file="RealTimeConfig.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.Config
{
    using RealTime.UI;

    /// <summary>
    /// Configuration du mode.
    /// </summary>
    public sealed class RealTimeConfig
    {
        /// <summary>
        /// Définit une valeur indiquant si les week-ends sont activés. Les gens ne vont pas au travail le week-end.
        /// </summary>
        [ConfigItem("1General", 0)]
        [ConfigItemCheckBox]
        public bool IsWeekendEnabled { get; set; } = true;

        /// <summary>
        /// Définit une valeur indiquant si les gens devraient sortir au déjeûner pour s'alimenter.
        /// </summary>
        [ConfigItem("1General", 1)]
        [ConfigItemCheckBox]
        public bool IsLunchtimeEnabled { get; set; } = true;

        [ConfigItem("1General", 2)]
        [ConfigItemCheckBox]
        public bool StopConstructionAtNight { get; set; } = true;

        [ConfigItem("1General", 3)]
        [ConfigItemSlider(1, 100)]
        public uint ConstructionSpeed { get; set; } = 50;

        /// <summary>
        /// Définit le pourcentage de gens qui vont sortir pour le déjeûner.
        /// Valeurs comprises entre 0 et 100.
        /// </summary>
        [ConfigItem("2Quotas", 0)]
        [ConfigItemSlider(0, 100)]
        public uint LunchQuota { get; set; } = 80;

        /// <summary>
        /// Définit le pourcentage de la population qui recherchera localement un bâtiment.
        /// Valeurs comprises entre 0 et 100.
        /// </summary>
        [ConfigItem("2Quotas", 1)]
        [ConfigItemSlider(0, 100)]
        public uint LocalBuildingSearchQuota { get; set; } = 60;

        /// <summary>
        /// Définit le pourcentage de gens qui iront et quitteront leur travail ainsi que les élèves ou les étudiants
        /// à temps (pas d'heures supplémentaires !).
        /// Valeurs comprises entre 0 et 100.
        /// </summary>
        [ConfigItem("2Quotas", 2)]
        [ConfigItemSlider(0, 100)]
        public uint OnTimeQuota { get; set; } = 80;

        [ConfigItem("3Events", 0)]
        [ConfigItemSlider(0, 23.75f, 0.25f, SliderValueType.Time)]
        public float EarliestHourEventStartWeekday { get; set; } = 16f;

        [ConfigItem("3Events", 1)]
        [ConfigItemSlider(0, 23.75f, 0.25f, SliderValueType.Time)]
        public float LatestHourEventStartWeekday { get; set; } = 20f;

        [ConfigItem("3Events", 2)]
        [ConfigItemSlider(0, 23.75f, 0.25f, SliderValueType.Time)]
        public float EarliestHourEventStartWeekend { get; set; } = 8f;

        [ConfigItem("3Events", 3)]
        [ConfigItemSlider(0, 23.75f, 0.25f, SliderValueType.Time)]
        public float LatestHourEventStartWeekend { get; set; } = 22f;

        /// <summary>
        /// Définit le travail des heures de jour. Les adultes doivent être au travail.
        /// </summary>
        [ConfigItem("4Time", 0)]
        [ConfigItemSlider(4, 11, 0.25f, SliderValueType.Time)]
        public float WorkBegin { get; set; } = 9f;

        /// <summary>
        /// Définit l'heure de la journée lorsque les adultes reviennet du travail.
        /// </summary>
        [ConfigItem("4Time", 1)]
        [ConfigItemSlider(12, 20, 0.25f, SliderValueType.Time)]
        public float WorkEnd { get; set; } = 18f;

        /// <summary>
        /// Définit l'heure de la journée lorsque les gens sortent pour le déjeûner.
        /// </summary>
        [ConfigItem("4Time", 2)]
        [ConfigItemSlider(11, 13, 0.25f, SliderValueType.Time)]
        public float LunchBegin { get; set; } = 12f;

        /// <summary>
        /// Définit l'heure de la journée lorsque les gens reviennent du déjeûner au travail.
        /// </summary>
        [ConfigItem("4Time", 3)]
        [ConfigItemSlider(13, 15, 0.25f, SliderValueType.Time)]
        public float LunchEnd { get; set; } = 13f;

        /// <summary>
        /// Définit le temps supplémentaire maximum pour les gens. Ils viennent travailler plus tôt ou restent au travail plus longtemps
        /// selon le temps d'heures imparties. Ceci s'applique aux gens qui sont en retard, voir <see cref="OnTimeQuota"/>.
        /// Les enfants et adolescants (élèves et étudiants) ne font pas d'heures supplémentaires.
        /// </summary>
        [ConfigItem("4Time", 4)]
        [ConfigItemSlider(0, 4, 0.25f, SliderValueType.Duration)]
        public float MaxOvertime { get; set; } = 2f;

        /// <summary>
        /// Définit l'heure de début des cours de l'école ou de l'université. Les élèves ou les étudiants doivent être en cours.
        /// </summary>
        [ConfigItem("4Time", 5)]
        [ConfigItemSlider(4, 10, 0.25f, SliderValueType.Time)]
        public float SchoolBegin { get; set; } = 8f;

        /// <summary>
        /// Définit l'heure de la journée lorsque les élèves ou les étudiants reviennent des cours.
        /// </summary>
        [ConfigItem("4Time", 6)]
        [ConfigItemSlider(11, 16, 0.25f, SliderValueType.Time)]
        public float SchoolEnd { get; set; } = 14f;
    }
}
