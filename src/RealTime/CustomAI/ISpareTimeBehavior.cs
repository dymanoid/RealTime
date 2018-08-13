// <copyright file="ISpareTimeBehavior.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.CustomAI
{
    /// <summary>
    /// An interface for custom logic for the spare time simulation.
    /// </summary>
    internal interface ISpareTimeBehavior
    {
        /// <summary>
        /// Gets a value indicating whether the fireworks in the parks are allowed at current time.
        /// </summary>
        bool AreFireworksAllowed { get; }

        /// <summary>
        /// Gets the probability whether a citizen with specified age would go relaxing on current time.
        /// </summary>
        ///
        /// <param name="citizenAge">The age of the citizen to check.</param>
        /// <param name="workShift">The citizen's assigned work shift (default is <see cref="WorkShift.Unemployed"/>).</param>
        /// <param name="isOnVacation"><c>true</c> if the citizen is on vacation.</param>
        ///
        /// <returns>A percentage value in range of 0..100 that describes the probability whether
        /// a citizen with specified age would go relaxing on current time.</returns>
        uint GetRelaxingChance(Citizen.AgeGroup citizenAge, WorkShift workShift = WorkShift.Unemployed, bool isOnVacation = false);

        /// <summary>
        /// Gets the probability whether a citizen with specified age would go shopping on current time.
        /// </summary>
        ///
        /// <param name="citizenAge">The age of the citizen to check.</param>
        ///
        /// <returns>A percentage value in range of 0..100 that describes the probability whether
        /// a citizen with specified age would go shopping on current time.</returns>
        uint GetShoppingChance(Citizen.AgeGroup citizenAge);

        /// <summary>Gets a precise probability (in percent multiplied by 100) for a citizen with specified
        /// wealth to go on vacation on current day.</summary>
        /// <param name="wealth">The citizen's wealth.</param>
        /// <returns>The precise probability (in percent multiplied by 100) for the citizen to go on vacation
        /// on current day.</returns>
        uint GetPreciseVacationChance(Citizen.Wealth wealth);
    }
}