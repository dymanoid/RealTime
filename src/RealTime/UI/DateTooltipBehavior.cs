// <copyright file="DateTooltipBehavior.cs" company="dymanoid">
//     Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.UI
{
    using System;
    using System.Globalization;
    using ColossalFramework.UI;
    using UnityEngine;

    /// <summary>
    /// A script that can be attached to any <see cref="UIComponent"/>. Observes the
    /// <see cref="SimulationManager.m_currentGameTime"/> value and sets the tool tip of the
    /// <see cref="UIComponent"/> to the date part of that value. The configured
    /// <see cref="CultureInfo"/> is used for string conversion.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Instantiated by Unity Engine")]
    internal sealed class DateTooltipBehavior : MonoBehaviour
    {
        private UIComponent target;
        private DateTime lastValue;
        private string tooltip;
        private CultureInfo currentCulture = CultureInfo.CurrentCulture;

        /// <summary>
        /// Gets or sets the name prefix of the child components whose tool tips should not be updated.
        /// </summary>
        public string IgnoredComponentNamePrefix { get; set; }

        /// <summary>Translates the tool tip behavior using the specified culture information.</summary>
        /// <param name="cultureInfo">The culture information to use for translation.</param>
        /// <exception cref="ArgumentNullException">Thrown when the argument is null.</exception>
        public void Translate(CultureInfo cultureInfo)
        {
            currentCulture = cultureInfo ?? throw new ArgumentNullException(nameof(cultureInfo));
            UpdateTooltip(force: true);
        }

        /// <summary>
        /// <see cref="Start"/> is called on the frame when a script is enabled just before any of
        /// the <see cref="Update"/> methods are called the first time.
        /// </summary>
        public void Start()
        {
            target = gameObject.GetComponent<UIComponent>();
        }

        /// <summary>
        /// <see cref="Update"/> is called every frame, if the <see cref="MonoBehaviour"/> is enabled.
        /// </summary>
        public void Update()
        {
            UpdateTooltip();
        }

        private void UpdateTooltip(bool force = false)
        {
            if (target == null)
            {
                return;
            }

            DateTime newValue = SimulationManager.instance.m_currentGameTime;
            if (lastValue.Date != newValue.Date || force)
            {
                tooltip = newValue.ToString("d", currentCulture);
            }

            lastValue = newValue;

            if (!target.containsMouse)
            {
                return;
            }

            if (!string.IsNullOrEmpty(IgnoredComponentNamePrefix))
            {
                UIComponent hovered = UIInput.hoveredComponent;
                if (hovered?.name != null && hovered.name.StartsWith(IgnoredComponentNamePrefix, StringComparison.Ordinal))
                {
                    return;
                }
            }

            target.tooltip = tooltip;
            target.RefreshTooltip();
        }
    }
}