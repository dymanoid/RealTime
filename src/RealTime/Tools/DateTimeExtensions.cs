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

        public static bool IsWeekendTime(this DateTime dateTime, float fridayStartHour, float sundayEndHour)
        {
            switch (dateTime.DayOfWeek)
            {
                case DayOfWeek.Friday when dateTime.Hour >= fridayStartHour:
                case DayOfWeek.Saturday:
                case DayOfWeek.Sunday when dateTime.Hour < sundayEndHour:
                    return true;

                default:
                    return false;
            }
        }
    }
}
