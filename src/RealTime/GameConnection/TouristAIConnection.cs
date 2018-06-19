// <copyright file="TouristAIConnection.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.GameConnection
{
    using System;

    internal sealed class TouristAIConnection<TAI, TCitizen>
        where TAI : class
        where TCitizen : struct
    {
        public TouristAIConnection(
            GetRandomTargetTypeDelegate getRandomTargetType,
            GetLeavingReasonDelegate getLeavingReason,
            AddTouristVisitDelegate addTouristVisit,
            DoRandomMoveDelegate doRandomMove,
            FindEvacuationPlaceDelegate findEvacuationPlace,
            FindVisitPlaceDelegate findVisitPlace,
            GetEntertainmentReasonDelegate getEntertainmentReason,
            GetEvacuationReasonDelegate getEvacuationReason,
            GetShoppingReasonDelegate getShoppingReason)
        {
            GetRandomTargetType = getRandomTargetType ?? throw new ArgumentNullException(nameof(getRandomTargetType));
            GetLeavingReason = getLeavingReason ?? throw new ArgumentNullException(nameof(getLeavingReason));
            AddTouristVisit = addTouristVisit ?? throw new ArgumentNullException(nameof(addTouristVisit));
            DoRandomMove = doRandomMove ?? throw new ArgumentNullException(nameof(doRandomMove));
            FindEvacuationPlace = findEvacuationPlace ?? throw new ArgumentNullException(nameof(findEvacuationPlace));
            FindVisitPlace = findVisitPlace ?? throw new ArgumentNullException(nameof(findVisitPlace));
            GetEntertainmentReason = getEntertainmentReason ?? throw new ArgumentNullException(nameof(getEntertainmentReason));
            GetEvacuationReason = getEvacuationReason ?? throw new ArgumentNullException(nameof(getEvacuationReason));
            GetShoppingReason = getShoppingReason ?? throw new ArgumentNullException(nameof(getShoppingReason));
        }

        public delegate int GetRandomTargetTypeDelegate(TAI instance, int doNothingProbability);

        public delegate TransferManager.TransferReason GetLeavingReasonDelegate(TAI instance, uint citizenId, ref TCitizen citizen);

        public delegate void AddTouristVisitDelegate(TAI instance, uint citizenID, ushort buildingId);

        public delegate bool DoRandomMoveDelegate(TAI instance);

        public delegate void FindEvacuationPlaceDelegate(TAI instance, uint citizenId, ushort sourceBuilding, TransferManager.TransferReason reason);

        public delegate void FindVisitPlaceDelegate(TAI instance, uint citizenId, ushort sourceBuilding, TransferManager.TransferReason reason);

        public delegate TransferManager.TransferReason GetEntertainmentReasonDelegate(TAI instance);

        public delegate TransferManager.TransferReason GetEvacuationReasonDelegate(TAI instance, ushort sourceBuilding);

        public delegate TransferManager.TransferReason GetShoppingReasonDelegate(TAI instance);

        public GetRandomTargetTypeDelegate GetRandomTargetType { get; }

        public GetLeavingReasonDelegate GetLeavingReason { get; }

        public AddTouristVisitDelegate AddTouristVisit { get; }

        public DoRandomMoveDelegate DoRandomMove { get; }

        public FindEvacuationPlaceDelegate FindEvacuationPlace { get; }

        public FindVisitPlaceDelegate FindVisitPlace { get; }

        public GetEntertainmentReasonDelegate GetEntertainmentReason { get; }

        public GetEvacuationReasonDelegate GetEvacuationReason { get; }

        public GetShoppingReasonDelegate GetShoppingReason { get; }
    }
}