// <copyright file="WorkStatus.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.CustomAI
{
    /// <summary>
    /// Describes the work (or school/university) status of a citizen.
    /// </summary>
    internal enum WorkStatus : byte
    {
        /// <summary>No special handling.</summary>
        Default,

        /// <summary>The citizen is at work or is heading to the work building.</summary>
        AtWork,

        /// <summary>The citizen takes a break and goes out for lunch.</summary>
        AtLunch,

        /// <summary>The citizen is on vacation or has a day off work.</summary>
        OnVacation
    }
}
