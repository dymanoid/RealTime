// <copyright file="GameRandomizer.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.GameConnection
{
    using RealTime.Simulation;

    /// <summary>
    /// The default implementation of the <see cref="IRandomizer"/> interface.
    /// </summary>
    /// <seealso cref="IRandomizer" />
    internal sealed class GameRandomizer : IRandomizer
    {
        /// <summary>Gets a random value that fits in a range from 0 to <paramref name="max"/> (inclusive).</summary>
        /// <param name="max">A value that specifies the upper bound of the returned values.</param>
        /// <returns>A random value that fits in a range from 0 to <paramref name="max"/> (inclusive).</returns>
        public int GetRandomValue(uint max) => SimulationManager.instance.m_randomizer.Int32(max + 1u);

        /// <summary>Returns a value indicating whether an event with specified probability should now occur.</summary>
        /// <param name="probability">The probability of the event in percent. Valid values are 0..100..</param>
        /// <returns><c>true</c> if the event should occur; otherwise, <c>false</c>.</returns>
        public bool ShouldOccur(uint probability) => SimulationManager.instance.m_randomizer.Int32(100u) < probability;

        /// <summary>Returns a value indicating whether an event with specified precise probability should now occur.</summary>
        /// <param name="preciseProbability">The probability of the event in percent multiplied by 100. Valid values are 0..10.000.</param>
        /// <returns><c>true</c> if the event should occur; otherwise, <c>false</c>.</returns>
        public bool ShouldOccurPrecise(uint preciseProbability)
            => SimulationManager.instance.m_randomizer.Int32(10000u) < preciseProbability;
    }
}
