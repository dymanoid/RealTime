// <copyright file="ICitizenManagerConnection.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.GameConnection
{
    internal interface ICitizenManagerConnection
    {
        void ReleaseCitizen(uint citizenId);

        ushort GetTargetBuilding(ushort instanceId);

        CitizenInstance.Flags GetInstanceFlags(ushort instanceId);

        byte GetInstanceWaitCounter(ushort instanceId);
    }
}