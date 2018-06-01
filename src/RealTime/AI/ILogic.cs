// <copyright file="ILogic.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.AI
{
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
        /// Determines whether the provided <paramref name="citizen"/> should go to work.
        /// </summary>
        ///
        /// <param name="citizen">The <see cref="Citizen"/> to check.</param>
        /// <param name="ignoreMinimumDuration">True to ignore the time constraints for the minimum work duration.</param>
        ///
        /// <returns>True if the <paramref name="citizen"/> should go go to work; otherwise, false.</returns>
        bool ShouldGoToWork(ref Citizen citizen, bool ignoreMinimumDuration = false);

        /// <summary>
        /// Determines whether the provided <paramref name="citizen"/> should return back from work.
        /// </summary>
        ///
        /// <param name="citizen">The <see cref="Citizen"/> to check.</param>
        ///
        /// <returns>True if the <paramref name="citizen"/> should return back from work; otherwise, false.</returns>
        bool ShouldReturnFromWork(ref Citizen citizen);

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
    }
}