// <copyright file="RealTimeUIDateTimeWrapper.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.UI
{
    using System;
    using System.Globalization;
    using ColossalFramework.Globalization;

    public sealed class RealTimeUIDateTimeWrapper : UIDateTimeWrapper
    {
        internal RealTimeUIDateTimeWrapper(DateTime def)
            : base(def)
        {
            Convert();
        }

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
