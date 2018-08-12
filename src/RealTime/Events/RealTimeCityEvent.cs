// <copyright file="RealTimeCityEvent.cs" company="dymanoid">Copyright (c) dymanoid. All rights reserved.</copyright>

namespace RealTime.Events
{
    using System;
    using System.Linq;
    using RealTime.Events.Storage;
    using RealTime.Simulation;

    /// <summary>A custom city event.</summary>
    /// <seealso cref="CityEventBase"/>
    internal sealed class RealTimeCityEvent : CityEventBase
    {
        private readonly CityEventTemplate eventTemplate;

        private int attendeesCount;

        /// <summary>
        /// Initializes a new instance of the <see cref="RealTimeCityEvent"/> class using the specified <paramref name="eventTemplate"/>.
        /// </summary>
        /// <param name="eventTemplate">The event template this city event is created from.</param>
        /// <exception cref="ArgumentNullException">Thrown when the argument is null.</exception>
        public RealTimeCityEvent(CityEventTemplate eventTemplate)
        {
            this.eventTemplate = eventTemplate ?? throw new ArgumentNullException(nameof(eventTemplate));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RealTimeCityEvent"/> class using the specified
        /// <paramref name="eventTemplate"/> and the already known current <paramref name="attendeesCount"/>.
        /// </summary>
        /// <param name="eventTemplate">The event template this city event is created from.</param>
        /// <param name="attendeesCount">The current attendees count of this city event.</param>
        public RealTimeCityEvent(CityEventTemplate eventTemplate, int attendeesCount)
            : this(eventTemplate)
        {
            this.attendeesCount = attendeesCount;
        }

        /// <summary>Accepts an event attendee with specified properties.</summary>
        /// <param name="age">The attendee age.</param>
        /// <param name="gender">The attendee gender.</param>
        /// <param name="education">The attendee education.</param>
        /// <param name="wealth">The attendee wealth.</param>
        /// <param name="wellbeing">The attendee wellbeing.</param>
        /// <param name="happiness">The attendee happiness.</param>
        /// <param name="randomizer">A reference to the game's randomizer.</param>
        /// <returns>
        /// <c>true</c> if the event attendee with specified properties is accepted and can attend this city event;
        /// otherwise, <c>false</c>.
        /// </returns>
        public override bool TryAcceptAttendee(
            Citizen.AgeGroup age,
            Citizen.Gender gender,
            Citizen.Education education,
            Citizen.Wealth wealth,
            Citizen.Wellbeing wellbeing,
            Citizen.Happiness happiness,
            IRandomizer randomizer)
        {
            if (attendeesCount > eventTemplate.Capacity)
            {
                return false;
            }

            if (eventTemplate.Costs != null && eventTemplate.Costs.Entry > GetCitizenBudgetForEvent(wealth, randomizer))
            {
                return false;
            }

            CityEventAttendees attendees = eventTemplate.Attendees;
            float randomPercentage = randomizer.GetRandomValue(100u);

            if (!CheckAge(age, attendees, randomPercentage))
            {
                return false;
            }

            randomPercentage = randomizer.GetRandomValue(100u);
            if (!CheckGender(gender, attendees, randomPercentage))
            {
                return false;
            }

            randomPercentage = randomizer.GetRandomValue(100u);
            if (!CheckEducation(education, attendees, randomPercentage))
            {
                return false;
            }

            randomPercentage = randomizer.GetRandomValue(100u);
            if (!CheckWealth(wealth, attendees, randomPercentage))
            {
                return false;
            }

            randomPercentage = randomizer.GetRandomValue(100u);
            if (!CheckWellbeing(wellbeing, attendees, randomPercentage))
            {
                return false;
            }

            randomPercentage = randomizer.GetRandomValue(100u);
            if (!CheckHappiness(happiness, attendees, randomPercentage))
            {
                return false;
            }

            attendeesCount++;
            return true;
        }

        /// <summary>
        /// Creates an instance of the <see cref="RealTimeEventStorage"/> class that contains the current city event data.
        /// </summary>
        /// <returns>A new instance of the <see cref="RealTimeEventStorage"/> class.</returns>
        public RealTimeEventStorage GetStorageData()
        {
            return new RealTimeEventStorage
            {
                EventName = eventTemplate.EventName,
                BuildingClassName = eventTemplate.BuildingClassName,
                StartTime = StartTime.Ticks,
                BuildingId = BuildingId,
                BuildingName = BuildingName,
                AttendeesCount = attendeesCount,
            };
        }

        /// <summary>Calculates the city event duration.</summary>
        /// <returns>This city event duration in hours.</returns>
        protected override float GetDuration()
        {
            return (float)eventTemplate.Duration;
        }

        private static bool CheckAge(Citizen.AgeGroup age, CityEventAttendees attendees, float randomPercentage)
        {
            switch (age)
            {
                case Citizen.AgeGroup.Child:
                    return randomPercentage < attendees.Children;

                case Citizen.AgeGroup.Teen:
                    return randomPercentage < attendees.Teens;

                case Citizen.AgeGroup.Young:
                    return randomPercentage < attendees.YoungAdults;

                case Citizen.AgeGroup.Adult:
                    return randomPercentage < attendees.Adults;

                case Citizen.AgeGroup.Senior:
                    return randomPercentage < attendees.Seniors;
            }

            return false;
        }

        private static bool CheckWellbeing(Citizen.Wellbeing wellbeing, CityEventAttendees attendees, float randomPercentage)
        {
            switch (wellbeing)
            {
                case Citizen.Wellbeing.VeryUnhappy:
                    return randomPercentage < attendees.VeryUnhappyWellbeing;

                case Citizen.Wellbeing.Unhappy:
                    return randomPercentage < attendees.UnhappyWellbeing;

                case Citizen.Wellbeing.Satisfied:
                    return randomPercentage < attendees.SatisfiedWellbeing;

                case Citizen.Wellbeing.Happy:
                    return randomPercentage < attendees.HappyWellbeing;

                case Citizen.Wellbeing.VeryHappy:
                    return randomPercentage < attendees.VeryHappyWellbeing;
            }

            return false;
        }

        private static bool CheckHappiness(Citizen.Happiness happiness, CityEventAttendees attendees, float randomPercentage)
        {
            switch (happiness)
            {
                case Citizen.Happiness.Bad:
                    return randomPercentage < attendees.BadHappiness;

                case Citizen.Happiness.Poor:
                    return randomPercentage < attendees.PoorHappiness;

                case Citizen.Happiness.Good:
                    return randomPercentage < attendees.GoodHappiness;

                case Citizen.Happiness.Excellent:
                    return randomPercentage < attendees.ExcellentHappiness;

                case Citizen.Happiness.Suberb:
                    return randomPercentage < attendees.SuperbHappiness;
            }

            return false;
        }

        private static bool CheckGender(Citizen.Gender gender, CityEventAttendees attendees, float randomPercentage)
        {
            switch (gender)
            {
                case Citizen.Gender.Female:
                    return randomPercentage < attendees.Females;

                case Citizen.Gender.Male:
                    return randomPercentage < attendees.Males;
            }

            return false;
        }

        private static bool CheckEducation(Citizen.Education education, CityEventAttendees attendees, float randomPercentage)
        {
            switch (education)
            {
                case Citizen.Education.Uneducated:
                    return randomPercentage < attendees.Uneducated;

                case Citizen.Education.OneSchool:
                    return randomPercentage < attendees.OneSchool;

                case Citizen.Education.TwoSchools:
                    return randomPercentage < attendees.TwoSchools;

                case Citizen.Education.ThreeSchools:
                    return randomPercentage < attendees.ThreeSchools;
            }

            return false;
        }

        private static bool CheckWealth(Citizen.Wealth wealth, CityEventAttendees attendees, float randomPercentage)
        {
            switch (wealth)
            {
                case Citizen.Wealth.Low:
                    return randomPercentage < attendees.LowWealth;

                case Citizen.Wealth.Medium:
                    return randomPercentage < attendees.MediumWealth;

                case Citizen.Wealth.High:
                    return randomPercentage < attendees.HighWealth;
            }

            return false;
        }
    }
}