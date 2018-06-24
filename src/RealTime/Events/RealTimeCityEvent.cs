// <copyright file="RealTimeCityEvent.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.Events
{
    using System;
    using System.Linq;
    using ColossalFramework.Math;
    using RealTime.Events.Storage;

    internal sealed class RealTimeCityEvent : CityEventBase
    {
        private readonly CityEventTemplate eventTemplate;

        private readonly int attendChanceAdjustment;

        private int attendeesCount;

        public RealTimeCityEvent(CityEventTemplate eventTemplate)
        {
            this.eventTemplate = eventTemplate ?? throw new ArgumentNullException(nameof(eventTemplate));
            var incentives = eventTemplate.Incentives?.Where(i => i.ActiveWhenRandomEvent).ToList();
            if (incentives != null)
            {
                attendChanceAdjustment = incentives.Sum(i => i.PositiveEffect) - incentives.Sum(i => i.NegativeEffect);
            }
        }

        public RealTimeCityEvent(CityEventTemplate eventTemplate, int attendeesCount)
            : this(eventTemplate)
        {
            this.attendeesCount = attendeesCount;
        }

        public override bool TryAcceptAttendee(
            Citizen.AgeGroup age,
            Citizen.Gender gender,
            Citizen.Education education,
            Citizen.Wealth wealth,
            Citizen.Wellbeing wellbeing,
            Citizen.Happiness happiness,
            ref Randomizer randomizer)
        {
            if (attendeesCount > eventTemplate.Capacity)
            {
                return false;
            }

            if (eventTemplate.Costs != null && eventTemplate.Costs.Entry > GetCitizenBudgetForEvent(wealth, ref randomizer))
            {
                return false;
            }

            CityEventAttendees attendees = eventTemplate.Attendees;
            float chanceAdjustment = 1f + (attendChanceAdjustment / 100f);

            float randomPercentage = randomizer.Int32(100u) / chanceAdjustment;

            if (!CheckAge(age, attendees, randomPercentage))
            {
                return false;
            }

            randomPercentage = randomizer.Int32(100u) / chanceAdjustment;
            if (!CheckGender(gender, attendees, randomPercentage))
            {
                return false;
            }

            randomPercentage = randomizer.Int32(100u) / chanceAdjustment;
            if (!CheckEducation(education, attendees, randomPercentage))
            {
                return false;
            }

            randomPercentage = randomizer.Int32(100u) / chanceAdjustment;
            if (!CheckWealth(wealth, attendees, randomPercentage))
            {
                return false;
            }

            randomPercentage = randomizer.Int32(100u) / chanceAdjustment;
            if (!CheckWellbeing(wellbeing, attendees, randomPercentage))
            {
                return false;
            }

            randomPercentage = randomizer.Int32(100u) / chanceAdjustment;
            if (!CheckHappiness(happiness, attendees, randomPercentage))
            {
                return false;
            }

            attendeesCount++;
            return true;
        }

        public RealTimeEventStorage GetStorageData()
        {
            return new RealTimeEventStorage
            {
                EventName = eventTemplate.EventName,
                BuildingClassName = eventTemplate.BuildingClassName,
                StartTime = StartTime.Ticks,
                BuildingId = BuildingId,
                BuildingName = BuildingName,
                AttendeesCount = attendeesCount
            };
        }

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
