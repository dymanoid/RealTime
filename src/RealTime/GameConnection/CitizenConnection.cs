// <copyright file="CitizenConnection.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.GameConnection
{
    internal sealed class CitizenConnection : ICitizenConnection<Citizen>
    {
        public Citizen.Flags AddFlags(ref Citizen citizen, Citizen.Flags flags)
        {
            return citizen.m_flags |= flags;
        }

        public Citizen.AgeGroup GetAge(ref Citizen citizen)
        {
            return Citizen.GetAgeGroup(citizen.m_age);
        }

        public ushort GetCurrentBuilding(ref Citizen citizen)
        {
            return citizen.GetBuildingByLocation();
        }

        public Citizen.Flags GetFlags(ref Citizen citizen)
        {
            return citizen.m_flags;
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
            return citizen.m_flags &= ~flags;
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
