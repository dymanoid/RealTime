// <copyright file="CitizenManagerConnection.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.GameConnection
{
    internal sealed class CitizenManagerConnection : ICitizenManagerConnection
    {
        public void ReleaseCitizen(uint citizenId)
        {
            CitizenManager.instance.ReleaseCitizen(citizenId);
        }

        public ushort GetTargetBuilding(ushort instanceId)
        {
            if (instanceId == 0)
            {
                return 0;
            }

            ref CitizenInstance instance = ref CitizenManager.instance.m_instances.m_buffer[instanceId];
            return (instance.m_flags & CitizenInstance.Flags.TargetIsNode) == 0
                ? instance.m_targetBuilding
                : (ushort)0;
        }

        public CitizenInstance.Flags GetInstanceFlags(ushort instanceId)
        {
            return instanceId == 0
               ? CitizenInstance.Flags.None
               : CitizenManager.instance.m_instances.m_buffer[instanceId].m_flags;
        }

        public byte GetInstanceWaitCounter(ushort instanceId)
        {
            return instanceId == 0
               ? (byte)0
               : CitizenManager.instance.m_instances.m_buffer[instanceId].m_waitCounter;
        }
    }
}