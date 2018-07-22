// <copyright file="ICityEventsProvider.cs" company="dymanoid">
//     Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.Events
{
    using RealTime.Events.Storage;

    /// <summary>
    /// An interface for a type that can create city event instances for specified building classes.
    /// </summary>
    internal interface ICityEventsProvider
    {
        /// <summary>
        /// Gets a randomly created city event for a building of specified class. If no city event
        /// could be created, returns <c>null</c>.
        /// </summary>
        /// <param name="buildingClass">The building class to create a city event for.</param>
        /// <returns>An instance of <see cref="ICityEvent"/> or null.</returns>
        /// <exception cref="System.ArgumentException">
        /// Thrown when the argument is null or an empty string.
        /// </exception>
        ICityEvent GetRandomEvent(string buildingClass);

        /// <summary>
        /// Gets the event template that has the specified name and is configured for the specified
        /// building class.
        /// </summary>
        /// <param name="eventName">The unique name of the city event template.</param>
        /// <param name="buildingClassName">
        /// The name of the building class the searched template is configured for.
        /// </param>
        /// <returns>An instance of <see cref="CityEventTemplate"/> or null of none found.</returns>
        /// <exception cref="System.ArgumentException">
        /// Thrown when any argument is null or an empty string.
        /// </exception>
        CityEventTemplate GetEventTemplate(string eventName, string buildingClassName);
    }
}