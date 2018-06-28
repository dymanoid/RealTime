// <copyright file="ResidentAIConnection.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.GameConnection
{
    using System;

    /// <summary>
    /// A class that incorporates the game connection to the original resident AI.
    /// </summary>
    /// <typeparam name="TAI">The type of the resident AI.</typeparam>
    /// <typeparam name="TCitizen">The type of the citizen object.</typeparam>
    internal sealed class ResidentAIConnection<TAI, TCitizen>
        where TAI : class
        where TCitizen : struct
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

        public delegate bool DoRandomMoveDelegate(TAI instance);

        public delegate void FindEvacuationPlaceDelegate(TAI instance, uint citizenId, ushort sourceBuilding, TransferManager.TransferReason reason);

        public delegate bool FindHospitalDelegate(TAI instance, uint citizenId, ushort sourceBuilding, TransferManager.TransferReason reason);

        public delegate void FindVisitPlaceDelegate(TAI instance, uint citizenId, ushort sourceBuilding, TransferManager.TransferReason reason);

        public delegate TransferManager.TransferReason GetEntertainmentReasonDelegate(TAI instance);

        public delegate TransferManager.TransferReason GetEvacuationReasonDelegate(TAI instance, ushort sourceBuilding);

        public delegate TransferManager.TransferReason GetShoppingReasonDelegate(TAI instance);

        public delegate bool StartMovingDelegate(TAI instance, uint citizenId, ref TCitizen citizen, ushort sourceBuilding, ushort targetBuilding);

        public delegate bool StartMovingWithOfferDelegate(TAI instance, uint citizenId, ref TCitizen citizen, ushort sourceBuilding, TransferManager.TransferOffer offer);

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