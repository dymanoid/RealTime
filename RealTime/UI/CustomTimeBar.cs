// <copyright file="CustomTimeBar.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.UI
{
    using System;
    using System.Reflection;
    using ColossalFramework.UI;
    using RealTime.Tools;
    using UnityEngine;

    /// <summary>
    /// Manages the time bar customization. The customized time bar will show the day of the week
    /// and the current time instead of the date. The date will be displayed in the time bar's tooltip.
    /// </summary>
    internal sealed class CustomTimeBar
    {
        private const string UIInfoPanel = "InfoPanel";
        private const string UIWrapperField = "m_GameTime";
        private const string UIPanelTime = "PanelTime";
        private const string UISprite = "Sprite";
        private const string UILabelTime = "Time";

        private UIDateTimeWrapper originalWrapper;

        /// <summary>
        /// Enables the time bar customization. If the customization is already enabled, has no effect.
        /// </summary>
        ///
        /// <param name="currentDate">The current game date to set as the time bar's initial value.</param>
        public void Enable(DateTime currentDate)
        {
            if (originalWrapper != null)
            {
                Log.Warning("Trying to enable the CustomTimeBar multiple times.");
                return;
            }

            var customWrapper = new RealTimeUIDateTimeWrapper(currentDate);
            originalWrapper = SetUIDateTimeWrapper(customWrapper, true);
        }

        /// <summary>
        /// Disables the time bar configuration. If the customization is already disabled or was not enabled,
        /// has no effect.
        /// </summary>
        public void Disable()
        {
            if (originalWrapper == null)
            {
                return;
            }

            SetUIDateTimeWrapper(originalWrapper, false);
            originalWrapper = null;
        }

        private static UIDateTimeWrapper SetUIDateTimeWrapper(UIDateTimeWrapper wrapper, bool customize)
        {
            UIPanel infoPanel = UIView.Find<UIPanel>(UIInfoPanel);
            if (infoPanel == null)
            {
                Log.Warning("No UIPanel found: " + UIInfoPanel);
                return null;
            }

            UISprite progressSprite = GetProgressSprite(infoPanel);
            if (progressSprite != null)
            {
                SetTooltip(progressSprite, customize);

                if (customize)
                {
                    CustomizeTimePanel(progressSprite);
                }
            }

            return ReplaceUIDateTimeWrapperInPanel(infoPanel, wrapper);
        }

        private static UIDateTimeWrapper ReplaceUIDateTimeWrapperInPanel(UIPanel infoPanel, UIDateTimeWrapper wrapper)
        {
            Bindings bindings = infoPanel.GetUIView().GetComponent<Bindings>();
            if (bindings == null)
            {
                Log.Warning($"The UIPanel '{UIInfoPanel}' contains no '{nameof(Bindings)}' component");
                return null;
            }

            FieldInfo field = typeof(Bindings).GetField(UIWrapperField, BindingFlags.GetField | BindingFlags.NonPublic | BindingFlags.Instance);
            if (field == null)
            {
                Log.Warning($"The UIPanel {UIInfoPanel} has no field '{UIWrapperField}'");
                return null;
            }

            var originalWrapper = field.GetValue(bindings) as UIDateTimeWrapper;
            if (originalWrapper == null)
            {
                Log.Warning($"The '{nameof(Bindings)}' component has no '{nameof(UIDateTimeWrapper)}'");
                return null;
            }

            field.SetValue(bindings, wrapper);
            return originalWrapper;
        }

        private static UISprite GetProgressSprite(UIPanel infoPanel)
        {
            UIPanel panelTime = infoPanel.Find<UIPanel>(UIPanelTime);
            if (panelTime == null)
            {
                Log.Warning("No UIPanel found: " + UIPanelTime);
                return null;
            }

            UISprite progressSprite = panelTime.Find<UISprite>(UISprite);
            if (progressSprite == null)
            {
                Log.Warning("No UISprite found: " + UISprite);
                return null;
            }

            return progressSprite;
        }

        private static void CustomizeTimePanel(UISprite progressSprite)
        {
            UILabel dateLabel = progressSprite.Find<UILabel>(UILabelTime);
            if (dateLabel == null)
            {
                Log.Warning("No UILabel found: " + UILabelTime);
                return;
            }

            dateLabel.autoSize = false;
            dateLabel.size = progressSprite.size;
            dateLabel.textAlignment = UIHorizontalAlignment.Center;
            dateLabel.relativePosition = new Vector3(0, 0, 0);
        }

        private static void SetTooltip(UIComponent component, bool customize)
        {
            DateTooltipBehavior tooltipBehavior = component.gameObject.GetComponent<DateTooltipBehavior>();
            if (tooltipBehavior == null && customize)
            {
                tooltipBehavior = component.gameObject.AddComponent<DateTooltipBehavior>();
            }
            else if (tooltipBehavior != null && !customize)
            {
                UnityEngine.Object.Destroy(tooltipBehavior);
            }
        }
    }
}
