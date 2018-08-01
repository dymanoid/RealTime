// <copyright file="ITravelBehavior.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.CustomAI
{
    /// <summary>
    /// An interface for citizens traveling behavior.
    /// </summary>
    internal interface ITravelBehavior
    {
        /// <summary>Gets an estimated travel time (in hours) between two specified buildings.</summary>
        /// <param name="building1">The ID of the first building.</param>
        /// <param name="building2">The ID of the second building.</param>
        /// <returns>An estimated travel time in hours.</returns>
        float GetEstimatedTravelTime(ushort building1, ushort building2);
    }
}