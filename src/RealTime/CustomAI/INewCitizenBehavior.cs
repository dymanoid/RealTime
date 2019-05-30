// <copyright file="INewCitizenBehavior.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.CustomAI
{
    /// <summary>
    /// An interface for a behavior that determines the creation of new citizens.
    /// </summary>
    internal interface INewCitizenBehavior
    {
        /// <summary>
        /// Gets the education level of the new citizen based on their <paramref name="age"/>.
        /// </summary>
        ///
        /// <param name="age">The citizen's age as raw value (0-255).</param>
        ///
        /// <returns>The education level of the new citizen with the specified age.</returns>
        Citizen.Education GetEducation(int age);

        /// <summary>
        /// Adjusts the age of the new citizen based on their current <paramref name="age"/>.
        /// </summary>
        /// <param name="age">The citizen's age as raw value (0-255).</param>
        /// <returns>An adjusted raw value (0-255) for the citizen's age.</returns>
        int AdjustCitizenAge(int age);
    }
}
