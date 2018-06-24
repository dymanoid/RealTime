// <copyright file="VanillaEvent.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.Events
{
    using ColossalFramework.Math;

    internal sealed class VanillaEvent : CityEventBase
    {
        private readonly float duration;
        private readonly float ticketPrice;

        public VanillaEvent(float duration, float ticketPrice)
        {
            this.duration = duration;
            this.ticketPrice = ticketPrice;
        }

        public ushort EventId { get; set; }

        public override bool TryAcceptAttendee(
            Citizen.AgeGroup age,
            Citizen.Gender gender,
            Citizen.Education education,
            Citizen.Wealth wealth,
            Citizen.Wellbeing wellbeing,
            Citizen.Happiness happiness,
            ref Randomizer randomizer)
        {
            return ticketPrice <= GetCitizenBudgetForEvent(wealth, ref randomizer);
        }

        protected override float GetDuration()
        {
            return duration;
        }
    }
}
