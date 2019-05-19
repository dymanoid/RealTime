// <copyright file="CustomCampusWorldInfoPanel.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.UI
{
    using System;
    using ColossalFramework.UI;
    using RealTime.Localization;
    using SkyTools.Localization;
    using SkyTools.UI;
    using UnityEngine;

    /// <summary>
    /// A customized campus info panel that shows the correct length of the academic year.
    /// </summary>
    internal sealed class CustomCampusWorldInfoPanel : CustomInfoPanelBase<CampusWorldInfoPanel>
    {
        private const string GameInfoPanelName = "(Library) CampusWorldInfoPanel";
        private const string ProgressTooltipLabelName = "ProgressTooltipLabel";
        private const string ComponentId = "RealTimeAcademicYearProgress";

        private readonly ILocalizationProvider localizationProvider;
        private UILabel progressTooltipLabel;
        private UILabel originalProgressTooltipLabel;

        private CustomCampusWorldInfoPanel(string infoPanelName, ILocalizationProvider localizationProvider)
            : base(infoPanelName)
        {
            this.localizationProvider = localizationProvider;
        }

        /// <summary>Enables the campus info panel customization. Can return null on failure.</summary>
        ///
        /// <param name="localizationProvider">The localization provider to use for text translation.</param>
        ///
        /// <returns>An instance of the <see cref="CustomCampusWorldInfoPanel"/> object that can be used for disabling
        /// the customization, or null when the customization fails.</returns>
        ///
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="localizationProvider"/> is <c>null</c>.</exception>
        public static CustomCampusWorldInfoPanel Enable(ILocalizationProvider localizationProvider)
        {
            if (localizationProvider == null)
            {
                throw new ArgumentNullException(nameof(localizationProvider));
            }

            var result = new CustomCampusWorldInfoPanel(GameInfoPanelName, localizationProvider);
            return result.Initialize() ? result : null;
        }

        /// <summary>Updates the custom information in this panel.</summary>
        /// <param name="instance">The game object instance to get the information from.</param>
        public override void UpdateCustomInfo(ref InstanceID instance)
        {
            ushort mainGate = DistrictManager.instance.m_parks.m_buffer[instance.Park].m_mainGate;
            ushort eventIndex = BuildingManager.instance.m_buildings.m_buffer[mainGate].m_eventIndex;
            ref EventData eventData = ref EventManager.instance.m_events.m_buffer[eventIndex];

            if (!(eventData.Info.m_eventAI is AcademicYearAI academicYearAI))
            {
                return;
            }

            var endFrame = eventData.m_startFrame + (int)(academicYearAI.m_eventDuration * SimulationManager.DAYTIME_HOUR_TO_FRAME);
            var framesLeft = endFrame - SimulationManager.instance.m_currentFrameIndex;
            if (framesLeft < 0)
            {
                framesLeft = 0;
            }

            float hoursLeft = framesLeft * SimulationManager.DAYTIME_FRAME_TO_HOUR;
            if (hoursLeft < 1f)
            {
                progressTooltipLabel.text = localizationProvider.Translate(TranslationKeys.AcademicYearEndsSoon);
            }
            else if (hoursLeft < 24f)
            {
                string template = localizationProvider.Translate(TranslationKeys.AcademicYearHoursLeft);
                progressTooltipLabel.text = string.Format(template, Mathf.RoundToInt(hoursLeft));
            }
            else
            {
                float daysLeft = hoursLeft / 24f;
                string template = localizationProvider.Translate(TranslationKeys.AcademicYearDaysLeft);
                progressTooltipLabel.text = string.Format(template, (int)Math.Ceiling(daysLeft));
            }
        }

        /// <summary>Destroys the custom UI objects for the info panel.</summary>
        protected override void DisableCore()
        {
            progressTooltipLabel.parent.RemoveUIComponent(progressTooltipLabel);
            UnityEngine.Object.Destroy(progressTooltipLabel.gameObject);
            originalProgressTooltipLabel.isVisible = true;
            progressTooltipLabel = null;
            originalProgressTooltipLabel = null;
        }

        /// <summary>Builds up the custom UI objects for the info panel.</summary>
        /// <returns><c>true</c> on success; otherwise, <c>false</c>.</returns>
        protected override bool InitializeCore()
        {
            originalProgressTooltipLabel = ItemsPanel.parent.Find<UILabel>(ProgressTooltipLabelName);
            if (originalProgressTooltipLabel == null)
            {
                return false;
            }

            progressTooltipLabel = UIComponentTools.CreateCopy(originalProgressTooltipLabel, originalProgressTooltipLabel.parent, ComponentId);
            originalProgressTooltipLabel.isVisible = false;
            return true;
        }
    }
}
