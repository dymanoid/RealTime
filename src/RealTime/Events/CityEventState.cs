// <copyright file="CityEventState.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.Events
{
    /// <summary>A state of a city event.</summary>
    internal enum CityEventState
    {
        /// <summary>The state is unknown.</summary>
        None,

        /// <summary>The city event is planned or being prepared. It will take place in the future.</summary>
        Upcoming,

        /// <summary>The city event is currently taking place.</summary>
        Ongoing,

        /// <summary>The city event has finished.</summary>
        Finished,
    }
}
