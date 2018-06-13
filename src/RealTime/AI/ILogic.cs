// <copyright file="ILogic.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.AI
{
    using RealTime.Config;

    /// <summary>
    /// Describes the basic Real Time customized logic for the Cims.
    /// </summary>
    internal interface ILogic
    {
        /// <summary>
        /// Gets a value indicating whether the current game time represents a weekend (no work on those days).
        /// Cims have free time.
        /// </summary>
        bool IsWeekend { get; }

        /// <summary>
        /// Gets a value indicating whether the current game time represents a work day.
        /// Cims go to work.
        /// </summary>
        bool IsWorkDay { get; }

        /// <summary>
        /// Gets a value indicating whether the current game time represents a work hour.
        /// Cims are working.
        /// </summary>
        bool IsWorkHour { get; }

        /// <summary>
        /// Gets a value indicating whether the current game time represents a school hour.
        /// Students are studying.
        /// </summary>
        bool IsSchoolHour { get; }

        /// <summary>
        /// Gets a value indicating whether the current game time represents a lunch hour.
        /// Cims are going out for lunch.
        /// </summary>
        bool IsLunchHour { get; }

        /// <summary>
        /// Gets a reference to the current logic configuration.
        /// </summary>
        Configuration CurrentConfig { get; }

        /// <summary>
        /// Determines whether the provided <paramref name="citizen"/> should go to school or to work.
        /// </summary>
        ///
        /// <param name="citizen">The <see cref="Citizen"/> to check.</param>
        /// <param name="buildingManager">A reference to a <see cref="BuildingManager"/> instance.</param>
        ///
        /// <returns>True if the <paramref name="citizen"/> should go go to work; otherwise, false.</returns>
        bool ShouldGoToSchoolOrWork(ref Citizen citizen, BuildingManager buildingManager);

        /// <summary>
        /// Determines whether the provided <paramref name="citizen"/> should return back from school or from work.
        /// </summary>
        ///
        /// <param name="citizen">The <see cref="Citizen"/> to check.</param>
        ///
        /// <returns>True if the <paramref name="citizen"/> should return back from work; otherwise, false.</returns>
        bool ShouldReturnFromSchoolOrWork(ref Citizen citizen);

        /// <summary>
        /// Determines whether the provided <paramref name="citizen"/> should go out for lunch.
        /// </summary>
        ///
        /// <param name="citizen">The <see cref="Citizen"/> to check.</param>
        ///
        /// <returns>True if the <paramref name="citizen"/> should go out for lunch; otherwise, false.</returns>
        bool ShouldGoToLunch(ref Citizen citizen);

        /// <summary>
        /// Determines whether the provided <paramref name="citizen"/> should find some entertainment.
        /// </summary>
        ///
        /// <param name="citizen">The <see cref="Citizen"/> to check.</param>
        ///
        /// <returns>True if the <paramref name="citizen"/> should find entertainment; otherwise, false.</returns>
        bool ShouldFindEntertainment(ref Citizen citizen);

        /// <summary>
        /// Determines whether the provided <paramref name="citizen"/> can stay out.
        /// </summary>
        ///
        /// <param name="citizen">The <see cref="Citizen"/> to check.</param>
        ///
        /// <returns>True if the <paramref name="citizen"/> can stay out; otherwise, false.</returns>
        bool CanStayOut(ref Citizen citizen);

        /// <summary>
        /// Gets a value in range from 0 to 100 that indicates a chance in percent that a Cim
        /// with provided <paramref name="age"/> will go out at night.
        /// </summary>
        /// <param name="age">The Cim's age.</param>
        ///
        /// <returns>A value in range from 0 to 100.</returns>
        int GetGoOutAtNightChance(int age);

        /// <summary>
        /// Determines whether the current game day is a work day and the time is between
        /// the provided hours.
        /// </summary>
        /// <param name="fromInclusive">The start day time hour of the range to check.</param>
        /// <param name="toExclusive">The end day time hour of the range to check.</param>
        ///
        /// <returns>True if the curent game day is a work day and the day time hour is
        /// between the provided values; otherwise, false.</returns>
        bool IsWorkDayAndBetweenHours(float fromInclusive, float toExclusive);
    }
}