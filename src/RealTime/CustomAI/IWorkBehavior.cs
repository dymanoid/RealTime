// <copyright file="IWorkBehavior.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.CustomAI
{
    /// <summary>
    /// An interface for the citizens work behavior.
    /// </summary>
    internal interface IWorkBehavior
    {
        /// <summary>Notifies this object that a new game day starts.</summary>
        void BeginNewDay();

        /// <summary>
        /// Determines whether a building of specified <paramref name="service"/> and <paramref name="subService"/>
        /// currently has working hours. Note that this method always returns <c>true</c> for residential buildings.
        /// </summary>
        /// <param name="service">The building service.</param>
        /// <param name="subService">The building sub-service.</param>
        /// <returns>
        ///   <c>true</c> if a building of specified <paramref name="service"/> and <paramref name="subService"/>
        /// currently has working hours; otherwise, <c>false</c>.
        /// </returns>
        bool IsBuildingWorking(ItemClass.Service service, ItemClass.SubService subService);

        /// <summary>Updates the citizen's work schedule by determining the time for going to work.</summary>
        /// <param name="schedule">The citizen's schedule to update.</param>
        /// <param name="currentBuilding">The ID of the building where the citizen is currently located.</param>
        /// <param name="simulationCycle">The duration (in hours) of a full citizens simulation cycle.</param>
        /// <returns><c>true</c> if work was scheduled; otherwise, <c>false</c>.</returns>
        bool ScheduleGoToWork(ref CitizenSchedule schedule, ushort currentBuilding, float simulationCycle);

        /// <summary>Updates the citizen's work schedule by determining the lunch time.</summary>
        /// <param name="schedule">The citizen's schedule to update.</param>
        /// <param name="citizenAge">The citizen's age.</param>
        /// <returns><c>true</c> if a lunch time was scheduled; otherwise, <c>false</c>.</returns>
        bool ScheduleLunch(ref CitizenSchedule schedule, Citizen.AgeGroup citizenAge);

        /// <summary>Updates the citizen's work schedule by determining the returning from lunch time.</summary>
        /// <param name="schedule">The citizen's schedule to update.</param>
        void ScheduleReturnFromLunch(ref CitizenSchedule schedule);

        /// <summary>Updates the citizen's work schedule by determining the time for returning from work.</summary>
        /// <param name="schedule">The citizen's schedule to update.</param>
        /// <param name="citizenAge">The age of the citizen.</param>
        void ScheduleReturnFromWork(ref CitizenSchedule schedule, Citizen.AgeGroup citizenAge);

        /// <summary>Updates the citizen's work shift parameters in the specified citizen's <paramref name="schedule"/>.</summary>
        /// <param name="schedule">The citizen's schedule to update the work shift in.</param>
        /// <param name="citizenAge">The age of the citizen.</param>
        void UpdateWorkShift(ref CitizenSchedule schedule, Citizen.AgeGroup citizenAge);
    }
}