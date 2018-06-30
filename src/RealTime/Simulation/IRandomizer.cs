// <copyright file="IRandomizer.cs" company="dymanoid">Copyright (c) dymanoid. All rights reserved.</copyright>

namespace RealTime.Simulation
{
    /// <summary>An interface for a randomizer.</summary>
    internal interface IRandomizer
    {
        /// <summary>Returns a value indicating whether an event with specified probability should now occur.</summary>
        /// <param name="probability">The probability of the event in percent. Valid values are 0..100..</param>
        /// <returns><c>true</c> if the event should occur; otherwise, <c>false</c>.</returns>
        bool ShouldOccur(uint probability);

        /// <summary>Gets a random value that fits in a range from 0 to <paramref name="max"/> (inclusive).</summary>
        /// <param name="max">A value that specifies the upper bound of the returned values.</param>
        /// <returns>A random value that fits in a range from 0 to <paramref name="max"/> (inclusive).</returns>
        int GetRandomValue(uint max);
    }
}