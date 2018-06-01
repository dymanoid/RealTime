// <copyright file="RealTimeResidentAI.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.AI
{
    using System;
    using System.Runtime.CompilerServices;
    using ColossalFramework;
    using Redirection;

    internal static partial class RealTimeResidentAI
    {
        private const string RedirectNeededMessage = "This method must be redirected to the original implementation";

        [RedirectFrom(typeof(ResidentAI))]
        private static void UpdateLocation(ResidentAI instance, uint citizenID, ref Citizen data)
        {
            CitizenManager citizenMgr = Singleton<CitizenManager>.instance;
            BuildingManager buildingMgr = Singleton<BuildingManager>.instance;
            SimulationManager simMgr = Singleton<SimulationManager>.instance;
            var args = new Arguments(instance, citizenMgr, buildingMgr, simMgr);

            if (data.m_homeBuilding == 0 && data.m_workBuilding == 0 && data.m_visitBuilding == 0 && data.m_instance == 0 && data.m_vehicle == 0)
            {
                citizenMgr.ReleaseCitizen(citizenID);
                return;
            }

            switch (data.CurrentLocation)
            {
                case Citizen.Location.Home:
                    if (ProcessCitizenAtHome(args, citizenID, ref data))
                    {
                        return;
                    }

                    break;

                case Citizen.Location.Work:
                    if (ProcessCitizenAtWork(args, citizenID, ref data))
                    {
                        return;
                    }

                    break;

                case Citizen.Location.Visit:
                    if (ProcessCitizenVisit(args, citizenID, ref data))
                    {
                        return;
                    }

                    break;

                case Citizen.Location.Moving:
                    if (ProcessCitizenMoving(args, citizenID, ref data))
                    {
                        return;
                    }

                    break;
            }

            data.m_flags &= ~Citizen.Flags.NeedGoods;
        }

        [RedirectTo(typeof(ResidentAI))]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static bool FindHospital(ResidentAI instance, uint citizenID, ushort sourceBuilding, TransferManager.TransferReason reason)
        {
            throw new InvalidOperationException(RedirectNeededMessage);
        }

        [RedirectTo(typeof(ResidentAI))]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void FindEvacuationPlace(ResidentAI instance, uint citizenID, ushort sourceBuilding, TransferManager.TransferReason reason)
        {
            throw new InvalidOperationException(RedirectNeededMessage);
        }

        [RedirectTo(typeof(ResidentAI))]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static TransferManager.TransferReason GetEvacuationReason(ResidentAI instance, ushort sourceBuilding)
        {
            throw new InvalidOperationException(RedirectNeededMessage);
        }

        [RedirectTo(typeof(ResidentAI))]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void FindVisitPlace(ResidentAI instance, uint citizenID, ushort sourceBuilding, TransferManager.TransferReason reason)
        {
            throw new InvalidOperationException(RedirectNeededMessage);
        }

        [RedirectTo(typeof(ResidentAI))]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static TransferManager.TransferReason GetShoppingReason(ResidentAI instance)
        {
            throw new InvalidOperationException(RedirectNeededMessage);
        }

        [RedirectTo(typeof(ResidentAI))]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static bool DoRandomMove(ResidentAI instance)
        {
            throw new InvalidOperationException(RedirectNeededMessage);
        }

        [RedirectTo(typeof(ResidentAI))]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static TransferManager.TransferReason GetEntertainmentReason(ResidentAI instance)
        {
            throw new InvalidOperationException(RedirectNeededMessage);
        }

        private sealed class Arguments
        {
            public Arguments(ResidentAI residentAI, CitizenManager citizenMgr, BuildingManager buildingMgr, SimulationManager simMgr)
            {
                ResidentAI = residentAI;
                CitizenMgr = citizenMgr;
                BuildingMgr = buildingMgr;
                SimMgr = simMgr;
            }

            public ResidentAI ResidentAI { get; }

            public CitizenManager CitizenMgr { get; }

            public BuildingManager BuildingMgr { get; }

            public SimulationManager SimMgr { get; }
        }
    }
}
