// <copyright file="ICitizenConnection.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.GameConnection
{
    /// <summary>
    /// An interface for a proxy object that can get and set properties of a citizen object.
    /// </summary>
    /// <typeparam name="T">The type of the citizen object to operate on.</typeparam>
    internal interface ICitizenConnection<T>
        where T : struct
    {
        /// <summary>Gets the home building ID of the specified citizen.</summary>
        /// <param name="citizen">The citizen to get the home building ID of.</param>
        /// <returns>The ID of the citizen's home building, or 0 if none found.</returns>
        ushort GetHomeBuilding(ref T citizen);

        /// <summary>Gets the work building ID of the specified citizen.</summary>
        /// <param name="citizen">The citizen to get the work building ID of.</param>
        /// <returns>The ID of the citizen's work building, or 0 if none found.</returns>
        ushort GetWorkBuilding(ref T citizen);

        /// <summary>Gets the ID of the building currently visited by the specified citizen.</summary>
        /// <param name="citizen">The citizen to get the visited building ID of.</param>
        /// <returns>The ID of the building currently visited by the citizen, or 0 if none found.</returns>
        ushort GetVisitBuilding(ref T citizen);

        /// <summary>
        /// Sets the ID of the building that is currently visited by the specified citizen.
        /// </summary>
        /// <param name="citizen">The citizen to set the visited building ID for.</param>
        /// <param name="visitBuilding">The ID of the currently visited building.</param>
        void SetVisitBuilding(ref T citizen, ushort visitBuilding);

        /// <summary>Gets the instance ID of the specified citizen.</summary>
        /// <param name="citizen">The citizen to get the instance ID of.</param>
        /// <returns>The ID of the citizen's instance, or 0 if none found.</returns>
        ushort GetInstance(ref T citizen);

        /// <summary>Gets the vehicle ID of the specified citizen.</summary>
        /// <param name="citizen">The citizen to get the vehicle ID of.</param>
        /// <returns>The ID of the citizen's vehicle, or 0 if none found.</returns>
        ushort GetVehicle(ref T citizen);

        /// <summary>Determines whether the specified citizen is collapsed.</summary>
        /// <param name="citizen">The citizen to check.</param>
        /// <returns>
        ///   <c>true</c> if the specified citizen is collapsed; otherwise, <c>false</c>.
        /// </returns>
        bool IsCollapsed(ref T citizen);

        /// <summary>Determines whether the specified citizen is dead.</summary>
        /// <param name="citizen">The citizen to check.</param>
        /// <returns>
        ///   <c>true</c> if the specified citizen is dead; otherwise, <c>false</c>.
        /// </returns>
        bool IsDead(ref T citizen);

        /// <summary>Determines whether the specified citizen is sick.</summary>
        /// <param name="citizen">The citizen to check.</param>
        /// <returns>
        ///   <c>true</c> if the specified citizen is sick; otherwise, <c>false</c>.
        /// </returns>
        bool IsSick(ref T citizen);

        /// <summary>Determines whether the specified citizen is arrested.</summary>
        /// <param name="citizen">The citizen to check.</param>
        /// <returns>
        ///   <c>true</c> if the specified citizen is arrested; otherwise, <c>false</c>.
        /// </returns>
        bool IsArrested(ref T citizen);

        /// <summary>Sets the specified citizen's arrested flag value.</summary>
        /// <param name="citizen">The citizen change the arrested flag of.</param>
        /// <param name="isArrested">The flag value.</param>
        void SetArrested(ref T citizen, bool isArrested);

        /// <summary>Gets the ID of the building the specified citizen is currently located in.</summary>
        /// <param name="citizen">The citizen to get the current location of.</param>
        /// <returns>The ID of the building where the specified citizen is located, 0 if none found.</returns>
        ushort GetCurrentBuilding(ref T citizen);

        /// <summary>Gets the current location type of the specified citizen.</summary>
        /// <param name="citizen">The citizen to get the current location of.</param>
        /// <returns>The citizen's location type.</returns>
        Citizen.Location GetLocation(ref T citizen);

        /// <summary>Sets the current location type of the specified citizen.</summary>
        /// <param name="citizen">The citizen to set location type of.</param>
        /// <param name="location">The location type to set.</param>
        void SetLocation(ref T citizen, Citizen.Location location);

        /// <summary>Gets the citizen's age.</summary>
        /// <param name="citizen">The citizen to get the age of.</param>
        /// <returns>The citizen's age.</returns>
        Citizen.AgeGroup GetAge(ref T citizen);

        /// <summary>Gets the citizen's wealth level.</summary>
        /// <param name="citizen">The citizen to get the wealth level of.</param>
        /// <returns>The citizen's wealth level.</returns>
        Citizen.Wealth GetWealthLevel(ref T citizen);

        /// <summary>Gets the citizen's education level.</summary>
        /// <param name="citizen">The citizen to get the education level of.</param>
        /// <returns>The citizen's education level.</returns>
        Citizen.Education GetEducationLevel(ref T citizen);

        /// <summary>Gets the citizen's gender.</summary>
        /// <param name="citizenId">The ID of the citizen to get the gender of.</param>
        /// <returns>The citizen's gender.</returns>
        Citizen.Gender GetGender(uint citizenId);

        /// <summary>Gets the citizen's happiness level.</summary>
        /// <param name="citizen">The citizen to get the happiness level of.</param>
        /// <returns>The citizen's happiness level.</returns>
        Citizen.Happiness GetHappinessLevel(ref T citizen);

        /// <summary>Gets the citizen's wellbeing level.</summary>
        /// <param name="citizen">The citizen to get the wellbeing level of.</param>
        /// <returns>The citizen's wellbeing level.</returns>
        Citizen.Wellbeing GetWellbeingLevel(ref T citizen);

        /// <summary>Determines whether the specified citizen has particular flags.</summary>
        /// <param name="citizen">The citizen to check flags of.</param>
        /// <param name="flags">The flags to check.</param>
        /// <returns>
        ///   <c>true</c> if the citizen has any of the specified flags; otherwise, <c>false</c>.
        /// </returns>
        bool HasFlags(ref T citizen, Citizen.Flags flags);

        /// <summary>Sets the ID of the home building for the specified citizen.</summary>
        /// <param name="citizen">The citizen to set the home building for.</param>
        /// <param name="citizenId">The citizen ID.</param>
        /// <param name="buildingId">The building ID to set as home.</param>
        void SetHome(ref T citizen, uint citizenId, ushort buildingId);

        /// <summary>Sets the ID of the work building for the specified citizen.</summary>
        /// <param name="citizen">The citizen to set the work building for.</param>
        /// <param name="citizenId">The citizen ID.</param>
        /// <param name="buildingId">The building ID to set as workplace.</param>
        void SetWorkplace(ref T citizen, uint citizenId, ushort buildingId);

        /// <summary>Sets the ID of the building the specified citizen is currently visiting.</summary>
        /// <param name="citizen">The citizen to set the building for.</param>
        /// <param name="citizenId">The citizen ID.</param>
        /// <param name="buildingId">The ID of the building the citizen is visiting.</param>
        void SetVisitPlace(ref T citizen, uint citizenId, ushort buildingId);

        /// <summary>Adds the specified flags to a citizen.</summary>
        /// <param name="citizen">The citizen to add flags to.</param>
        /// <param name="flags">The flags to add.</param>
        /// <returns>The current citizen's flags after adding the specified flags.</returns>
        Citizen.Flags AddFlags(ref T citizen, Citizen.Flags flags);

        /// <summary>removes the specified flags from a citizen.</summary>
        /// <param name="citizen">The citizen to remove flags from.</param>
        /// <param name="flags">The flags to remove.</param>
        /// <returns>The current citizen's flags after removing the specified flags.</returns>
        Citizen.Flags RemoveFlags(ref T citizen, Citizen.Flags flags);

        /// <summary>Gets the unit ID that contains the specified citizen.</summary>
        /// <param name="citizen">The citizen to get the unit ID for.</param>
        /// <param name="citizenId">The citizen ID to get the unit ID for.</param>
        /// <param name="unitId">The unit ID of the citizen's building specified by the <paramref name="flag"/>.</param>
        /// <param name="flag">The citizen unit mode.</param>
        /// <returns>An ID of the citizen unit that contains the specified citizen</returns>
        uint GetContainingUnit(ref T citizen, uint citizenId, uint unitId, CitizenUnit.Flags flag);
    }
}
