// <copyright file="CitizenConnection.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.GameConnection
{
    /// <summary>
    /// A default implementation of the <see cref="ICitizenConnection{TCitizen}"/> interface.
    /// </summary>
    /// <seealso cref="ICitizenConnection{Citizen}" />
    internal sealed class CitizenConnection : ICitizenConnection<Citizen>
    {
        /// <summary>Adds the specified flags to a citizen.</summary>
        /// <param name="citizen">The citizen to add flags to.</param>
        /// <param name="flags">The flags to add.</param>
        /// <returns>The current citizen's flags after adding the specified flags.</returns>
        public Citizen.Flags AddFlags(ref Citizen citizen, Citizen.Flags flags)
        {
            Citizen.Flags currentFlags = citizen.m_flags;
            currentFlags |= flags;
            return citizen.m_flags = currentFlags;
        }

        /// <summary>Gets the citizen's age.</summary>
        /// <param name="citizen">The citizen to get the age of.</param>
        /// <returns>The citizen's age.</returns>
        public Citizen.AgeGroup GetAge(ref Citizen citizen)
        {
            return Citizen.GetAgeGroup(citizen.m_age);
        }

        /// <summary>Gets the citizen's wealth level.</summary>
        /// <param name="citizen">The citizen to get the wealth level of.</param>
        /// <returns>The citizen's wealth level.</returns>
        public Citizen.Wealth GetWealthLevel(ref Citizen citizen)
        {
            return citizen.WealthLevel;
        }

        /// <summary>Gets the citizen's education level.</summary>
        /// <param name="citizen">The citizen to get the education level of.</param>
        /// <returns>The citizen's education level.</returns>
        public Citizen.Education GetEducationLevel(ref Citizen citizen)
        {
            return citizen.EducationLevel;
        }

        /// <summary>Gets the citizen's gender.</summary>
        /// <param name="citizenId">The ID of the citizen to get the gender of.</param>
        /// <returns>The citizen's gender.</returns>
        public Citizen.Gender GetGender(uint citizenId)
        {
            return Citizen.GetGender(citizenId);
        }

        /// <summary>Gets the citizen's happiness level.</summary>
        /// <param name="citizen">The citizen to get the happiness level of.</param>
        /// <returns>The citizen's happiness level.</returns>
        public Citizen.Happiness GetHappinessLevel(ref Citizen citizen)
        {
            return Citizen.GetHappinessLevel(Citizen.GetHappiness(citizen.m_health, citizen.m_wellbeing));
        }

        /// <summary>Gets the citizen's wellbeing level.</summary>
        /// <param name="citizen">The citizen to get the wellbeing level of.</param>
        /// <returns>The citizen's wellbeing level.</returns>
        public Citizen.Wellbeing GetWellbeingLevel(ref Citizen citizen)
        {
            return Citizen.GetWellbeingLevel(citizen.EducationLevel, citizen.m_wellbeing);
        }

        /// <summary>
        /// Gets the ID of the building the specified citizen is currently located in.
        /// </summary>
        /// <param name="citizen">The citizen to get the current location of.</param>
        /// <returns>
        /// The ID of the building where the specified citizen is located, 0 if none found.
        /// </returns>
        public ushort GetCurrentBuilding(ref Citizen citizen)
        {
            return citizen.GetBuildingByLocation();
        }

        /// <summary>
        /// Determines whether the specified citizen has particular flags.
        /// </summary>
        /// <param name="citizen">The citizen to check flags of.</param>
        /// <param name="flags">The flags to check.</param>
        /// <returns>
        /// <c>true</c> if the citizen has any of the specified flags; otherwise, <c>false</c>.
        /// </returns>
        public bool HasFlags(ref Citizen citizen, Citizen.Flags flags)
        {
            return (citizen.m_flags & flags) != 0;
        }

        /// <summary>Gets the home building ID of the specified citizen.</summary>
        /// <param name="citizen">The citizen to get the home building ID of.</param>
        /// <returns>The ID of the citizen's home building, or 0 if none found.</returns>
        public ushort GetHomeBuilding(ref Citizen citizen)
        {
            return citizen.m_homeBuilding;
        }

        /// <summary>Gets the instance ID of the specified citizen.</summary>
        /// <param name="citizen">The citizen to get the instance ID of.</param>
        /// <returns>The ID of the citizen's instance, or 0 if none found.</returns>
        public ushort GetInstance(ref Citizen citizen)
        {
            return citizen.m_instance;
        }

        /// <summary>Gets the current location type of the specified citizen.</summary>
        /// <param name="citizen">The citizen to get the current location of.</param>
        /// <returns>The citizen's location type.</returns>
        public Citizen.Location GetLocation(ref Citizen citizen)
        {
            return citizen.CurrentLocation;
        }

        /// <summary>Gets the vehicle ID of the specified citizen.</summary>
        /// <param name="citizen">The citizen to get the vehicle ID of.</param>
        /// <returns>The ID of the citizen's vehicle, or 0 if none found.</returns>
        public ushort GetVehicle(ref Citizen citizen)
        {
            return citizen.m_vehicle;
        }

        /// <summary>
        /// Gets the ID of the building currently visited by the specified citizen.
        /// </summary>
        /// <param name="citizen">The citizen to get the visited building ID of.</param>
        /// <returns>
        /// The ID of the building currently visited by the citizen, or 0 if none found.
        /// </returns>
        public ushort GetVisitBuilding(ref Citizen citizen)
        {
            return citizen.m_visitBuilding;
        }

        /// <summary>Gets the work building ID of the specified citizen.</summary>
        /// <param name="citizen">The citizen to get the work building ID of.</param>
        /// <returns>The ID of the citizen's work building, or 0 if none found.</returns>
        public ushort GetWorkBuilding(ref Citizen citizen)
        {
            return citizen.m_workBuilding;
        }

        /// <summary>Determines whether the specified citizen is arrested.</summary>
        /// <param name="citizen">The citizen to check.</param>
        /// <returns>
        /// <c>true</c> if the specified citizen is arrested; otherwise, <c>false</c>.
        /// </returns>
        public bool IsArrested(ref Citizen citizen)
        {
            return citizen.Arrested;
        }

        /// <summary>Determines whether the specified citizen is collapsed.</summary>
        /// <param name="citizen">The citizen to check.</param>
        /// <returns>
        /// <c>true</c> if the specified citizen is collapsed; otherwise, <c>false</c>.
        /// </returns>
        public bool IsCollapsed(ref Citizen citizen)
        {
            return citizen.Collapsed;
        }

        /// <summary>Determines whether the specified citizen is dead.</summary>
        /// <param name="citizen">The citizen to check.</param>
        /// <returns>
        /// <c>true</c> if the specified citizen is dead; otherwise, <c>false</c>.
        /// </returns>
        public bool IsDead(ref Citizen citizen)
        {
            return citizen.Dead;
        }

        /// <summary>Determines whether the specified citizen is sick.</summary>
        /// <param name="citizen">The citizen to check.</param>
        /// <returns>
        /// <c>true</c> if the specified citizen is sick; otherwise, <c>false</c>.
        /// </returns>
        public bool IsSick(ref Citizen citizen)
        {
            return citizen.Sick;
        }

        /// <summary>removes the specified flags from a citizen.</summary>
        /// <param name="citizen">The citizen to remove flags from.</param>
        /// <param name="flags">The flags to remove.</param>
        /// <returns>
        /// The current citizen's flags after removing the specified flags.
        /// </returns>
        public Citizen.Flags RemoveFlags(ref Citizen citizen, Citizen.Flags flags)
        {
            Citizen.Flags currentFlags = citizen.m_flags;
            currentFlags &= ~flags;
            return citizen.m_flags = currentFlags;
        }

        /// <summary>Sets the specified citizen's arrested flag value.</summary>
        /// <param name="citizen">The citizen change the arrested flag of.</param>
        /// <param name="isArrested">The flag value.</param>
        public void SetArrested(ref Citizen citizen, bool isArrested)
        {
            citizen.Arrested = isArrested;
        }

        /// <summary>Sets the ID of the home building for the specified citizen.</summary>
        /// <param name="citizen">The citizen to set the home building for.</param>
        /// <param name="citizenId">The citizen ID.</param>
        /// <param name="buildingId">The building ID to set as home.</param>
        public void SetHome(ref Citizen citizen, uint citizenId, ushort buildingId)
        {
            citizen.SetHome(citizenId, buildingId, 0u);
        }

        /// <summary>Sets the current location type of the specified citizen.</summary>
        /// <param name="citizen">The citizen to set location type of.</param>
        /// <param name="location">The location type to set.</param>
        public void SetLocation(ref Citizen citizen, Citizen.Location location)
        {
            citizen.CurrentLocation = location;
        }

        /// <summary>
        /// Sets the ID of the building the specified citizen is currently visiting.
        /// </summary>
        /// <param name="citizen">The citizen to set the building for.</param>
        /// <param name="citizenId">The citizen ID.</param>
        /// <param name="buildingId">The ID of the building the citizen is visiting.</param>
        public void SetVisitPlace(ref Citizen citizen, uint citizenId, ushort buildingId)
        {
            citizen.SetVisitplace(citizenId, buildingId, 0u);
        }

        /// <summary>Sets the ID of the work building for the specified citizen.</summary>
        /// <param name="citizen">The citizen to set the work building for.</param>
        /// <param name="citizenId">The citizen ID.</param>
        /// <param name="buildingId">The building ID to set as workplace.</param>
        public void SetWorkplace(ref Citizen citizen, uint citizenId, ushort buildingId)
        {
            citizen.SetWorkplace(citizenId, buildingId, 0u);
        }

        /// <summary>Gets the unit ID that contains the specified citizen.</summary>
        /// <param name="citizen">The citizen to get the unit ID for.</param>
        /// <param name="citizenId">The ID of the citizen to get the unit ID for.</param>
        /// <param name="unitId">The unit ID of the citizen's building specified by the <paramref name="flag"/>.</param>
        /// <param name="flag">The citizen unit mode.</param>
        /// <returns>An ID of the citizen unit that contains the specified citizen.</returns>
        public uint GetContainingUnit(ref Citizen citizen, uint citizenId, uint unitId, CitizenUnit.Flags flag)
        {
            if (citizenId == 0 || unitId == 0 || flag == CitizenUnit.Flags.None)
            {
                return 0;
            }

            return citizen.GetContainingUnit(citizenId, unitId, flag);
        }

        /// <summary>Determines whether the specified citizen object is empty, that means the citizen has no home, no work,
        /// no visit buildings, no vehicle, and is not instantiated.</summary>
        /// <param name="citizen">The citizen to check.</param>
        /// <returns><c>true</c> if the specified citizen is empty; otherwise, <c>false</c>.</returns>
        public bool IsEmpty(ref Citizen citizen)
        {
            return citizen.m_homeBuilding == 0
                && citizen.m_workBuilding == 0
                && citizen.m_visitBuilding == 0
                && citizen.m_instance == 0
                && citizen.m_vehicle == 0;
        }
    }
}
