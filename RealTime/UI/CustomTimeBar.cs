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

    internal sealed class CustomTimeBar
    {
        private const string UIInfoPanel = "InfoPanel";
        private const string UIWrapperField = "m_GameTime";
        private const string UIPanelTime = "PanelTime";
        private const string UISprite = "Sprite";
        private const string UILabelTime = "Time";

        private UIDateTimeWrapper originalWrapper;

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
