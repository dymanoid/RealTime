// <copyright file="CitizenConnection.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.GameConnection
{
    internal sealed class CitizenConnection : ICitizenConnection<Citizen>
    {
        public Citizen.Flags AddFlags(ref Citizen citizen, Citizen.Flags flags)
        {
            Citizen.Flags currentFlags = citizen.m_flags;
            currentFlags |= flags;
            return citizen.m_flags = currentFlags;
        }

        public Citizen.AgeGroup GetAge(ref Citizen citizen)
        {
            return Citizen.GetAgeGroup(citizen.m_age);
        }

        public Citizen.Wealth GetWealthLevel(ref Citizen citizen)
        {
            return citizen.WealthLevel;
        }

        public Citizen.Education GetEducationLevel(ref Citizen citizen)
        {
            return citizen.EducationLevel;
        }

        public Citizen.Gender GetGender(uint citizenId)
        {
            return Citizen.GetGender(citizenId);
        }

        public Citizen.Happiness GetHappinessLevel(ref Citizen citizen)
        {
            return Citizen.GetHappinessLevel(Citizen.GetHappiness(citizen.m_health, citizen.m_wellbeing));
        }

        public Citizen.Wellbeing GetWellbeingLevel(ref Citizen citizen)
        {
            return Citizen.GetWellbeingLevel(citizen.EducationLevel, citizen.m_wellbeing);
        }

        public ushort GetCurrentBuilding(ref Citizen citizen)
        {
            return citizen.GetBuildingByLocation();
        }

        public bool HasFlags(ref Citizen citizen, Citizen.Flags flags)
        {
            return (citizen.m_flags & flags) != 0;
        }

        public ushort GetHomeBuilding(ref Citizen citizen)
        {
            return citizen.m_homeBuilding;
        }

        public ushort GetInstance(ref Citizen citizen)
        {
            return citizen.m_instance;
        }

        public Citizen.Location GetLocation(ref Citizen citizen)
        {
            return citizen.CurrentLocation;
        }

        public ushort GetVehicle(ref Citizen citizen)
        {
            return citizen.m_vehicle;
        }

        public ushort GetVisitBuilding(ref Citizen citizen)
        {
            return citizen.m_visitBuilding;
        }

        public ushort GetWorkBuilding(ref Citizen citizen)
        {
            return citizen.m_workBuilding;
        }

        public bool IsArrested(ref Citizen citizen)
        {
            return citizen.Arrested;
        }

        public bool IsCollapsed(ref Citizen citizen)
        {
            return citizen.Collapsed;
        }

        public bool IsDead(ref Citizen citizen)
        {
            return citizen.Dead;
        }

        public bool IsSick(ref Citizen citizen)
        {
            return citizen.Sick;
        }

        public Citizen.Flags RemoveFlags(ref Citizen citizen, Citizen.Flags flags)
        {
            Citizen.Flags currentFlags = citizen.m_flags;
            currentFlags &= ~flags;
            return citizen.m_flags = currentFlags;
        }

        public void SetArrested(ref Citizen citizen, bool isArrested)
        {
            citizen.Arrested = isArrested;
        }

        public void SetHome(ref Citizen citizen, uint citizenId, ushort buildingId)
        {
            citizen.SetHome(citizenId, buildingId, 0u);
        }

        public void SetLocation(ref Citizen citizen, Citizen.Location location)
        {
            citizen.CurrentLocation = location;
        }

        public void SetVisitBuilding(ref Citizen citizen, ushort visitBuilding)
        {
            citizen.m_visitBuilding = visitBuilding;
        }

        public void SetVisitPlace(ref Citizen citizen, uint citizenId, ushort buildingId)
        {
            citizen.SetVisitplace(citizenId, buildingId, 0u);
        }

        public void SetWorkplace(ref Citizen citizen, uint citizenId, ushort buildingId)
        {
            citizen.SetWorkplace(citizenId, buildingId, 0u);
        }
    }
}
