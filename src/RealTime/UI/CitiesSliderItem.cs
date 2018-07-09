// <copyright file="CitiesSliderItem.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.UI
{
    using System;
    using System.Globalization;
    using System.Reflection;
    using ColossalFramework.UI;
    using ICities;
    using RealTime.Localization;

    /// <summary>A slider view item.</summary>
    internal sealed class CitiesSliderItem : CitiesViewItem<UISlider, float>
    {
        private const string LabelName = "Label";
        private const int SliderValueLabelPadding = 20;

        private readonly UILabel valueLabel;
        private readonly SliderValueType valueType;
        private readonly float displayMultiplier;
        private CultureInfo currentCulture;

        /// <summary>Initializes a new instance of the <see cref="CitiesSliderItem"/> class.</summary>
        /// <param name="uiHelper">The game's UI helper reference.</param>
        /// <param name="id">The view item's unique ID.</param>
        /// <param name="property">
        /// The property description that specifies the target property where to store the value.
        /// </param>
        /// <param name="config">The configuration storage object for the value.</param>
        /// <param name="min">The minimum slider value.</param>
        /// <param name="max">The maximum slider value.</param>
        /// <param name="step">The slider step value. Default is 1.</param>
        /// <param name="valueType">The type of the value to display. Default is <see cref="SliderValueType.Percentage"/>.</param>
        /// <param name="displayMultiplier">A value that will be multiplied with original values for displaying purposes.</param>
        /// <exception cref="ArgumentNullException">Thrown when any argument is null.</exception>
        /// <exception cref="ArgumentException">
        /// thrown when the <paramref name="id"/> is an empty string.
        /// </exception>
        public CitiesSliderItem(
            UIHelperBase uiHelper,
            string id,
            PropertyInfo property,
            object config,
            float min,
            float max,
            float step,
            SliderValueType valueType,
            float displayMultiplier)
            : base(uiHelper, id, property, config)
        {
            UIComponent.minValue = min;
            UIComponent.maxValue = max;
            UIComponent.stepSize = step;
            this.valueType = valueType;
            this.displayMultiplier = displayMultiplier;
            var parentPanel = UIComponent.parent as UIPanel;
            if (parentPanel != null)
            {
                parentPanel.autoLayoutDirection = LayoutDirection.Horizontal;
                parentPanel.autoSize = true;
            }

            if (UIComponent.parent != null)
            {
                valueLabel = UIComponent.parent.AddUIComponent<UILabel>();
                valueLabel.padding.left = SliderValueLabelPadding;
                valueLabel.name = id + LabelName;
                UpdateValueLabel(Value);
            }
        }

        /// <summary>Translates this view item using the specified localization provider.</summary>
        /// <param name="localizationProvider">The localization provider to use for translation.</param>
        /// <exception cref="ArgumentNullException">Thrown when the argument is null.</exception>
        public override void Translate(ILocalizationProvider localizationProvider)
        {
            if (localizationProvider == null)
            {
                throw new ArgumentNullException(nameof(localizationProvider));
            }

            UIComponent panel = UIComponent.parent;
            if (panel == null)
            {
                return;
            }

            panel.tooltip = localizationProvider.Translate(UIComponent.name + TranslationKeys.Tooltip);

            UILabel label = panel.Find<UILabel>(LabelName);
            if (label != null)
            {
                label.text = localizationProvider.Translate(UIComponent.name);
            }

            currentCulture = localizationProvider.CurrentCulture;
            UpdateValueLabel(Value);
        }

        /// <summary>
        /// Refreshes this view item by re-fetching its value from the bound configuration property.
        /// </summary>
        public override void Refresh()
        {
            UIComponent.value = Value;
        }

        /// <summary>Creates the view item using the provided <see cref="UIHelperBase"/>.</summary>
        /// <param name="uiHelper">The UI helper to use for item creation.</param>
        /// <param name="defaultValue">The item's default value.</param>
        /// <returns>A newly created view item.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="uiHelper"/> is null.
        /// </exception>
        protected override UISlider CreateItem(UIHelperBase uiHelper, float defaultValue)
        {
            if (uiHelper == null)
            {
                throw new ArgumentNullException(nameof(uiHelper));
            }

            return (UISlider)uiHelper.AddSlider(Constants.Placeholder, defaultValue, defaultValue + 1, 1, defaultValue, ValueChanged);
        }

        /// <summary>Updates the current configuration item value.</summary>
        /// <param name="newValue">The new item value.</param>
        protected override void ValueChanged(float newValue)
        {
            base.ValueChanged(newValue);
            if (valueLabel == null)
            {
                return;
            }

            UpdateValueLabel(newValue);
        }

        private void UpdateValueLabel(float value)
        {
            string valueString;
            switch (valueType)
            {
                case SliderValueType.Percentage:
                    valueString = (value * displayMultiplier / 100f).ToString("P0", currentCulture ?? CultureInfo.CurrentCulture);
                    break;

                case SliderValueType.Time:
                    valueString = default(DateTime).AddHours(value).ToString("t", currentCulture ?? CultureInfo.CurrentCulture);
                    break;

                case SliderValueType.Duration:
                    TimeSpan ts = TimeSpan.FromHours(value);
                    valueString = $"{ts.Hours}:{ts.Minutes:00}";
                    break;

                default:
                    valueString = (value * displayMultiplier).ToString();
                    break;
            }

            valueLabel.text = valueString;
        }
    }
}