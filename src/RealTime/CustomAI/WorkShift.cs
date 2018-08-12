// <copyright file="WorkShift.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.CustomAI
{
    /// <summary>
    /// An enumeration that describes the citizen's work shift.
    /// </summary>
    internal enum WorkShift : byte
    {
        /// <summary>The citizen will not go to work or school.</summary>
        Unemployed,

        /// <summary>The citizen will not work first (or default) shift.</summary>
        First,

        /// <summary>The citizen will not work second shift.</summary>
        Second,

        /// <summary>The citizen will not work night shift.</summary>
        Night,
    }
}
