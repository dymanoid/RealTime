// <copyright file="LogCategory.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime
{
    /// <summary>
    /// The categories for the debug logging.
    /// </summary>
    internal enum LogCategory
    {
        /// <summary>The invalid category - no logging</summary>
        None,

        /// <summary>No specific category</summary>
        Generic,

        /// <summary>Citizens scheduling</summary>
        Schedule,

        /// <summary>Citizens movement</summary>
        Movement,

        /// <summary>Citizens movement</summary>
        Events,

        /// <summary>Citizens state</summary>
        State,

        /// <summary>Simulation related information</summary>
        Simulation
    }
}
