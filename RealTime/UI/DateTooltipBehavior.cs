// <copyright file="DateTooltipBehavior.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.Tools
{
    using System;
    using ColossalFramework.Globalization;
    using ColossalFramework.UI;
    using UnityEngine;

    internal sealed class DateTooltipBehavior : MonoBehaviour
    {
        private UIComponent target;
        private DateTime lastValue;
        private string tooltip;

        public void Start()
        {
            target = gameObject.GetComponent<UIComponent>();
        }

        public void Update()
        {
            if (target == null)
            {
                return;
            }

            DateTime newValue = SimulationManager.instance.m_currentGameTime;
            if (lastValue.Date != newValue.Date)
            {
                tooltip = newValue.ToString("d", LocaleManager.cultureInfo);
            }

            lastValue = newValue;
            target.tooltip = tooltip;
            target.RefreshTooltip();
        }
    }
}
