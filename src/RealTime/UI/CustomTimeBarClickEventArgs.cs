// <copyright file="CustomTimeBarClickEventArgs.cs" company="dymanoid">
//     Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.UI
{
    using System;

    /// <summary>Additional information for the time bar click events.</summary>
    internal sealed class CustomTimeBarClickEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CustomTimeBarClickEventArgs"/> class.
        /// </summary>
        /// <param name="cityEventBuildingId">The ID of the building where a city event takes place.</param>
        public CustomTimeBarClickEventArgs(ushort cityEventBuildingId)
        {
            CityEventBuildingId = cityEventBuildingId;
        }

        /// <summary>Gets the ID of the building where a clicked city event takes place.</summary>
        public ushort CityEventBuildingId { get; }
    }
}