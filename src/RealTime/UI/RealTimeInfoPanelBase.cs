// <copyright file="RealTimeInfoPanelBase.cs" company="dymanoid">Copyright (c) dymanoid. All rights reserved.</copyright>

namespace RealTime.UI
{
    using System.Text;
    using ColossalFramework.UI;
    using RealTime.CustomAI;
    using SkyTools.Localization;
    using SkyTools.UI;
    using static Localization.TranslationKeys;

    /// <summary>A base class for the customized world info panels.</summary>
    /// <typeparam name="T">The type of the game world info panel to customize.</typeparam>
    internal abstract class RealTimeInfoPanelBase<T> : CustomInfoPanelBase<T>
        where T : WorldInfoPanel
    {
        private const string ComponentId = "RealTimeInfoSchedule";
        private const string AgeEducationLabelName = "AgeEducation";
        private const float LineHeight = 14f;

        private readonly RealTimeResidentAI<ResidentAI, Citizen> residentAI;
        private readonly ILocalizationProvider localizationProvider;
        private UILabel scheduleLabel;
        private CitizenSchedule scheduleCopy;

        /// <summary>Initializes a new instance of the <see cref="RealTimeInfoPanelBase{T}"/> class.</summary>
        /// <param name="panelName">Name of the game's panel object.</param>
        /// <param name="residentAI">The custom resident AI.</param>
        /// <param name="localizationProvider">The localization provider to use for text translation.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="residentAI"/> or <paramref name="localizationProvider"/> is null.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// Thrown when <paramref name="panelName"/> is null or an empty string.
        /// </exception>
        protected RealTimeInfoPanelBase(string panelName, RealTimeResidentAI<ResidentAI, Citizen> residentAI, ILocalizationProvider localizationProvider)
            : base(panelName)
        {
            this.residentAI = residentAI ?? throw new System.ArgumentNullException(nameof(residentAI));
            this.localizationProvider = localizationProvider ?? throw new System.ArgumentNullException(nameof(localizationProvider));
        }

        /// <summary>Disables the custom citizen info panel, if it is enabled.</summary>
        protected sealed override void DisableCore()
        {
            if (scheduleLabel == null)
            {
                return;
            }

            ItemsPanel.RemoveUIComponent(scheduleLabel);
            UnityEngine.Object.Destroy(scheduleLabel.gameObject);
            scheduleLabel = null;
        }

        /// <summary>Updates the citizen information for the citizen with specified ID.</summary>
        /// <param name="citizenId">The citizen ID.</param>
        protected void UpdateCitizenInfo(uint citizenId)
        {
            if (citizenId == 0)
            {
                SetCustomPanelVisibility(scheduleLabel, visible: false);
                return;
            }

            ref CitizenSchedule schedule = ref residentAI.GetCitizenSchedule(citizenId);

            if (schedule.LastScheduledState == scheduleCopy.LastScheduledState
                && schedule.ScheduledStateTime == scheduleCopy.ScheduledStateTime
                && schedule.WorkStatus == scheduleCopy.WorkStatus
                && schedule.VacationDaysLeft == scheduleCopy.VacationDaysLeft
                && schedule.WorkShift == scheduleCopy.WorkShift)
            {
                return;
            }

            if (schedule.LastScheduledState == ResidentState.Ignored)
            {
                return;
            }

            SetCustomPanelVisibility(scheduleLabel, false);
            scheduleCopy = schedule;
            BuildTextInfo(ref schedule);
        }

        /// <summary>Builds up the custom UI objects for the info panel.</summary>
        /// <returns><c>true</c> on success; otherwise, <c>false</c>.</returns>
        protected sealed override bool InitializeCore()
        {
            UILabel statusLabel = ItemsPanel.Find<UILabel>(AgeEducationLabelName);
            if (statusLabel == null)
            {
                return false;
            }

            scheduleLabel = UIComponentTools.CreateCopy(statusLabel, ItemsPanel, ComponentId);
            scheduleLabel.width = 270;
            scheduleLabel.zOrder = statusLabel.zOrder + 1;
            scheduleLabel.isVisible = false;
            return true;
        }

        private void BuildTextInfo(ref CitizenSchedule schedule)
        {
            var info = new StringBuilder(100);
            float labelHeight = 0;
            if (schedule.LastScheduledState != ResidentState.Unknown)
            {
                string action = localizationProvider.Translate(ScheduledAction + "." + schedule.LastScheduledState.ToString());
                if (!string.IsNullOrEmpty(action))
                {
                    info.Append(localizationProvider.Translate(ScheduledAction)).Append(": ").Append(action);
                    labelHeight += LineHeight;
                }
            }

            if (schedule.ScheduledStateTime != default)
            {
                string action = localizationProvider.Translate(NextScheduledAction);
                if (!string.IsNullOrEmpty(action))
                {
                    if (info.Length > 0)
                    {
                        info.AppendLine();
                    }

                    info.Append(action).Append(": ").Append(schedule.ScheduledStateTime.ToString(localizationProvider.CurrentCulture));
                    labelHeight += LineHeight;
                }
            }

            if (schedule.WorkShift != WorkShift.Unemployed)
            {
                string workShift = localizationProvider.Translate(WorkShiftKey + "." + schedule.WorkShift.ToString());
                if (!string.IsNullOrEmpty(workShift))
                {
                    if (info.Length > 0)
                    {
                        info.AppendLine();
                    }

                    info.Append(workShift);
                    labelHeight += LineHeight;

                    if (schedule.WorkStatus == WorkStatus.OnVacation)
                    {
                        string vacation = localizationProvider.Translate(WorkStatusOnVacation);
                        if (!string.IsNullOrEmpty(vacation))
                        {
                            info.Append(' ');
                            info.AppendFormat(vacation, schedule.VacationDaysLeft);
                        }
                    }
                }
            }

            scheduleLabel.height = labelHeight;
            scheduleLabel.text = info.ToString();
            SetCustomPanelVisibility(scheduleLabel, info.Length > 0);
        }
    }
}