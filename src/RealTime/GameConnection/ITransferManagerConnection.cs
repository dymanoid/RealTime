// <copyright file="ITransferManagerConnection.cs" company="dymanoid">
//     Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.GameConnection
{
    /// <summary>An interface for the game logic related to the transfer management.</summary>
    internal interface ITransferManagerConnection
    {
        /// <summary>
        /// Adds an outgoing transfer offer that originates from the current position of a citizen
        /// with specified ID.
        /// </summary>
        /// <param name="citizenId">
        /// The ID of the citizen whose position is the transfer offer origin.
        /// </param>
        /// <param name="reason">The transfer reason for the offer.</param>
        void AddOutgoingOfferFromCurrentPosition(uint citizenId, TransferManager.TransferReason reason);
    }
}