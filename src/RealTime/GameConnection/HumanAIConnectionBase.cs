// <copyright file="HumanAIConnectionBase.cs" company="dymanoid">Copyright (c) dymanoid. All rights reserved.</copyright>

namespace RealTime.GameConnection
{
    using System;

    /// <summary>A base class for the human AI game connection classes.</summary>
    /// <typeparam name="TAI">The type of the human AI class.</typeparam>
    /// <typeparam name="TCitizen">The type of the citizen object.</typeparam>
    internal abstract class HumanAIConnectionBase<TAI, TCitizen>
        where TAI : class
        where TCitizen : struct
    {
        /// <summary>Initializes a new instance of the <see cref="HumanAIConnectionBase{TAI, TCitizen}"/> class.</summary>
        /// <param name="doRandomMove">A method that corresponds to the AI's original <c>RandomMove</c> method.</param>
        /// <param name="findEvacuationPlace">
        /// A method that corresponds to the AI's original <c>FindEvacuationPlace</c> method.
        /// </param>
        /// <param name="findVisitPlace">A method that corresponds to the AI's original <c>FindVisitPlace</c> method.</param>
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
        /// A method that corresponds to the AI's original <c>StartMoving</c> method specifying a target building ID.
        /// </param>
        /// <exception cref="ArgumentNullException">Thrown when any argument is null.</exception>
        protected HumanAIConnectionBase(
            DoRandomMoveDelegate doRandomMove,
            FindEvacuationPlaceDelegate findEvacuationPlace,
            FindVisitPlaceDelegate findVisitPlace,
            GetEntertainmentReasonDelegate getEntertainmentReason,
            GetEvacuationReasonDelegate getEvacuationReason,
            GetShoppingReasonDelegate getShoppingReason,
            StartMovingDelegate startMoving)
        {
            DoRandomMove = doRandomMove ?? throw new ArgumentNullException(nameof(doRandomMove));
            FindEvacuationPlace = findEvacuationPlace ?? throw new ArgumentNullException(nameof(findEvacuationPlace));
            FindVisitPlace = findVisitPlace ?? throw new ArgumentNullException(nameof(findVisitPlace));
            GetEntertainmentReason = getEntertainmentReason ?? throw new ArgumentNullException(nameof(getEntertainmentReason));
            GetEvacuationReason = getEvacuationReason ?? throw new ArgumentNullException(nameof(getEvacuationReason));
            GetShoppingReason = getShoppingReason ?? throw new ArgumentNullException(nameof(getShoppingReason));
            StartMoving = startMoving ?? throw new ArgumentNullException(nameof(startMoving));
        }

        /// <summary>Represents the method that corresponds to the AI's original <c>RandomMove</c> method.</summary>
        /// <param name="instance">The AI instance the method is called on.</param>
        /// <returns><c>true</c> if a citizen can be realized to an instance; otherwise, <c>false</c>.</returns>
        public delegate bool DoRandomMoveDelegate(TAI instance);

        /// <summary>Represents the method that corresponds to the AI's original <c>FindEvacuationPlace</c> method.</summary>
        /// <param name="instance">The AI instance the method is called on.</param>
        /// <param name="citizenId">The citizen ID to process.</param>
        /// <param name="sourceBuilding">The ID of the building the citizen is currently located in.</param>
        /// <param name="reason">The transfer reason for the citizen.</param>
        public delegate void FindEvacuationPlaceDelegate(TAI instance, uint citizenId, ushort sourceBuilding, TransferManager.TransferReason reason);

        /// <summary>Represents the method that corresponds to the AI's original <c>FindVisitPlace</c> method.</summary>
        /// <param name="instance">The AI instance the method is called on.</param>
        /// <param name="citizenId">The citizen ID to process.</param>
        /// <param name="sourceBuilding">The ID of the building the citizen is currently located in.</param>
        /// <param name="reason">The transfer reason for the citizen.</param>
        public delegate void FindVisitPlaceDelegate(TAI instance, uint citizenId, ushort sourceBuilding, TransferManager.TransferReason reason);

        /// <summary>Represents the method that corresponds to the AI's original <c>GetEntertainmentReason</c> method.</summary>
        /// <param name="instance">The AI instance the method is called on.</param>
        /// <returns>The randomly selected entertainment <see cref="TransferManager.TransferReason"/>.</returns>
        public delegate TransferManager.TransferReason GetEntertainmentReasonDelegate(TAI instance);

        /// <summary>Represents the method that corresponds to the AI's original <c>GetEvacuationReason</c> method.</summary>
        /// <param name="instance">The AI instance the method is called on.</param>
        /// <param name="sourceBuilding">The ID of the building the citizen is currently located in.</param>
        /// <returns>The randomly selected evacuation <see cref="TransferManager.TransferReason"/>.</returns>
        public delegate TransferManager.TransferReason GetEvacuationReasonDelegate(TAI instance, ushort sourceBuilding);

        /// <summary>Represents the method that corresponds to the AI's original <c>GetShoppingReason</c> method.</summary>
        /// <param name="instance">The AI instance the method is called on.</param>
        /// <returns>The randomly selected shopping <see cref="TransferManager.TransferReason"/>.</returns>
        public delegate TransferManager.TransferReason GetShoppingReasonDelegate(TAI instance);

        /// <summary>
        /// Represents the method that corresponds to the AI's original <c>StartMoving</c> method specifying a target
        /// building ID.
        /// </summary>
        /// <param name="instance">The AI instance the method is called on.</param>
        /// <param name="citizenId">The citizen ID to process.</param>
        /// <param name="citizen">The citizen object to process.</param>
        /// <param name="sourceBuilding">The ID of the building the citizen is currently located in.</param>
        /// <param name="targetBuilding">The ID of the building the citizen want to move to.</param>
        /// <returns><c>true</c> if the citizen started moving to the target building; otherwise, <c>false</c>.</returns>
        public delegate bool StartMovingDelegate(TAI instance, uint citizenId, ref TCitizen citizen, ushort sourceBuilding, ushort targetBuilding);

        /// <summary>Gets a method that calls a <see cref="DoRandomMoveDelegate"/>.</summary>
        public DoRandomMoveDelegate DoRandomMove { get; }

        /// <summary>Gets a method that calls a <see cref="FindEvacuationPlaceDelegate"/>.</summary>
        public FindEvacuationPlaceDelegate FindEvacuationPlace { get; }

        /// <summary>Gets a method that calls a <see cref="FindVisitPlaceDelegate"/>.</summary>
        public FindVisitPlaceDelegate FindVisitPlace { get; }

        /// <summary>Gets a method that calls a <see cref="GetEntertainmentReasonDelegate"/>.</summary>
        public GetEntertainmentReasonDelegate GetEntertainmentReason { get; }

        /// <summary>Gets a method that calls a <see cref="GetEvacuationReasonDelegate"/>.</summary>
        public GetEvacuationReasonDelegate GetEvacuationReason { get; }

        /// <summary>Gets a method that calls a <see cref="GetShoppingReasonDelegate"/>.</summary>
        public GetShoppingReasonDelegate GetShoppingReason { get; }

        /// <summary>Gets a method that calls a <see cref="StartMovingDelegate"/>.</summary>
        public StartMovingDelegate StartMoving { get; }
    }
}