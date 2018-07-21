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
        /// Determines whether this <see cref="DateTime"/> represents the Friday night or the Weekend time.
        /// </summary>
        ///
        /// <param name="dateTime">The <see cref="DateTime"/> to check.</param>
        /// <param name="fridayStartHour">The Friday's start hour to assume as the Weekend start.</param>
        /// <param name="sundayEndHour">The Sunday's end hour to assume as the Weekend end.</param>
        ///
        /// <returns>
        ///   <c>true</c> if this <see cref="DateTime"/> represents the Friday night or the Weekend time; otherwise, <c>false</c>.
        /// </returns>
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

        /// <summary>
        /// Rounds this <see cref="DateTime"/> to the provided <paramref name="interval"/> (ceiling).
        /// </summary>
        ///
        /// <param name="dateTime">The <see cref="DateTime"/> to round.</param>
        /// <param name="interval">The interval to round to.</param>
        ///
        /// <returns>The rounded <see cref="DateTime"/>.</returns>
        public static DateTime RoundCeil(this DateTime dateTime, TimeSpan interval)
        {
            long overflow = dateTime.Ticks % interval.Ticks;
            return overflow == 0 ? dateTime : dateTime.AddTicks(interval.Ticks - overflow);
        }

        /// <summary>Gets a new <see cref="DateTime"/> value that is greater than this <see cref="DateTime"/>
        /// and whose daytime hour equals to the specified one. If <paramref name="hour"/> is negative,
        /// it will be shifted in the next day to become positive.</summary>
        /// <param name="dateTime">The <see cref="DateTime"/> to get the future value for.</param>
        /// <param name="hour">The daytime hour for the result value. Can be negative.</param>
        /// <returns>A new <see cref="DateTime"/> value that is greater than this <see cref="DateTime"/>
        /// and whose daytime hour is set to the specified <paramref name="hour"/>.</returns>
        public static DateTime FutureHour(this DateTime dateTime, float hour)
        {
            if (hour < 0)
            {
                hour += 24f;
            }

            float delta = hour - (float)dateTime.TimeOfDay.TotalHours;
            return delta > 0
                ? dateTime.AddHours(delta)
                : dateTime.AddHours(24f + delta);
        }
    }
}
