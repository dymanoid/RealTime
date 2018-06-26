// <copyright file="ICitizenManagerConnection.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.GameConnection
{
    internal interface ICitizenManagerConnection
    {
        void ReleaseCitizen(uint citizenId);

        ushort GetTargetBuilding(ushort instanceId);

        bool InstanceHasFlags(ushort instanceId, CitizenInstance.Flags flags, bool exact = false);

        byte GetInstanceWaitCounter(ushort instanceId);

        bool IsAreaEvacuating(ushort instanceId);
    }
}