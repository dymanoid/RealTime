// <copyright file="LogCategories.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.Tools
{
    using System;

    /// <summary>
    /// The categories for the debug logging.
    /// </summary>
    [Flags]
    internal enum LogCategories
    {
        /// <summary>The invalid category - no logging</summary>
        Invalid = 0,

        /// <summary>No specific category</summary>
        Generic = 1,

        /// <summary>Citizens scheduling</summary>
        Schedule = 2,

        /// <summary>Citizens movement</summary>
        Movement = 4,

        /// <summary>Citizens movement</summary>
        Events = 8,

        /// <summary>Citizens state</summary>
        State = 16,

        /// <summary>Simulation related information</summary>
        Simulation = 32,

        /// <summary>All categories</summary>
        All = Generic | Schedule | Movement | Events | State | Simulation
    }
}
