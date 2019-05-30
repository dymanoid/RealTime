// <copyright file="NewCitizenBehavior.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.CustomAI
{
    using RealTime.Simulation;

    /// <summary>
    /// A behavior that determines the creation of new citizens.
    /// </summary>
    internal sealed class NewCitizenBehavior : INewCitizenBehavior
    {
        private readonly IRandomizer randomizer;

        /// <summary>
        /// Initializes a new instance of the <see cref="NewCitizenBehavior"/> class.
        /// </summary>
        /// <param name="randomizer">The randomizer to use for random decisions.</param>
        public NewCitizenBehavior(IRandomizer randomizer)
        {
            this.randomizer = randomizer;
        }

        /// <summary>
        /// Gets the education level of the new citizen based on their <paramref name="age" />.
        /// </summary>
        /// <param name="age">The citizen's age as raw value (0-255).</param>
        /// <returns>The education level of the new citizen with the specified age.</returns>
        public Citizen.Education GetEducation(int age)
        {
            var randomValue = randomizer.GetRandomValue(100u);

            // Age:
            // 0-14    -> child
            // 15-44   -> teen
            // 45-89   -> young
            // 90-179  -> adult
            // 180-255 -> senior
            if (age < 10)
            {
                // little children
                return Citizen.Education.Uneducated;
            }
            else if (age < 40)
            {
                // children and most of the teens
                return randomValue <= 25 ? Citizen.Education.Uneducated : Citizen.Education.OneSchool;
            }
            else if (age < 80)
            {
                // few teens and most of the young adults
                if (randomValue < 10)
                {
                    return Citizen.Education.Uneducated;
                }
                else if (randomValue < 50)
                {
                    return Citizen.Education.OneSchool;
                }
                else
                {
                    return Citizen.Education.TwoSchools;
                }
            }
            else if (age < 120)
            {
                // few young adults and some adults
                if (randomValue < 5)
                {
                    return Citizen.Education.Uneducated;
                }
                else if (randomValue < 15)
                {
                    return Citizen.Education.OneSchool;
                }
                else if (randomValue < 50)
                {
                    return Citizen.Education.TwoSchools;
                }
                else
                {
                    return Citizen.Education.ThreeSchools;
                }
            }
            else
            {
                // mature adults and all seniors
                if (randomValue < 10)
                {
                    return Citizen.Education.Uneducated;
                }
                else if (randomValue < 20)
                {
                    return Citizen.Education.OneSchool;
                }
                else if (randomValue < 40)
                {
                    return Citizen.Education.TwoSchools;
                }
                else
                {
                    return Citizen.Education.ThreeSchools;
                }
            }
        }

        /// <summary>
        /// Adjusts the age of the new citizen based on their current <paramref name="age"/>.
        /// </summary>
        /// <param name="age">The citizen's age as raw value (0-255).</param>
        /// <returns>An adjusted raw value (0-255) for the citizen's age.</returns>
        public int AdjustCitizenAge(int age)
        {
            // Age:
            // 0-14    -> child
            // 15-44   -> teen
            // 45-89   -> young
            // 90-179  -> adult
            // 180-255 -> senior
            if (age <= 1)
            {
                return age;
            }
            else if (age <= 15)
            {
                // children may be teens too
                return 4 + randomizer.GetRandomValue(40u);
            }
            else
            {
                return 75 + randomizer.GetRandomValue(115u);
            }
        }
    }
}
