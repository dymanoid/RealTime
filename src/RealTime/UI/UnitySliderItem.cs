// <copyright file="UnitySliderItem.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.UI
{
    using System.Reflection;
    using ColossalFramework.UI;
    using ICities;
    using RealTime.Localization;

    internal sealed class UnitySliderItem : UnityViewItem<UISlider, float>
    {
        private const string LabelName = "Label";

        public UnitySliderItem(UIHelperBase uiHelper, string id, PropertyInfo property, object config, float min, float max, float step)
            : base(uiHelper, id, property, config)
        {
            UIComponent.minValue = min;
            UIComponent.maxValue = max;
            UIComponent.stepSize = step;
        }

        public override void Translate(LocalizationProvider localizationProvider)
        {
            UIComponent panel = UIComponent.parent;
            if (panel == null)
            {
                return;
            }

            panel.tooltip = localizationProvider.Translate(UIComponent.name + Constants.Tooltip);

            UILabel label = panel.Find<UILabel>(LabelName);
            if (label != null)
            {
                label.text = localizationProvider.Translate(UIComponent.name);
            }
        }

        protected override UISlider CreateItem(UIHelperBase uiHelper, float defaultValue)
        {
            return (UISlider)uiHelper.AddSlider(Constants.Placeholder, defaultValue, defaultValue + 1, 1, defaultValue, ValueChanged);
        }
    }
}
