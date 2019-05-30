// <copyright file="CityEventBase.cs" company="dymanoid">
//     Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.Events
{
    using System;
    using RealTime.Simulation;

    /// <summary>A base class for a city event.</summary>
    /// <seealso cref="ICityEvent"/>
    internal abstract class CityEventBase : ICityEvent
    {
        /// <summary>Gets the start time of this city event.</summary>
        public DateTime StartTime { get; private set; }

        /// <summary>Gets the end time of this city event.</summary>
        public DateTime EndTime => StartTime.AddHours(GetDuration());

        /// <summary>Gets the building ID this city event takes place in.</summary>
        public ushort BuildingId { get; private set; }

        /// <summary>Gets the localized name of the building this city event takes place in.</summary>
        public string BuildingName { get; private set; }

        /// <summary>
        /// Gets the event color.
        /// </summary>
        public abstract EventColor Color { get; }

        /// <summary>Accepts an event attendee with specified properties.</summary>
        /// <param name="age">The attendee age.</param>
        /// <param name="gender">The attendee gender.</param>
        /// <param name="education">The attendee education.</param>
        /// <param name="wealth">The attendee wealth.</param>
        /// <param name="wellbeing">The attendee wellbeing.</param>
        /// <param name="happiness">The attendee happiness.</param>
        /// <param name="randomizer">A reference to the game's randomizer.</param>
        /// <returns>
        /// <c>true</c> if the event attendee with specified properties is accepted and can attend
        /// this city event; otherwise, <c>false</c>.
        /// </returns>
        public virtual bool TryAcceptAttendee(
            Citizen.AgeGroup age,
            Citizen.Gender gender,
            Citizen.Education education,
            Citizen.Wealth wealth,
            Citizen.Wellbeing wellbeing,
            Citizen.Happiness happiness,
            IRandomizer randomizer) => true;

        /// <summary>
        /// Configures this event to take place in the specified building and at the specified start time.
        /// </summary>
        /// <param name="buildingId">The building ID this city event should take place in.</param>
        /// <param name="buildingName">
        /// The localized name of the building this city event should take place in.
        /// </param>
        /// <param name="startTime">The city event start time.</param>
        public void Configure(ushort buildingId, string buildingName, DateTime startTime)
        {
            BuildingId = buildingId;
            BuildingName = buildingName ?? string.Empty;
            StartTime = startTime;
        }

        /// <summary>
        /// Calculates the citizen's maximum budget for visiting a city event. It depends on the
        /// citizen's wealth.
        /// </summary>
        /// ///
        /// <param name="wealth">The citizen's wealth.</param>
        /// <param name="randomizer">A reference to the game's randomizer.</param>
        /// <returns>The citizen's budget for attending an event.</returns>
        protected static float GetCitizenBudgetForEvent(Citizen.Wealth wealth, IRandomizer randomizer)
        {
            switch (wealth)
            {
                case Citizen.Wealth.Low:
                    return 30f + randomizer.GetRandomValue(60);

                case Citizen.Wealth.Medium:
                    return 80f + randomizer.GetRandomValue(80);

                case Citizen.Wealth.High:
                    return 120f + randomizer.GetRandomValue(320);

                default:
                    return 0;
            }
        }

        /// <summary>When overridden in derived classes, calculates the city event duration.</summary>
        /// ///
        /// <returns>This city event duration in hours.</returns>
        protected abstract float GetDuration();
    }
}