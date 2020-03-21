﻿// <copyright file="TransferManagerConnection.cs" company="dymanoid">
//     Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.GameConnection
{
    /// <summary>The default implementation of the <see cref="ITransferManagerConnection"/> interface.</summary>
    /// <seealso cref="ITransferManagerConnection"/>
    internal sealed class TransferManagerConnection : ITransferManagerConnection
    {
        /// <summary>
        /// Adds an outgoing transfer offer that originates from the current position of a citizen
        /// with specified ID.
        /// </summary>
        /// <param name="citizenId">
        /// The ID of the citizen whose position is the transfer offer origin.
        /// </param>
        /// <param name="reason">The transfer reason for the offer.</param>
        public void AddOutgoingOfferFromCurrentPosition(uint citizenId, TransferManager.TransferReason reason)
        {
            if (citizenId == 0)
            {
                return;
            }

            ushort instanceId = CitizenManager.instance.m_citizens.m_buffer[citizenId].m_instance;
            if (instanceId == 0)
            {
                return;
            }

            var position = CitizenManager.instance.m_instances.m_buffer[instanceId].GetLastFramePosition();

            TransferManager.TransferOffer offer = default;
            offer.Priority = SimulationManager.instance.m_randomizer.Int32(8u);
            offer.Citizen = citizenId;
            offer.Position = position;
            offer.Amount = 1;
            offer.Active = true;
            TransferManager.instance.AddOutgoingOffer(reason, offer);
        }
    }
}