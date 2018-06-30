// <copyright file="SliderValueType.cs" company="dymanoid">
//     Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.UI
{
    /// <summary>Possible types of values that will be displayed by the slider.</summary>
    internal enum SliderValueType
    {
        /// <summary>The default value type. A simple string representation.</summary>
        Default,

        /// <summary>The percentage value type.</summary>
        Percentage,

        /// <summary>The time value type. Localized time display.</summary>
        Time,

        /// <summary>The duration value type. Displayed as hours and minutes.</summary>
        Duration
    }
}