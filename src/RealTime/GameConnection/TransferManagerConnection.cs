// <copyright file="TransferManagerConnection.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.GameConnection
{
    internal sealed class TransferManagerConnection : ITransferManagerConnection
    {
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

            UnityEngine.Vector3 position = CitizenManager.instance.m_instances.m_buffer[instanceId].GetLastFramePosition();

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
