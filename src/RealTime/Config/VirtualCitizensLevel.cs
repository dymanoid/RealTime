// <copyright file="VirtualCitizensLevel.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.Config
{
    /// <summary>
    /// Possible virtual citizens configurations.
    /// </summary>
    public enum VirtualCitizensLevel
    {
        /// <summary>No virtual citizens.</summary>
        None,

        /// <summary>Only few virtual citizens.</summary>
        Few,

        /// <summary>The configuration uses the vanilla game setting.</summary>
        Vanilla,

        /// <summary>Many virtual citizens.</summary>
        Many
    }
}
