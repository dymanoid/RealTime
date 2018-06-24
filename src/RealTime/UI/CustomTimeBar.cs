// <copyright file="CustomTimeBar.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.UI
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using ColossalFramework.UI;
    using RealTime.Events;
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
        private const string UISpriteProgress = "Sprite";
        private const string UILabelTime = "Time";
        private const string UISpriteEvent = "Event";

        private static readonly Color32 EventColor = new Color32(180, 0, 90, 160);

        private readonly List<ICityEvent> displayedEvents = new List<ICityEvent>();

        private CultureInfo cultureInfo = CultureInfo.CurrentCulture;
        private RealTimeUIDateTimeWrapper customDateTimeWrapper;
        private UIDateTimeWrapper originalWrapper;
        private UISprite progressSprite;

        public event EventHandler<CustomTimeBarClickEventArgs> CityEventClick;

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

            customDateTimeWrapper = new RealTimeUIDateTimeWrapper(currentDate);
            originalWrapper = SetUIDateTimeWrapper(customDateTimeWrapper, true);
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

            RemoveAllCityEvents();
            SetUIDateTimeWrapper(originalWrapper, false);
            originalWrapper = null;
            progressSprite = null;
            customDateTimeWrapper = null;
        }

        public void Translate(CultureInfo cultureInfo)
        {
            this.cultureInfo = cultureInfo ?? throw new ArgumentNullException(nameof(cultureInfo));
            customDateTimeWrapper.Translate(cultureInfo);
            TranslateTooltip(progressSprite, cultureInfo);

            DateTime todayStart = customDateTimeWrapper.CurrentValue.Date;
            DateTime todayEnd = todayStart.AddDays(1).AddMilliseconds(-1);
            foreach (UISprite item in progressSprite.components.Where(c => c.name != null && c.name.StartsWith(UISpriteEvent)))
            {
                SetEventTooltip(item, todayStart, todayEnd);
            }
        }

        public void UpdateEventsDisplay(IEnumerable<ICityEvent> availableEvents)
        {
            DateTime todayStart = customDateTimeWrapper.CurrentValue.Date;
            DateTime todayEnd = todayStart.AddDays(1).AddMilliseconds(-1);

            var eventsToDisplay = availableEvents
                .Where(e => (e.StartTime >= todayStart && e.StartTime <= todayEnd) || (e.EndTime >= todayStart && e.EndTime <= todayEnd))
                .ToList();

            if (displayedEvents.SequenceEqual(eventsToDisplay))
            {
                return;
            }

            RemoveAllCityEvents();

            if (progressSprite == null)
            {
                return;
            }

            displayedEvents.AddRange(eventsToDisplay);
            foreach (ICityEvent cityEvent in displayedEvents)
            {
                DisplayCityEvent(cityEvent, todayStart, todayEnd);
            }
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

            UISprite progressSprite = panelTime.Find<UISprite>(UISpriteProgress);
            if (progressSprite == null)
            {
                Log.Warning("No UISprite found: " + UISpriteProgress);
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

        private static void SetTooltip(UIComponent component, CultureInfo cultureInfo, bool customize)
        {
            DateTooltipBehavior tooltipBehavior = component.gameObject.GetComponent<DateTooltipBehavior>();
            if (tooltipBehavior == null && customize)
            {
                tooltipBehavior = component.gameObject.AddComponent<DateTooltipBehavior>();
                tooltipBehavior.IgnoredComponentNamePrefix = UISpriteEvent;
                tooltipBehavior.Translate(cultureInfo);
            }
            else if (tooltipBehavior != null && !customize)
            {
                UnityEngine.Object.Destroy(tooltipBehavior);
            }
        }

        private static void TranslateTooltip(UIComponent tooltipParent, CultureInfo cultureInfo)
        {
            DateTooltipBehavior tooltipBehavior = tooltipParent.gameObject.GetComponent<DateTooltipBehavior>();
            if (tooltipBehavior != null)
            {
                tooltipBehavior.Translate(cultureInfo);
            }
        }

        private UIDateTimeWrapper SetUIDateTimeWrapper(UIDateTimeWrapper wrapper, bool customize)
        {
            UIPanel infoPanel = UIView.Find<UIPanel>(UIInfoPanel);
            if (infoPanel == null)
            {
                Log.Warning("No UIPanel found: " + UIInfoPanel);
                return null;
            }

            progressSprite = GetProgressSprite(infoPanel);
            if (progressSprite != null)
            {
                SetTooltip(progressSprite, cultureInfo, customize);

                if (customize)
                {
                    CustomizeTimePanel(progressSprite);
                }
            }

            return ReplaceUIDateTimeWrapperInPanel(infoPanel, wrapper);
        }

        private void DisplayCityEvent(ICityEvent cityEvent, DateTime todayStart, DateTime todayEnd)
        {
            float startPercent = cityEvent.StartTime <= todayStart
                ? startPercent = 0
                : (float)cityEvent.StartTime.TimeOfDay.TotalHours / 24f;

            float endPercent = cityEvent.EndTime >= todayEnd
                ? endPercent = 1f
                : (float)cityEvent.EndTime.TimeOfDay.TotalHours / 24f;

            float startPosition = progressSprite.width * startPercent;
            float endPosition = progressSprite.width * endPercent;

            UISprite eventSprite = progressSprite.AddUIComponent<UISprite>();
            eventSprite.name = UISpriteEvent + cityEvent.BuildingId;
            eventSprite.relativePosition = new Vector3(startPosition, 0);
            eventSprite.atlas = progressSprite.atlas;
            eventSprite.spriteName = progressSprite.spriteName;
            eventSprite.height = progressSprite.height;
            eventSprite.width = endPosition - startPosition;
            eventSprite.fillDirection = UIFillDirection.Horizontal;
            eventSprite.color = EventColor;
            eventSprite.fillAmount = 1f;
            eventSprite.objectUserData = cityEvent;
            eventSprite.eventClicked += EventSprite_Clicked;
            SetEventTooltip(eventSprite, todayStart, todayEnd);
        }

        private void SetEventTooltip(UISprite eventSprite, DateTime todayStart, DateTime todayEnd)
        {
            if (eventSprite.objectUserData is ICityEvent cityEvent)
            {
                string startString = cityEvent.StartTime <= todayStart
                ? cityEvent.StartTime.ToString(cultureInfo)
                : cityEvent.StartTime.ToString("t", cultureInfo);

                string endString = cityEvent.EndTime >= todayEnd
                    ? cityEvent.EndTime.ToString(cultureInfo)
                    : cityEvent.EndTime.ToString("t", cultureInfo);

                eventSprite.tooltip = $"{cityEvent.BuildingName} ({startString} - {endString})";
            }
        }

        private void RemoveAllCityEvents()
        {
            if (progressSprite == null)
            {
                return;
            }

            foreach (ICityEvent cityEvent in displayedEvents)
            {
                UISprite sprite = progressSprite.Find<UISprite>(UISpriteEvent + cityEvent.BuildingId);
                if (sprite != null)
                {
                    sprite.eventClicked -= EventSprite_Clicked;
                    UnityEngine.Object.Destroy(sprite);
                }
            }

            displayedEvents.Clear();
        }

        private void EventSprite_Clicked(UIComponent component, UIMouseEventParameter eventParam)
        {
            if (component.objectUserData is ICityEvent cityEvent)
            {
                OnCityEventClick(cityEvent.BuildingId);
            }
        }

        private void OnCityEventClick(ushort buildingId)
        {
            CityEventClick?.Invoke(this, new CustomTimeBarClickEventArgs(buildingId));
        }
    }
}
