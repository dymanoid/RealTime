// <copyright file="ResidentAIHook.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.GameConnection
{
    using System;
    using System.Runtime.CompilerServices;
    using RealTime.CustomAI;
    using Redirection;

    internal static class ResidentAIHook
    {
        private const string RedirectNeededMessage = "This method must be redirected to the original game implementation";

        internal static RealTimeResidentAI<ResidentAI, Citizen> RealTimeAI { get; set; }

        internal static ResidentAIConnection<ResidentAI, Citizen> GetResidentAIConnection()
        {
            return new ResidentAIConnection<ResidentAI, Citizen>(
                DoRandomMove,
                FindEvacuationPlace,
                FindHospital,
                FindVisitPlace,
                GetEntertainmentReason,
                GetEvacuationReason,
                GetShoppingReason,
                StartMoving,
                StartMoving);
        }

        [RedirectFrom(typeof(ResidentAI))]
        private static void UpdateLocation(ResidentAI instance, uint citizenId, ref Citizen citizen)
        {
            RealTimeAI?.UpdateLocation(instance, citizenId, ref citizen);
        }

        [RedirectTo(typeof(ResidentAI))]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static bool FindHospital(ResidentAI instance, uint citizenId, ushort sourceBuilding, TransferManager.TransferReason reason)
        {
            throw new InvalidOperationException(RedirectNeededMessage);
        }

        [RedirectTo(typeof(ResidentAI))]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void FindEvacuationPlace(ResidentAI instance, uint citizenId, ushort sourceBuilding, TransferManager.TransferReason reason)
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
        private static void FindVisitPlace(ResidentAI instance, uint citizenId, ushort sourceBuilding, TransferManager.TransferReason reason)
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

        [RedirectTo(typeof(HumanAI))]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static bool StartMoving(ResidentAI instance, uint citizenId, ref Citizen citizen, ushort sourceBuilding, ushort targetBuilding)
        {
            throw new InvalidOperationException(RedirectNeededMessage);
        }

        [RedirectTo(typeof(HumanAI))]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static bool StartMoving(ResidentAI instance, uint citizenId, ref Citizen citizen, ushort sourceBuilding, TransferManager.TransferOffer offer)
        {
            throw new InvalidOperationException(RedirectNeededMessage);
        }
    }
}
