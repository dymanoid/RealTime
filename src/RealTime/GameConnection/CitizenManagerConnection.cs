// <copyright file="CitizenManagerConnection.cs" company="dymanoid">Copyright (c) dymanoid. All rights reserved.</copyright>

namespace RealTime.GameConnection
{
    using System;
    using UnityEngine;

    /// <summary>The default implementation of the <see cref="ICitizenManagerConnection{TCitizen}"/> interface.</summary>
    /// <seealso cref="ICitizenManagerConnection{TCitizen}"/>
    internal sealed class CitizenManagerConnection : ICitizenManagerConnection<Citizen>
    {
        /// <summary>Releases the specified citizen.</summary>
        /// <param name="citizenId">The ID of the citizen to release.</param>
        public void ReleaseCitizen(uint citizenId) => CitizenManager.instance.ReleaseCitizen(citizenId);

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

        /// <summary>Gets the ID of the game node this citizen is currently moving to.</summary>
        /// <param name="instanceId">The citizen's instance ID.</param>
        /// <returns>The ID of the game node the citizen is moving to, or 0 if none.</returns>
        public ushort GetTargetNode(ushort instanceId)
        {
            if (instanceId == 0)
            {
                return 0;
            }

            ref CitizenInstance instance = ref CitizenManager.instance.m_instances.m_buffer[instanceId];
            return (instance.m_flags & CitizenInstance.Flags.TargetIsNode) == 0
                ? (ushort)0
                : instance.m_targetBuilding;
        }

        /// <summary>Determines whether the citizen's instance with specified ID has particular flags.</summary>
        /// <param name="instanceId">The instance ID to check.</param>
        /// <param name="flags">The flags to check.</param>
        /// <param name="all">
        /// <c>true</c> to check all flags from the specified <paramref name="flags"/>, <c>false</c> to check any flags.
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

        /// <summary>Gets the current flags of the citizen's instance with specified ID.</summary>
        /// <param name="instanceId">The instance ID to check.</param>
        /// <param name="mask">The flags mask to apply for the check.</param>
        /// <returns>The citizen instance flags masked by the specified <paramref name="mask"/>.</returns>
        public CitizenInstance.Flags GetInstanceFlags(ushort instanceId, CitizenInstance.Flags mask)
        {
            return instanceId == 0
                ? CitizenInstance.Flags.None
                : CitizenManager.instance.m_instances.m_buffer[instanceId].m_flags & mask;
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

        /// <summary>Gets the citizen instance's current position.</summary>
        /// <param name="instanceId">The ID of the citizen's instance to get the position of.</param>
        /// <returns>A <see cref="Vector3"/> that specifies the instance position.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="instanceId"/> is 0.</exception>
        public Vector3 GetCitizenPosition(ushort instanceId)
        {
            if (instanceId == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(instanceId), "The citizen instance ID cannot be 0");
            }

            return CitizenManager.instance.m_instances.m_buffer[instanceId].GetLastFramePosition();
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
        public uint GetInstancesCount() => CitizenManager.instance.m_instances.ItemCount();

        /// <summary>Gets the maximum count of the active citizens instances.</summary>
        /// <returns>The maximum number of active citizens instances.</returns>
        public uint GetMaxInstancesCount() => CitizenManager.instance.m_instances.m_size;

        /// <summary>Gets the maximum count of the citizens.</summary>
        /// <returns>The maximum number of the citizens.</returns>
        public uint GetMaxCitizensCount() => CitizenManager.instance.m_citizens.m_size;

        /// <summary>Gets the location of the citizen with specified ID.</summary>
        /// <param name="citizenId">The ID of the citizen to query location of.</param>
        /// <returns>A <see cref="Citizen.Location"/> value that describes the citizen's current location.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the argument is 0.</exception>
        public Citizen.Location GetCitizenLocation(uint citizenId)
        {
            if (citizenId == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(citizenId), "The citizen ID cannot be 0");
            }

            return CitizenManager.instance.m_citizens.m_buffer[citizenId].CurrentLocation;
        }

        /// <summary>Gets the wealth of the citizen with specified ID.</summary>
        /// <param name="citizenId">The ID of the citizen to query wealth of.</param>
        /// <returns>A <see cref="Citizen.Wealth"/> value that describes the citizen's current wealth.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the argument is 0.</exception>
        public Citizen.Wealth GetCitizenWealth(uint citizenId)
        {
            if (citizenId == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(citizenId), "The citizen ID cannot be 0");
            }

