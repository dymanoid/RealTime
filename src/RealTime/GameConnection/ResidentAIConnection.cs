// <copyright file="ResidentAIConnection.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.GameConnection
{
    using System;

    internal sealed class ResidentAIConnection<T>
        where T : class
    {
        public ResidentAIConnection(
            DoRandomMoveDelegate doRandomMove,
            FindEvacuationPlaceDelegate findEvacuationPlace,
            FindHospitalDelegate findHospital,
            FindVisitPlaceDelegate findVisitPlace,
            GetEntertainmentReasonDelegate getEntertainmentReason,
            GetEvacuationReasonDelegate getEvacuationReason,
            GetShoppingReasonDelegate getShoppingReason,
            StartMovingDelegate startMoving,
            StartMovingWithOfferDelegate startMovingWithOffer)
        {
            DoRandomMove = doRandomMove ?? throw new ArgumentNullException(nameof(doRandomMove));
            FindEvacuationPlace = findEvacuationPlace ?? throw new ArgumentNullException(nameof(findEvacuationPlace));
            FindHospital = findHospital ?? throw new ArgumentNullException(nameof(findHospital));
            FindVisitPlace = findVisitPlace ?? throw new ArgumentNullException(nameof(findVisitPlace));
            GetEntertainmentReason = getEntertainmentReason ?? throw new ArgumentNullException(nameof(getEntertainmentReason));
            GetEvacuationReason = getEvacuationReason ?? throw new ArgumentNullException(nameof(getEvacuationReason));
            GetShoppingReason = getShoppingReason ?? throw new ArgumentNullException(nameof(getShoppingReason));
            StartMoving = startMoving ?? throw new ArgumentNullException(nameof(startMoving));
            StartMovingWithOffer = startMovingWithOffer ?? throw new ArgumentNullException(nameof(startMovingWithOffer));
        }

        public delegate bool DoRandomMoveDelegate(T instance);

        public delegate void FindEvacuationPlaceDelegate(T instance, uint citizenId, ushort sourceBuilding, TransferManager.TransferReason reason);

        public delegate bool FindHospitalDelegate(T instance, uint citizenId, ushort sourceBuilding, TransferManager.TransferReason reason);

        public delegate void FindVisitPlaceDelegate(T instance, uint citizenId, ushort sourceBuilding, TransferManager.TransferReason reason);

        public delegate TransferManager.TransferReason GetEntertainmentReasonDelegate(T instance);

        public delegate TransferManager.TransferReason GetEvacuationReasonDelegate(T instance, ushort sourceBuilding);

        public delegate TransferManager.TransferReason GetShoppingReasonDelegate(T instance);

        public delegate bool StartMovingDelegate(T instance, uint citizenID, ref Citizen data, ushort sourceBuilding, ushort targetBuilding);

        public delegate bool StartMovingWithOfferDelegate(T instance, uint citizenID, ref Citizen data, ushort sourceBuilding, TransferManager.TransferOffer offer);

        public DoRandomMoveDelegate DoRandomMove { get; }

        public FindEvacuationPlaceDelegate FindEvacuationPlace { get; }

        public FindHospitalDelegate FindHospital { get; }

        public FindVisitPlaceDelegate FindVisitPlace { get; }

        public GetEntertainmentReasonDelegate GetEntertainmentReason { get; }

        public GetEvacuationReasonDelegate GetEvacuationReason { get; }

        public GetShoppingReasonDelegate GetShoppingReason { get; }

        public StartMovingDelegate StartMoving { get; }

        public StartMovingWithOfferDelegate StartMovingWithOffer { get; }
    }
}