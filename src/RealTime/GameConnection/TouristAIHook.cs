// <copyright file="TouristAIHook.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.GameConnection
{
    using System;
    using System.Runtime.CompilerServices;
    using RealTime.CustomAI;
    using Redirection;

    internal static class TouristAIHook
    {
        private const string RedirectNeededMessage = "This method must be redirected to the original game implementation";

        internal static RealTimeTouristAI<TouristAI, Citizen> RealTimeAI { get; set; }

        internal static TouristAIConnection<TouristAI, Citizen> GetTouristAIConnection()
        {
            return new TouristAIConnection<TouristAI, Citizen>(
                GetRandomTargetType,
                GetLeavingReason,
                AddTouristVisit,
                DoRandomMove,
                FindEvacuationPlace,
                FindVisitPlace,
                GetEntertainmentReason,
                GetEvacuationReason,
                GetShoppingReason,
                StartMoving);
        }

        [RedirectFrom(typeof(TouristAI))]
        private static void UpdateLocation(TouristAI instance, uint citizenId, ref Citizen citizen)
        {
            RealTimeAI?.UpdateLocation(instance, citizenId, ref citizen);
        }

        [RedirectTo(typeof(TouristAI))]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static int GetRandomTargetType(TouristAI instance, int doNothingProbability)
        {
            throw new InvalidOperationException(RedirectNeededMessage);
        }

        [RedirectTo(typeof(HumanAI))]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static TransferManager.TransferReason GetLeavingReason(TouristAI instance, uint citizenId, ref Citizen citizen)
        {
            throw new InvalidOperationException(RedirectNeededMessage);
        }

        [RedirectTo(typeof(TouristAI))]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void AddTouristVisit(TouristAI instance, uint citizenId, ushort buildingId)
        {
            throw new InvalidOperationException(RedirectNeededMessage);
        }

        [RedirectTo(typeof(TouristAI))]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void FindEvacuationPlace(TouristAI instance, uint citizenId, ushort sourceBuilding, TransferManager.TransferReason reason)
        {
            throw new InvalidOperationException(RedirectNeededMessage);
        }

        [RedirectTo(typeof(TouristAI))]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static TransferManager.TransferReason GetEvacuationReason(TouristAI instance, ushort sourceBuilding)
        {
            throw new InvalidOperationException(RedirectNeededMessage);
        }

        [RedirectTo(typeof(TouristAI))]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void FindVisitPlace(TouristAI instance, uint citizenId, ushort sourceBuilding, TransferManager.TransferReason reason)
        {
            throw new InvalidOperationException(RedirectNeededMessage);
        }

        [RedirectTo(typeof(TouristAI))]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static TransferManager.TransferReason GetShoppingReason(TouristAI instance)
        {
            throw new InvalidOperationException(RedirectNeededMessage);
        }

        [RedirectTo(typeof(TouristAI))]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static bool DoRandomMove(TouristAI instance)
        {
            throw new InvalidOperationException(RedirectNeededMessage);
        }

        [RedirectTo(typeof(TouristAI))]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static TransferManager.TransferReason GetEntertainmentReason(TouristAI instance)
        {
            throw new InvalidOperationException(RedirectNeededMessage);
        }

        [RedirectTo(typeof(HumanAI))]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static bool StartMoving(TouristAI instance, uint citizenId, ref Citizen citizen, ushort sourceBuilding, ushort targetBuilding)
        {
            throw new InvalidOperationException(RedirectNeededMessage);
        }
    }
}
