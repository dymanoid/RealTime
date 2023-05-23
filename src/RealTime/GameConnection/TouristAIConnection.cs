// <copyright file="TouristAIConnection.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.GameConnection
{
    using System;

    /// <summary>A class that incorporates the game connection to the original tourist AI.</summary>
    /// <typeparam name="TAI">The type of the tourist AI class.</typeparam>
    /// <typeparam name="TCitizen">The type of the citizen object.</typeparam>
    internal sealed class TouristAIConnection<TAI, TCitizen> : HumanAIConnectionBase<TAI, TCitizen>
        where TAI : class
        where TCitizen : struct
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TouristAIConnection{TAI, TCitizen}" /> class.
        /// </summary>
        /// <param name="getRandomTargetType">A method that corresponds to the AI's original <c>GetRandomTargetType</c> method.</param>
        /// <param name="getLeavingReason">A method that corresponds to the AI's original <c>GetLeavingReason</c> method.</param>
        /// <param name="addTouristVisit">A method that corresponds to the AI's original <c>AddTouristVisit</c> method.</param>
        /// <param name="doRandomMove">A method that corresponds to the AI's original <c>RandomMove</c> method.</param>
        /// <param name="findEvacuationPlace">A method that corresponds to the AI's original <c>FindEvacuationPlace</c> method.</param>
        /// <param name="findVisitPlace">A method that corresponds to the AI's original <c>FindVisitPlace</c> method.</param>
        /// <param name="getEntertainmentReason">A method that corresponds to the AI's original <c>GetEntertainmentReason</c> method.</param>
        /// <param name="getEvacuationReason">A method that corresponds to the AI's original <c>GetEvacuationReason</c> method.</param>
        /// <param name="getShoppingReason">A method that corresponds to the AI's original <c>GetShoppingReason</c> method.</param>
        /// <param name="startMoving">A method that corresponds to the AI's original <c>StartMoving</c> method specifying a
        /// target building ID.</param>
        /// <exception cref="ArgumentNullException">Thrown when any argument is null.</exception>
        public TouristAIConnection(
            GetRandomTargetTypeDelegate getRandomTargetType,
            GetLeavingReasonDelegate getLeavingReason,
            AddTouristVisitDelegate addTouristVisit,
            DoRandomMoveDelegate doRandomMove,
            FindEvacuationPlaceDelegate findEvacuationPlace,
            FindVisitPlaceDelegate findVisitPlace,
            GetEntertainmentReasonDelegate getEntertainmentReason,
            GetEvacuationReasonDelegate getEvacuationReason,
            GetShoppingReasonDelegate getShoppingReason,
            StartMovingDelegate startMoving)
            : base(doRandomMove, findEvacuationPlace, findVisitPlace, getEntertainmentReason, getEvacuationReason, getShoppingReason, startMoving)
        {
            GetRandomTargetType = getRandomTargetType ?? throw new ArgumentNullException(nameof(getRandomTargetType));
            GetLeavingReason = getLeavingReason ?? throw new ArgumentNullException(nameof(getLeavingReason));
            AddTouristVisit = addTouristVisit ?? throw new ArgumentNullException(nameof(addTouristVisit));
        }

        /// <summary>
        /// Represents the method that corresponds to the AI's original <c>GetRandomTargetType</c> method.
        /// </summary>
        /// <param name="instance">The AI instance the method is called on.</param>
        /// <param name="doNothingProbability">A value that specified a probability that the citizen will do nothing.</param>
        /// <returns>A value specifying the citizen's next action: 0 for idle, 1 for leaving the city, 2 for shopping, 3 for entertainment.</returns>
        public delegate TouristAI.Target GetRandomTargetTypeDelegate(TAI instance, int doNothingProbability, ref Citizen data);

        /// <summary>
        /// Represents the method that corresponds to the AI's original <c>GetLeavingReason</c> method.
        /// </summary>
        /// <param name="instance">The AI instance the method is called on.</param>
        /// <param name="citizenId">The citizen ID to process.</param>
        /// <param name="citizen">The citizen object to process.</param>
        /// <returns>
        /// The randomly selected <see cref="TransferManager.TransferReason"/> for leaving the city.
        /// </returns>
        public delegate TransferManager.TransferReason GetLeavingReasonDelegate(TAI instance, uint citizenId, ref TCitizen citizen);

        /// <summary>
        /// Represents the method that corresponds to the AI's original <c>AddTouristVisit</c> method.
        /// </summary>
        /// <param name="instance">The AI instance the method is called on.</param>
        /// <param name="citizenId">The citizen ID to process.</param>
        /// <param name="buildingId">The ID of the building to add a tourist visit to.</param>
        public delegate void AddTouristVisitDelegate(TAI instance, uint citizenId, ushort buildingId);

        /// <summary>Gets a method that calls a <see cref="GetRandomTargetTypeDelegate"/>.</summary>
        public GetRandomTargetTypeDelegate GetRandomTargetType { get; }

        /// <summary>Gets a method that calls a <see cref="GetLeavingReasonDelegate"/>.</summary>
        public GetLeavingReasonDelegate GetLeavingReason { get; }

        /// <summary>Gets a method that calls a <see cref="AddTouristVisitDelegate"/>.</summary>
        public AddTouristVisitDelegate AddTouristVisit { get; }
    }
}
