// <copyright file="RealTimeResidentAI.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.AI
{
    using System;
    using System.Runtime.CompilerServices;
    using ColossalFramework;
    using RealTime.Tools;
    using Redirection;

    internal static partial class RealTimeResidentAI
    {
        private const string RedirectNeededMessage = "This method must be redirected to the original implementation";
        private static References references;

        internal static ILogic Logic { get; set; }

        [RedirectFrom(typeof(ResidentAI))]
        private static void UpdateLocation(ResidentAI instance, uint citizenId, ref Citizen citizen)
        {
            if (references == null)
            {
                CitizenManager citizenMgr = Singleton<CitizenManager>.instance;
                BuildingManager buildingMgr = Singleton<BuildingManager>.instance;
                SimulationManager simMgr = Singleton<SimulationManager>.instance;
                references = new References(instance, citizenMgr, buildingMgr, simMgr);
            }

            if (citizen.m_homeBuilding == 0 && citizen.m_workBuilding == 0 && citizen.m_visitBuilding == 0
                && citizen.m_instance == 0 && citizen.m_vehicle == 0)
            {
                references.CitizenMgr.ReleaseCitizen(citizenId);
                return;
            }

            if (citizen.Collapsed)
            {
                return;
            }

            if (citizen.Dead)
            {
                ProcessCitizenDead(references, citizenId, ref citizen);
                return;
            }

            if ((citizen.Sick && ProcessCitizenSick(references, citizenId, ref citizen))
                || (citizen.Arrested && ProcessCitizenArrested(references, ref citizen)))
            {
                return;
            }

            CitizenState citizenState = GetCitizenState(references, ref citizen);

            switch (citizenState)
            {
                case CitizenState.LeftCity:
                    references.CitizenMgr.ReleaseCitizen(citizenId);
                    break;

                case CitizenState.MovingHome:
                    ProcessCitizenMoving(references, citizenId, ref citizen, false);
                    break;

                case CitizenState.AtHome:
                    ProcessCitizenAtHome(references, citizenId, ref citizen);
                    break;

                case CitizenState.MovingToTarget:
                    ProcessCitizenMoving(references, citizenId, ref citizen, true);
                    break;

                case CitizenState.AtSchoolOrWork:
                    ProcessCitizenAtSchoolOrWork(references, citizenId, ref citizen);
                    break;

                case CitizenState.AtLunch:
                case CitizenState.Shopping:
                case CitizenState.AtLeisureArea:
                case CitizenState.Visiting:
                    ProcessCitizenVisit(citizenState, references, citizenId, ref citizen);
                    break;

                case CitizenState.Evacuating:
                    ProcessCitizenEvacuation(instance, citizenId, ref citizen);
                    break;
            }
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
    }
}
