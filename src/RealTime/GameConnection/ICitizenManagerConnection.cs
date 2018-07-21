// <copyright file="ICitizenManagerConnection.cs" company="dymanoid">
//     Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.GameConnection
{
    /// <summary>An interface for the game specific logic related to the citizen management.</summary>
    internal interface ICitizenManagerConnection
    {
        /// <summary>Releases the specified citizen.</summary>
        /// <param name="citizenId">The ID of the citizen to release.</param>
        void ReleaseCitizen(uint citizenId);

        /// <summary>Gets the ID of the building this citizen is currently moving to.</summary>
        /// <param name="instanceId">The citizen's instance ID.</param>
        /// <returns>The ID of the building the citizen is moving to, or 0 if none.</returns>
        ushort GetTargetBuilding(ushort instanceId);

        /// <summary>Determines whether the citizen's instance with specified ID has particular flags.</summary>
        /// <param name="instanceId">The instance ID to check.</param>
        /// <param name="flags">The flags to check.</param>
        /// <param name="all">
        /// <c>true</c> to check all flags from the specified <paramref name="flags"/>, <c>false</c> to check any flags.
        /// </param>
        /// <returns><c>true</c> if the citizen instance has the specified flags; otherwise, <c>false</c>.</returns>
        bool InstanceHasFlags(ushort instanceId, CitizenInstance.Flags flags, bool all = false);

        /// <summary>
        /// Gets the current wait counter value of the citizen's instance with specified ID.
        /// </summary>
        /// <param name="instanceId">The instance ID to check.</param>
        /// <returns>The wait counter value of the citizen's instance.</returns>
        byte GetInstanceWaitCounter(ushort instanceId);

        /// <summary>
        /// Determines whether the area around the citizen's instance with specified ID is currently marked for evacuation.
        /// </summary>
        /// <param name="instanceId">The ID of the citizen's instance to check.</param>
        /// <returns><c>true</c> if the area around the citizen's instance is marked for evacuation; otherwise, <c>false</c>.</returns>
        bool IsAreaEvacuating(ushort instanceId);

        /// <summary>Modifies the goods storage in the specified unit.</summary>
        /// <param name="unitId">The unit ID to process.</param>
        /// <param name="amount">The amount to modify the storage by.</param>
        /// <returns><c>true</c> on success; otherwise, <c>false</c>.</returns>
        bool ModifyUnitGoods(uint unitId, ushort amount);

        /// <summary>Gets the count of the currently active citizens instances.</summary>
        /// <returns>The number of active citizens instances.</returns>
        uint GetInstancesCount();

        /// <summary>Gets the maximum count of the active citizens instances.</summary>
        /// <returns>The maximum number of active citizens instances.</returns>
        uint GetMaxInstancesCount();

        /// <summary>Gets the maximum count of the citizens.</summary>
        /// <returns>The maximum number of the citizens.</returns>
        uint GetMaxCitizensCount();

        /// <summary>Gets the location of the citizen with specified ID.</summary>
        /// <param name="citizenId">The ID of the citizen to query location of.</param>
        /// <returns>A <see cref="Citizen.Location"/> value that describes the citizen's current location.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown when the argument is 0.</exception>
        Citizen.Location GetCitizenLocation(uint citizenId);

        /// <summary>Gets the game's citizens array (direct reference).</summary>
        /// <returns>The reference to the game's array containing the <see cref="Citizen"/> items.</returns>
        Citizen[] GetCitizensArray();
    }
}