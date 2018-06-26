// <copyright file="ICitizenConnection.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.GameConnection
{
    internal interface ICitizenConnection<T>
        where T : struct
    {
        ushort GetHomeBuilding(ref T citizen);

        ushort GetWorkBuilding(ref T citizen);

        ushort GetVisitBuilding(ref T citizen);

        void SetVisitBuilding(ref T citizen, ushort visitBuilding);

        ushort GetInstance(ref T citizen);

        ushort GetVehicle(ref T citizen);

        bool IsCollapsed(ref T citizen);

        bool IsDead(ref T citizen);

        bool IsSick(ref T citizen);

        bool IsArrested(ref T citizen);

        void SetArrested(ref T citizen, bool isArrested);

        ushort GetCurrentBuilding(ref T citizen);

        Citizen.Location GetLocation(ref T citizen);

        void SetLocation(ref T citizen, Citizen.Location location);

        Citizen.AgeGroup GetAge(ref T citizen);

        Citizen.Wealth GetWealthLevel(ref T citizen);

        Citizen.Education GetEducationLevel(ref T citizen);

        Citizen.Gender GetGender(uint citizenId);

        Citizen.Happiness GetHappinessLevel(ref T citizen);

        Citizen.Wellbeing GetWellbeingLevel(ref T citizen);

        bool HasFlags(ref T citizen, Citizen.Flags flags);

        void SetHome(ref T citizen, uint citizenId, ushort buildingId);

        void SetWorkplace(ref T citizen, uint citizenId, ushort buildingId);

        void SetVisitPlace(ref T citizen, uint citizenId, ushort buildingId);

        Citizen.Flags AddFlags(ref T citizen, Citizen.Flags flags);

        Citizen.Flags RemoveFlags(ref T citizen, Citizen.Flags flags);
    }
}
