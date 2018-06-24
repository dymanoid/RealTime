// <copyright file="ITransferManagerConnection.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.GameConnection
{
    internal interface ITransferManagerConnection
    {
        void AddOutgoingOfferFromCurrentPosition(uint citizenId, TransferManager.TransferReason reason);
    }
}