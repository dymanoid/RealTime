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

        /// <summary>Determines whether the citizen's instance with provided ID has particular flags.</summary>
        /// <param name="instanceId">The instance ID to check.</param>
        /// <param name="flags">The flags to check.</param>
        /// <param name="all">
        /// <c>true</c> to check all flags from the provided <paramref name="flags"/>, <c>false</c> to check any flags.
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
    }
}