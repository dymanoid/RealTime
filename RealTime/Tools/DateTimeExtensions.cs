// <copyright file="DateTimeExtensions.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.Tools
{
    using System;

    /// <summary>
    /// A class containing various extension methods for the <see cref="DateTime"/> struct.
    /// </summary>
    internal static class DateTimeExtensions
    {
        /// <summary>
        /// Determines whether the <see cref="DateTime"/>'s day of week is Saturday or Sunday.
        /// </summary>
        ///
        /// <param name="dateTime">The <see cref="DateTime"/> to check.</param>
        ///
        /// <returns>True if the <see cref="DateTime"/>'s day of week value is Saturday or Sunday;
        /// otherwise, false.</returns>
        public static bool IsWeekend(this DateTime dateTime)
        {
            return dateTime.DayOfWeek == DayOfWeek.Saturday || dateTime.DayOfWeek == DayOfWeek.Sunday;
        }

        /// <summary>
        /// Determines whether the <see cref="DateTime"/>'s day of week will still be Saturday or Sunday
        /// after the provided time <paramref name="interval"/> is elapsed.
        /// </summary>
        ///
        /// <param name="dateTime">The base <see cref="DateTime"/> to check.</param>
        /// <param name="interval">A <see cref="TimeSpan"/> that represents an interval relative to the
        /// <see cref="DateTime"/> to check.</param>
        ///
        /// <returns>True if the <see cref="DateTime"/>'s day of week value is Saturday or Sunday after
        /// the provided time <paramref name="interval"/> is elapsed; otherwise, false.</returns>
        public static bool IsWeekendAfter(this DateTime dateTime, TimeSpan interval)
        {
            return (dateTime + interval).IsWeekend();
        }

        /// <summary>
        /// Gets a <see cref="float"/> value representing the current day's hour.
        /// </summary>
        ///
        /// <param name="dateTime">The <see cref="DateTime"/> to act on.</param>
        ///
        /// <returns>A <see cref="float"/> value that represents the cuurent day's hour.</returns>
        public static float HourOfDay(this DateTime dateTime)
        {
            return (float)dateTime.TimeOfDay.TotalHours;
        }
    }
}