            return CitizenManager.instance.m_citizens.m_buffer[citizenId].WealthLevel;
        }

        /// <summary>Attempts to get IDs of the citizen's family members IDs.</summary>
        /// <param name="citizenId">The ID of the citizen to get family members for.</param>
        /// <param name="targetBuffer">An array of 4 elements to store the results in.</param>
        /// <returns><c>true</c> if the specified citizen has at least one family member; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="targetBuffer"/> is null.</exception>
        public bool TryGetFamily(uint citizenId, uint[] targetBuffer)
        {
            if (targetBuffer == null)
            {
                throw new ArgumentNullException(nameof(targetBuffer));
            }

            for (int i = 0; i < targetBuffer.Length; ++i)
            {
                targetBuffer[i] = 0;
            }

            ref Citizen citizen = ref CitizenManager.instance.m_citizens.m_buffer[citizenId];
            if (citizen.m_homeBuilding == 0)
            {
                return false;
            }

            uint unitId = BuildingManager.instance.m_buildings.m_buffer[citizen.m_homeBuilding].FindCitizenUnit(CitizenUnit.Flags.Home, citizenId);
            if (unitId == 0)
            {
                return false;
            }

            ref CitizenUnit unit = ref CitizenManager.instance.m_units.m_buffer[unitId];
            int currentMember = -1;
            if (unit.m_citizen0 != 0 && unit.m_citizen0 != citizenId)
            {
                targetBuffer[++currentMember] = unit.m_citizen0;
            }

            if (unit.m_citizen1 != 0 && unit.m_citizen1 != citizenId)
            {
                targetBuffer[++currentMember] = unit.m_citizen1;
            }

            if (unit.m_citizen2 != 0 && unit.m_citizen2 != citizenId)
            {
                targetBuffer[++currentMember] = unit.m_citizen2;
            }

            if (unit.m_citizen3 != 0 && unit.m_citizen3 != citizenId)
            {
                targetBuffer[++currentMember] = unit.m_citizen3;
            }

            if (currentMember < 3 && unit.m_citizen4 != 0 && unit.m_citizen4 != citizenId)
            {
                targetBuffer[++currentMember] = unit.m_citizen4;
            }

            return currentMember > 0;
        }

        /// <summary>Gets the game's citizens array (direct reference).</summary>
        /// <returns>The reference to the game's array containing the <see cref="Citizen"/> items.</returns>
        public Citizen[] GetCitizensArray() => CitizenManager.instance.m_citizens.m_buffer;

        /// <summary>Releases the citizen instance's path and cancels any ongoing movement.</summary>
        /// <param name="instanceId">The citizen's instance ID.</param>
        /// <param name="resetTarget"><c>true</c> to reset the current citizen's target.</param>
        public void StopMoving(ushort instanceId, bool resetTarget)
        {
            if (instanceId == 0)
            {
                return;
            }

            ref CitizenInstance instance = ref CitizenManager.instance.m_instances.m_buffer[instanceId];
            if (instance.m_path != 0)
            {
                PathManager.instance.ReleasePath(instance.m_path);
                instance.m_path = 0u;
            }

            instance.m_flags &= ~(CitizenInstance.Flags.WaitingTransport | CitizenInstance.Flags.EnteringVehicle | CitizenInstance.Flags.BoredOfWaiting | CitizenInstance.Flags.WaitingTaxi);
            if (resetTarget)
            {
                instance.Info.m_citizenAI.SetTarget(instanceId, ref instance, 0);
            }
        }

        /// <summary>Provides a direct reference to the citizen data structure located in the game's buffer.</summary>
        /// <param name="instanceId">The ID of the citizen instance to get the citizen data structure for.</param>
        /// <returns>A direct reference to the <see name="Citizen"/> data structure.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="instanceId"/> is 0.</exception>
        public ref Citizen GetCitizen(ushort instanceId)
        {
            if (instanceId == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(instanceId), "The instance ID cannot be 0");
            }

            CitizenManager citizenMgr = CitizenManager.instance;
            uint citizenId = citizenMgr.m_instances.m_buffer[instanceId].m_citizen;
            return ref citizenMgr.m_citizens.m_buffer[citizenId];
        }
    }
}