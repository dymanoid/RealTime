// <copyright file="RealTimeUIDateTimeWrapper.cs" company="dymanoid">
//     Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.UI
{
    using System;
    using System.Globalization;

    /// <summary>
    /// A wrapper that converts a <see cref="DateTime"/> value to a string representation containing
    /// the day of week and the time parts. The configured <see cref="CultureInfo"/> is used for
    /// string conversion.
    /// </summary>
    public sealed class RealTimeUIDateTimeWrapper : UIDateTimeWrapper
    {
        private CultureInfo currentCulture = CultureInfo.CurrentCulture;

        /// <summary>
        /// Initializes a new instance of the <see cref="RealTimeUIDateTimeWrapper"/> class.
        /// </summary>
        /// ///
        /// <param name="initial">The initial <see cref="DateTime"/> value to use for conversion.</param>
        internal RealTimeUIDateTimeWrapper(DateTime initial)
            : base(initial)
        {
            Convert();
        }

        /// <summary>Gets the current date and time value that is processed by this wrapper.</summary>
        public DateTime CurrentValue => m_Value;

        /// <summary>
        /// Checks the specified <see cref="DateTime"/> value whether it should be converted to a
        /// string representation. Converts the value when necessary.
        /// </summary>
        /// ///
        /// <param name="newVal">The <see cref="DateTime"/> value to process.</param>
        public override void Check(DateTime newVal)
        {
            if (m_Value.Minute == newVal.Minute && m_Value.Hour == newVal.Hour && m_Value.DayOfWeek == newVal.DayOfWeek)
            {
                return;
            }

            m_Value = newVal;
            Convert();
        }

        /// <summary>Translates this wrapper using the specified culture information.</summary>
        /// <param name="cultureInfo">The culture information to use for translation.</param>
        /// <exception cref="ArgumentNullException">Thrown when the argument is null.</exception>
        public void Translate(CultureInfo cultureInfo)
        {
            currentCulture = cultureInfo ?? throw new ArgumentNullException(nameof(cultureInfo));
            Convert();
        }

        private void Convert() => m_String = m_Value.ToString("t", currentCulture) + ", " + m_Value.ToString("dddd", currentCulture);
    }
}