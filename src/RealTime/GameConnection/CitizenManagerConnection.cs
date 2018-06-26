// <copyright file="CitizenManagerConnection.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.GameConnection
{
    using UnityEngine;

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

        public bool InstanceHasFlags(ushort instanceId, CitizenInstance.Flags flags, bool exact = false)
        {
            if (instanceId == 0)
            {
                return false;
            }

            CitizenInstance.Flags currentFlags = CitizenManager.instance.m_instances.m_buffer[instanceId].m_flags;
            return exact
                ? currentFlags == flags
                : currentFlags != 0;
        }

        public byte GetInstanceWaitCounter(ushort instanceId)
        {
            return instanceId == 0
               ? (byte)0
               : CitizenManager.instance.m_instances.m_buffer[instanceId].m_waitCounter;
        }

        public bool IsAreaEvacuating(ushort instanceId)
        {
            if (instanceId == 0)
            {
                return false;
            }

            Vector3 position = CitizenManager.instance.m_instances.m_buffer[instanceId].GetLastFramePosition();
            return DisasterManager.instance.IsEvacuating(position);
        }
    }
}