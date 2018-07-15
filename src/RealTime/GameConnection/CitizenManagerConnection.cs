// <copyright file="CitizenManagerConnection.cs" company="dymanoid">Copyright (c) dymanoid. All rights reserved.</copyright>

namespace RealTime.GameConnection
{
    using UnityEngine;

    /// <summary>The default implementation of the <see cref="ICitizenManagerConnection"/> interface.</summary>
    /// <seealso cref="ICitizenManagerConnection"/>
    internal sealed class CitizenManagerConnection : ICitizenManagerConnection
    {
        /// <summary>Releases the specified citizen.</summary>
        /// <param name="citizenId">The ID of the citizen to release.</param>
        public void ReleaseCitizen(uint citizenId)
        {
            CitizenManager.instance.ReleaseCitizen(citizenId);
        }

        /// <summary>Gets the ID of the building this citizen is currently moving to.</summary>
        /// <param name="instanceId">The citizen's instance ID.</param>
        /// <returns>The ID of the building the citizen is moving to, or 0 if none.</returns>
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

        /// <summary>Determines whether the citizen's instance with provided ID has particular flags.</summary>
        /// <param name="instanceId">The instance ID to check.</param>
        /// <param name="flags">The flags to check.</param>
        /// <param name="all">
        /// <c>true</c> to check all flags from the provided <paramref name="flags"/>, <c>false</c> to check any flags.
        /// </param>
        /// <returns><c>true</c> if the citizen instance has the specified flags; otherwise, <c>false</c>.</returns>
        public bool InstanceHasFlags(ushort instanceId, CitizenInstance.Flags flags, bool all = false)
        {
            if (instanceId == 0)
            {
                return false;
            }

            CitizenInstance.Flags currentFlags = CitizenManager.instance.m_instances.m_buffer[instanceId].m_flags & flags;
            return all
                ? currentFlags == flags
                : currentFlags != 0;
        }

        /// <summary>Gets the current wait counter value of the citizen's instance with specified ID.</summary>
        /// <param name="instanceId">The instance ID to check.</param>
        /// <returns>The wait counter value of the citizen's instance.</returns>
        public byte GetInstanceWaitCounter(ushort instanceId)
        {
            return instanceId == 0
               ? (byte)0
               : CitizenManager.instance.m_instances.m_buffer[instanceId].m_waitCounter;
        }

        /// <summary>
        /// Determines whether the area around the citizen's instance with specified ID is currently marked for evacuation.
        /// </summary>
        /// <param name="instanceId">The ID of the citizen's instance to check.</param>
        /// <returns><c>true</c> if the area around the citizen's instance is marked for evacuation; otherwise, <c>false</c>.</returns>
        public bool IsAreaEvacuating(ushort instanceId)
        {
            if (instanceId == 0)
            {
                return false;
            }

            Vector3 position = CitizenManager.instance.m_instances.m_buffer[instanceId].GetLastFramePosition();
            return DisasterManager.instance.IsEvacuating(position);
        }

        /// <summary>Modifies the goods storage in the specified unit.</summary>
        /// <param name="unitId">The unit ID to process.</param>
        /// <param name="amount">The amount to modify the storage by.</param>
        /// <returns><c>true</c> on success; otherwise, <c>false</c>.</returns>
        public bool ModifyUnitGoods(uint unitId, ushort amount)
        {
            if (unitId == 0)
            {
                return false;
            }

            CitizenManager.instance.m_units.m_buffer[unitId].m_goods += amount;
            return true;
        }

        /// <summary>Gets the count of the currently active citizens instances.</summary>
        /// <returns>The number of active citizens instances.</returns>
        public uint GetInstancesCount()
        {
            return CitizenManager.instance.m_instances.ItemCount();
        }

        /// <summary>Gets the maximum count of the active citizens instances.</summary>
        /// <returns>The maximum number of active citizens instances.</returns>
        public uint GetMaxInstancesCount()
        {
            return CitizenManager.instance.m_instances.m_size;
        }

        /// <summary>Gets the maximum count of the citizens.</summary>
        /// <returns>The maximum number of the citizens.</returns>
        public uint GetMaxCitizensCount()
        {
            return CitizenManager.instance.m_citizens.m_size;
        }
    }
}