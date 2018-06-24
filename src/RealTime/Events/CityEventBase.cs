// <copyright file="CityEventBase.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.Events
{
    using System;
    using ColossalFramework.Math;

    internal abstract class CityEventBase : ICityEvent
    {
        public DateTime StartTime { get; private set; }

        public DateTime EndTime => StartTime.AddHours(GetDuration());

        public ushort BuildingId { get; private set; }

        public string BuildingName { get; private set; }

        public virtual bool TryAcceptAttendee(
            Citizen.AgeGroup age,
            Citizen.Gender gender,
            Citizen.Education education,
            Citizen.Wealth wealth,
            Citizen.Wellbeing wellbeing,
            Citizen.Happiness happiness,
            ref Randomizer randomizer)
        {
            return true;
        }

        public void Configure(ushort buildingId, string buildingName, DateTime startTime)
        {
            BuildingId = buildingId;
            BuildingName = buildingName ?? string.Empty;
            StartTime = startTime;
        }

        protected static float GetCitizenBudgetForEvent(Citizen.Wealth wealth, ref Randomizer randomizer)
        {
            switch (wealth)
            {
                case Citizen.Wealth.Low:
                    return 30f + randomizer.Int32(60);

                case Citizen.Wealth.Medium:
                    return 80f + randomizer.Int32(80);

                case Citizen.Wealth.High:
                    return 120f + randomizer.Int32(320);

                default:
                    return 0;
            }
        }

        protected abstract float GetDuration();
    }
}
