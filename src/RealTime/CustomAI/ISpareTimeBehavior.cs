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
        /// Gets the probability whether a citizen with specified age would go relaxing on current time.
        /// </summary>
        ///
        /// <param name="citizenAge">The age of the citizen to check.</param>
        /// <param name="workShift">The citizen's assigned work shift (default is <see cref="WorkShift.Unemployed"/>).</param>
        ///
        /// <returns>A percentage value in range of 0..100 that describes the probability whether
        /// a citizen with specified age would go relaxing on current time.</returns>
        uint GetRelaxingChance(Citizen.AgeGroup citizenAge, WorkShift workShift = WorkShift.Unemployed);

        /// <summary>
        /// Gets the probability whether a citizen with specified age would go shopping on current time.
        /// </summary>
        ///
        /// <param name="citizenAge">The age of the citizen to check.</param>
        ///
        /// <returns>A percentage value in range of 0..100 that describes the probability whether
        /// a citizen with specified age would go shopping on current time.</returns>
        uint GetShoppingChance(Citizen.AgeGroup citizenAge);
    }
}