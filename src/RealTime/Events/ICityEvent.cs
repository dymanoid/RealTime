// <copyright file="ICityEvent.cs" company="dymanoid">
//     Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.Events
{
    using System;
    using ColossalFramework.Math;

    /// <summary>An interface for a city event that is taking pace or being prepared.</summary>
    internal interface ICityEvent
    {
        /// <summary>Gets the start time of this city event.</summary>
        DateTime StartTime { get; }

        /// <summary>Gets the end time of this city event.</summary>
        DateTime EndTime { get; }

        /// <summary>Gets the building ID this city event takes place in.</summary>
        ushort BuildingId { get; }

        /// <summary>Gets the localized name of the building this city event takes place in.</summary>
        string BuildingName { get; }

        /// <summary>
        /// Configures this event to take place in the provided building and at the provided start time.
        /// </summary>
        /// ///
        /// <param name="buildingId">The building ID this city event should take place in.</param>
        /// <param name="buildingName">
        /// The localized name of the building this city event should take place in.
        /// </param>
        /// <param name="startTime">The city event start time.</param>
        void Configure(ushort buildingId, string buildingName, DateTime startTime);

        /// <summary>Accepts an event attendee with provided properties.</summary>
        /// <param name="age">The attendee age.</param>
        /// <param name="gender">The attendee gender.</param>
        /// <param name="education">The attendee education.</param>
        /// <param name="wealth">The attendee wealth.</param>
        /// <param name="wellbeing">The attendee wellbeing.</param>
        /// <param name="happiness">The attendee happiness.</param>
        /// <param name="randomizer">A reference to the game's randomizer.</param>
        /// <returns>
        /// <c>true</c> if the event attendee with provided properties is accepted and can attend
        /// this city event; otherwise, <c>false</c>.
        /// </returns>
        bool TryAcceptAttendee(
            Citizen.AgeGroup age,
            Citizen.Gender gender,
            Citizen.Education education,
            Citizen.Wealth wealth,
            Citizen.Wellbeing wellbeing,
            Citizen.Happiness happiness,
            ref Randomizer randomizer);
    }
}