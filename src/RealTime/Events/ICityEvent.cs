// <copyright file="ICityEvent.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.Events
{
    using System;
    using ColossalFramework.Math;

    internal interface ICityEvent
    {
        DateTime StartTime { get; }

        DateTime EndTime { get; }

        ushort BuildingId { get; }

        string BuildingName { get; }

        void Configure(ushort buildingId, string buildingName, DateTime startTime);

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