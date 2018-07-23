// <copyright file="RealTimeMath.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.Tools
{
    /// <summary>
    /// A static class containing various math methods.
    /// </summary>
    internal static class RealTimeMath
    {
        /// <summary>Ensures that a value is constrained by the specified range.</summary>
        /// <param name="value">The value to check.</param>
        /// <param name="min">The minimum constraint.</param>
        /// <param name="max">The maximum constraint.</param>
        /// <returns>A value that is guaranteed to be in the specified range.</returns>
        public static float Clamp(float value, float min, float max)
        {
            if (value < min)
            {
                return min;
            }

            if (value > max)
            {
                return max;
            }

            return value;
        }

        /// <summary>Ensures that a value is constrained by the specified range.</summary>
        /// <param name="value">The value to check.</param>
        /// <param name="min">The minimum constraint.</param>
        /// <param name="max">The maximum constraint.</param>
        /// <returns>A value that is guaranteed to be in the specified range.</returns>
        public static uint Clamp(uint value, uint min, uint max)
        {
            if (value < min)
            {
                return min;
            }

            if (value > max)
            {
                return max;
            }

            return value;
        }

        /// <summary>Ensures that a value is constrained by the specified range.</summary>
        /// <param name="value">The value to check.</param>
        /// <param name="min">The minimum constraint.</param>
        /// <param name="max">The maximum constraint.</param>
        /// <returns>A value that is guaranteed to be in the specified range.</returns>
        public static int Clamp(int value, int min, int max)
        {
            if (value < min)
            {
                return min;
            }

            if (value > max)
            {
                return max;
            }

            return value;
        }
    }
}
