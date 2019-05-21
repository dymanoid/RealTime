// <copyright file="ResidentAIConnection.cs" company="dymanoid">
//     Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.GameConnection
{
    using System;

    /// <summary>A class that incorporates the game connection to the original resident AI.</summary>
    /// <typeparam name="TAI">The type of the resident AI class.</typeparam>
    /// <typeparam name="TCitizen">The type of the citizen object.</typeparam>
    internal sealed class ResidentAIConnection<TAI, TCitizen> : HumanAIConnectionBase<TAI, TCitizen>
        where TAI : class
        where TCitizen : struct
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ResidentAIConnection{TAI, TCitizen}"/> class.
        /// </summary>
        /// <param name="doRandomMove">
        /// A method that corresponds to the AI's original <c>RandomMove</c> method.
        /// </param>
        /// <param name="findEvacuationPlace">
        /// A method that corresponds to the AI's original <c>FindEvacuationPlace</c> method.
        /// </param>
        /// <param name="findHospital">
        /// A method that corresponds to the AI's original <c>FindHospital</c> method.
        /// </param>
        /// <param name="findVisitPlace">
        /// A method that corresponds to the AI's original <c>FindVisitPlace</c> method.
        /// </param>
        /// <param name="getEntertainmentReason">
        /// A method that corresponds to the AI's original <c>GetEntertainmentReason</c> method.
        /// </param>
        /// <param name="getEvacuationReason">
        /// A method that corresponds to the AI's original <c>GetEvacuationReason</c> method.
        /// </param>
        /// <param name="getShoppingReason">
        /// A method that corresponds to the AI's original <c>GetShoppingReason</c> method.
        /// </param>
        /// <param name="startMoving">
        /// A method that corresponds to the AI's original <c>StartMoving</c> method specifying a
        /// target building ID.
        /// </param>
        /// <param name="startMovingWithOffer">
        /// A method that corresponds to the AI's original <c>StartMoving</c> method specifying a
        /// transfer offer.
        /// </param>
        /// <param name="attemptAutodidact">
        /// A method that corresponds to the AI's original <c>AttemptAutodidact</c> method
        /// that updates the citizen's education level after visiting a library.
        /// </param>
        /// <exception cref="ArgumentNullException">Thrown when any argument is null.</exception>
        public ResidentAIConnection(
            DoRandomMoveDelegate doRandomMove,
            FindEvacuationPlaceDelegate findEvacuationPlace,
            FindHospitalDelegate findHospital,
            FindVisitPlaceDelegate findVisitPlace,
            GetEntertainmentReasonDelegate getEntertainmentReason,
            GetEvacuationReasonDelegate getEvacuationReason,
            GetShoppingReasonDelegate getShoppingReason,
            StartMovingDelegate startMoving,
            StartMovingWithOfferDelegate startMovingWithOffer,
            AttemptAutodidactDelegate attemptAutodidact)
        : base(doRandomMove, findEvacuationPlace, findVisitPlace, getEntertainmentReason, getEvacuationReason, getShoppingReason, startMoving)
        {
            FindHospital = findHospital ?? throw new ArgumentNullException(nameof(findHospital));
            StartMovingWithOffer = startMovingWithOffer ?? throw new ArgumentNullException(nameof(startMovingWithOffer));
            AttemptAutodidact = attemptAutodidact ?? throw new ArgumentNullException(nameof(attemptAutodidact));
        }

        /// <summary>
        /// Represents the method that corresponds to the AI's original <c>FindHospital</c> method.
        /// </summary>
        /// <param name="instance">The AI instance the method is called on.</param>
        /// <param name="citizenId">The citizen ID to process.</param>
        /// <param name="sourceBuilding">
        /// The ID of the building the citizen is currently located in.
        /// </param>
        /// <param name="reason">The transfer reason for the citizen.</param>
        /// <returns>
        /// <c>true</c> if a hospital building was found and the transfer beings; otherwise, <c>false</c>.
        /// </returns>
        public delegate bool FindHospitalDelegate(TAI instance, uint citizenId, ushort sourceBuilding, TransferManager.TransferReason reason);

        /// <summary>
        /// Represents the method that corresponds to the AI's original <c>StartMoving</c> method
        /// specifying a transfer offer.
        /// </summary>
        /// <param name="instance">The AI instance the method is called on.</param>
        /// <param name="citizenId">The citizen ID to process.</param>
        /// <param name="citizen">The citizen object to process.</param>
        /// <param name="sourceBuilding">
        /// The ID of the building the citizen is currently located in.
        /// </param>
        /// <param name="offer">The transfer offer for the movement.</param>
        /// <returns>
        /// <c>true</c> if the citizen started moving to the target building; otherwise, <c>false</c>.
        /// </returns>
        public delegate bool StartMovingWithOfferDelegate(TAI instance, uint citizenId, ref TCitizen citizen, ushort sourceBuilding, TransferManager.TransferOffer offer);

        /// <summary>
        /// Represents the method that corresponds to the AI's original <c>AttemptAutodidact</c> method
        /// that updates the citizen's education level after visiting a library.
        /// </summary>
        /// <param name="citizen">The citizen object to process.</param>
        /// <param name="visitedBuildingType">The type of the building the citizen leaves.</param>
        public delegate void AttemptAutodidactDelegate(ref TCitizen citizen, ItemClass.Service visitedBuildingType);

        /// <summary>Gets a method that calls a <see cref="FindHospitalDelegate"/>.</summary>
        public FindHospitalDelegate FindHospital { get; }

        /// <summary>Gets a method that calls a <see cref="StartMovingWithOfferDelegate"/>.</summary>
        public StartMovingWithOfferDelegate StartMovingWithOffer { get; }

        /// <summary>Gets a method that calls a <see cref="AttemptAutodidactDelegate"/>.</summary>
        public AttemptAutodidactDelegate AttemptAutodidact { get; }
    }
}