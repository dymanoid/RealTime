// <copyright file="CustomTimeBar.cs" company="dymanoid">
//     Copyright (c) dymanoid. All rights reserved.
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
    using SkyTools.Tools;
    using UnityEngine;

    /// <summary>
    /// Manages the time bar customization. The customized time bar will show the day of the week and
    /// the current time instead of the date. The date will be displayed in the time bar's tool tip.
    /// </summary>
    internal sealed class CustomTimeBar
    {
        private const string UIInfoPanel = "InfoPanel";
        private const string UIWrapperField = "m_GameTime";
        private const string UIPanelTime = "PanelTime";
        private const string UISpriteProgress = "Sprite";
        private const string UILabelTime = "Time";
        private const string UISpriteEvent = "Event";

        private const byte EventSpriteOpacity = 160;

        private readonly List<ICityEvent> displayedEvents = new List<ICityEvent>();
        private readonly List<UISprite> displayedEventSprites = new List<UISprite>();
        private readonly List<ICityEvent> eventsToDisplay = new List<ICityEvent>();

        private CultureInfo currentCulture = CultureInfo.CurrentCulture;
        private RealTimeUIDateTimeWrapper customDateTimeWrapper;
        private UIDateTimeWrapper originalWrapper;
        private UISprite progressSprite;

        /// <summary>Occurs when a city event bar is clicked by mouse.</summary>
        public event EventHandler<CustomTimeBarClickEventArgs> CityEventClick;

        /// <summary>
        /// Enables the time bar customization. If the customization is already enabled, has no effect.
        /// </summary>
        /// <param name="currentDate">The current game date to set as the time bar's initial value.</param>
        public void Enable(DateTime currentDate)
        {
            if (originalWrapper != null)
            {
                Log.Warning("Trying to enable the CustomTimeBar multiple times.");
                return;
            }

            customDateTimeWrapper = new RealTimeUIDateTimeWrapper(currentDate);
            originalWrapper = SetUIDateTimeWrapper(customDateTimeWrapper, customize: true);
            SetEventColorUpdater(progressSprite, enabled: true);
        }

        /// <summary>
        /// Disables the time bar configuration. If the customization is already disabled or was not
        /// enabled, has no effect.
        /// </summary>
        public void Disable()
        {
            if (originalWrapper == null)
            {
                return;
            }

            RemoveAllCityEvents();
            SetUIDateTimeWrapper(originalWrapper, customize: false);
            SetEventColorUpdater(progressSprite, enabled: false);
            originalWrapper = null;
            progressSprite = null;
            customDateTimeWrapper = null;
        }

        /// <summary>Translates the time bar using the specified culture information.</summary>
        /// <param name="cultureInfo">The culture information to use for translation.</param>
        /// <exception cref="ArgumentNullException">Thrown when the argument is null.</exception>
        public void Translate(CultureInfo cultureInfo)
        {
            currentCulture = cultureInfo ?? throw new ArgumentNullException(nameof(cultureInfo));
            customDateTimeWrapper.Translate(cultureInfo);
            TranslateTooltip(progressSprite, cultureInfo);

            DateTime todayStart = customDateTimeWrapper.CurrentValue.Date;
            DateTime todayEnd = todayStart.AddDays(1).AddMilliseconds(-1);

            IEnumerable<UISprite> sprites = progressSprite.components
                .OfType<UISprite>()
                .Where(c => c.name?.StartsWith(UISpriteEvent, StringComparison.Ordinal) == true);

            foreach (var item in sprites)
            {
                SetEventTooltip(item, todayStart, todayEnd);
            }
        }

        /// <summary>Updates the events bars on this time bar.</summary>
        /// <param name="availableEvents">The currently available events that need to be displayed.</param>
        public void UpdateEventsDisplay(IReadOnlyList<ICityEvent> availableEvents)
        {
            DateTime todayStart = customDateTimeWrapper.CurrentValue.Date;
            DateTime todayEnd = todayStart.AddDays(1).AddMilliseconds(-1);

            eventsToDisplay.Clear();
            for (int i = 0; i < availableEvents.Count; ++i)
            {
                var e = availableEvents[i];
                if (e.StartTime >= todayStart && e.StartTime <= todayEnd || e.EndTime >= todayStart && e.EndTime <= todayEnd)
                {
                    eventsToDisplay.Add(e);
                }
            }

            if (EventListsEqual(displayedEvents, eventsToDisplay))
            {
                return;
            }

            RemoveAllCityEvents();

            if (progressSprite == null)
            {
                return;
            }

            displayedEvents.AddRange(eventsToDisplay);
            foreach (var cityEvent in displayedEvents)
            {
                DisplayCityEvent(cityEvent, todayStart, todayEnd);
            }
        }

        /// <summary>
        /// Updates the colors of currently displayed event bars.
        /// </summary>
        public void UpdateEventsColors()
        {
            if (progressSprite == null)
            {
                return;
            }

            foreach (var sprite in displayedEventSprites)
            {
                var cityEvent = (ICityEvent)sprite.objectUserData;
                sprite.color = GetColor(cityEvent.Color, EventSpriteOpacity);
            }
        }

        private static Color32 GetColor(EventColor color, byte alpha) => new Color32(color.Red, color.Green, color.Blue, alpha);

        private static bool EventListsEqual(List<ICityEvent> first, List<ICityEvent> second)
        {
            if (first.Count != second.Count)
            {
                return false;
            }

            for (int i = 0; i < first.Count; ++i)
            {
                if (first[i] != second[i])
                {
                    return false;
                }
            }

            return true;
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

            if (!(field.GetValue(bindings) is UIDateTimeWrapper originalWrapper))
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
            tooltipBehavior?.Translate(cultureInfo);
        }

        private void SetEventColorUpdater(UIComponent component, bool enabled)
        {
            var updateBehavior = component.gameObject.GetComponent<EventColorsUpdateBehavior>();
            if (enabled && updateBehavior == null)
            {
                updateBehavior = component.gameObject.AddComponent<EventColorsUpdateBehavior>();
                updateBehavior.TimeBar = this;
            }
            else if (!enabled && updateBehavior != null)
            {
                UnityEngine.Object.Destroy(updateBehavior);
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
                SetTooltip(progressSprite, currentCulture, customize);

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
                ? 0
                : (float)cityEvent.StartTime.TimeOfDay.TotalHours / 24f;

            float endPercent = cityEvent.EndTime >= todayEnd
                ? 1f
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
            eventSprite.color = GetColor(cityEvent.Color, EventSpriteOpacity);
            eventSprite.fillAmount = 1f;
            eventSprite.objectUserData = cityEvent;
            eventSprite.eventClicked += EventSprite_Clicked;
            SetEventTooltip(eventSprite, todayStart, todayEnd);
            displayedEventSprites.Add(eventSprite);
        }

        private void SetEventTooltip(UISprite eventSprite, DateTime todayStart, DateTime todayEnd)
        {
            if (eventSprite.objectUserData is ICityEvent cityEvent)
            {
                string startString = cityEvent.StartTime <= todayStart
                ? cityEvent.StartTime.ToString(currentCulture)
                : cityEvent.StartTime.ToString("t", currentCulture);

                string endString = cityEvent.EndTime >= todayEnd
                    ? cityEvent.EndTime.ToString(currentCulture)
                    : cityEvent.EndTime.ToString("t", currentCulture);

                eventSprite.tooltip = $"{cityEvent.BuildingName} ({startString} - {endString})";
            }
        }

        private void RemoveAllCityEvents()
        {
            if (progressSprite == null)
            {
                return;
            }

            foreach (var sprite in displayedEventSprites)
            {
                sprite.eventClicked -= EventSprite_Clicked;
                UnityEngine.Object.Destroy(sprite);
            }

            displayedEventSprites.Clear();
            displayedEvents.Clear();
        }

        private void EventSprite_Clicked(UIComponent component, UIMouseEventParameter eventParam)
        {
            if (component.objectUserData is ICityEvent cityEvent)
            {
                OnCityEventClick(cityEvent.BuildingId);
            }
        }

        private void OnCityEventClick(ushort buildingId) => CityEventClick?.Invoke(this, new CustomTimeBarClickEventArgs(buildingId));

        private sealed class EventColorsUpdateBehavior : MonoBehaviour
        {
            public CustomTimeBar TimeBar { get; set; }

            public void Update() => TimeBar?.UpdateEventsColors();
        }
    }
}