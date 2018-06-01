// <copyright file="RealTimeResidentAI.Common.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.AI
{
    using System;

    internal static partial class RealTimeResidentAI
    {
        private static bool? ProcessCitizenCommonState(Arguments args, uint citizenID, ref Citizen data)
        {
            ushort currentBuilding = GetCurrentCitizenBuilding(ref data);

            if (data.Dead)
            {
                return ProcessCitizenDead(args, citizenID, ref data, currentBuilding);
            }

            if (data.CurrentLocation == Citizen.Location.Moving)
            {
                return null;
            }

            if (data.Arrested)
            {
                return ProcessCitizenArrested(ref data);
            }

            if (data.CurrentLocation == Citizen.Location.Visit && data.Collapsed)
            {
                return false;
            }

            if (data.CurrentLocation == Citizen.Location.Home && (data.m_homeBuilding == 0 || data.m_vehicle != 0))
            {
                return false;
            }

            if (data.Sick)
            {
                return ProcessCitizenSick(args, citizenID, ref data, currentBuilding);
            }

            return null;
        }

        private static bool ProcessCitizenDead(Arguments args, uint citizenID, ref Citizen data, ushort currentBuilding)
        {
            if (currentBuilding == 0 || (data.CurrentLocation == Citizen.Location.Moving && data.m_vehicle == 0))
            {
                args.CitizenMgr.ReleaseCitizen(citizenID);
                return true;
            }

            if (data.CurrentLocation != Citizen.Location.Home && data.m_homeBuilding != 0)
            {
                data.SetHome(citizenID, 0, 0u);
            }

            if (data.CurrentLocation != Citizen.Location.Work && data.m_workBuilding != 0)
            {
                data.SetWorkplace(citizenID, 0, 0u);
            }

            if (data.CurrentLocation != Citizen.Location.Visit && data.m_visitBuilding != 0)
            {
                data.SetVisitplace(citizenID, 0, 0u);
            }

            if (data.CurrentLocation == Citizen.Location.Moving || data.m_vehicle != 0)
            {
                return false;
            }

            if (data.CurrentLocation == Citizen.Location.Visit
                &&
                args.BuildingMgr.m_buildings.m_buffer[data.m_visitBuilding].Info.m_class.m_service == ItemClass.Service.HealthCare)
            {
                return false;
            }

            if (FindHospital(args.ResidentAI, citizenID, currentBuilding, TransferManager.TransferReason.Dead))
            {
                return false;
            }

            return true;
        }

        private static bool ProcessCitizenArrested(ref Citizen data)
        {
            if (data.CurrentLocation != Citizen.Location.Visit || data.m_visitBuilding == 0)
            {
                data.Arrested = false;
            }

            return false;
        }

        private static bool ProcessCitizenSick(Arguments args, uint citizenID, ref Citizen data, ushort currentBuilding)
        {
            if (data.CurrentLocation != Citizen.Location.Home && currentBuilding == 0)
            {
                data.CurrentLocation = Citizen.Location.Home;
                return false;
            }

            if (data.CurrentLocation != Citizen.Location.Home && data.m_vehicle != 0)
            {
                return false;
            }

            if (data.CurrentLocation == Citizen.Location.Visit)
            {
                ItemClass.Service service = args.BuildingMgr.m_buildings.m_buffer[data.m_visitBuilding].Info.m_class.m_service;
                if (service == ItemClass.Service.HealthCare || service == ItemClass.Service.Disaster)
                {
                    return false;
                }
            }

            if (FindHospital(args.ResidentAI, citizenID, currentBuilding, TransferManager.TransferReason.Sick))
            {
                return false;
            }

            return true;
        }

        private static ushort GetCurrentCitizenBuilding(ref Citizen data)
        {
            switch (data.CurrentLocation)
            {
                case Citizen.Location.Home:
                    return data.m_homeBuilding;

                case Citizen.Location.Work:
                    return data.m_workBuilding;

                case Citizen.Location.Visit:
                    return data.m_visitBuilding;

                case Citizen.Location.Moving:
                    // This value will not be used anyway
                    return ushort.MaxValue;

                default:
                    throw new NotSupportedException($"The location {data.CurrentLocation} is not supported by this method");
            }
        }
    }
}
