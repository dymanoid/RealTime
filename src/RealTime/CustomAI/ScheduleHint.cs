// <copyright file="ScheduleHint.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.CustomAI
{
    /// <summary>Describes various citizen schedule hints.</summary>
    internal enum ScheduleHint : byte
    {
        /// <summary>No hint.</summary>
        None,

        /// <summary>The citizen can shop only locally.</summary>
        LocalShoppingOnly,

        /// <summary>The citizen will not go shopping one more time right away.</summary>
        NoShoppingAnyMore,

        /// <summary>The citizen should find a leisure building.</summary>
        RelaxAtLeisureBuilding,

        /// <summary>The citizen is on a guided tour.</summary>
        OnTour,

        /// <summary>The citizen is attending an event.</summary>
        AttendingEvent,
    }
}