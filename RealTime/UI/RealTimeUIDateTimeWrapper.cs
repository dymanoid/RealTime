// <copyright file="RealTimeUIDateTimeWrapper.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.UI
{
    using System;
    using System.Globalization;
    using ColossalFramework.Globalization;

    /// <summary>
    /// A wrapper that converts a <see cref="DateTime"/> value to a string representation
    /// containing the day of week and the time parts. The current <see cref="LocaleManager.cultureInfo"/>
    /// is used for string conversion.
    /// </summary>
    public sealed class RealTimeUIDateTimeWrapper : UIDateTimeWrapper
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RealTimeUIDateTimeWrapper"/> class.
        /// </summary>
        ///
        /// <param name="initial">The initial <see cref="DateTime"/> value to use for conversion.</param>
        internal RealTimeUIDateTimeWrapper(DateTime initial)
            : base(initial)
        {
            Convert();
        }

        /// <summary>
        /// Checks the provided <see cref="DateTime"/> value whether it should be converted to a
        /// string representation. Converts the value when necessary.
        /// </summary>
        ///
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

        private void Convert()
        {
            CultureInfo cultureInfo = LocaleManager.cultureInfo;
            m_String = m_Value.ToString("t", cultureInfo) + ", " + m_Value.ToString("dddd", cultureInfo);
        }
    }
}
