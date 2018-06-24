// <copyright file="UnityCheckBoxItem.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.UI
{
    using System.Reflection;
    using ColossalFramework.UI;
    using ICities;
    using RealTime.Localization;

    internal sealed class UnityCheckBoxItem : UnityViewItem<UICheckBox, bool>
    {
        public UnityCheckBoxItem(UIHelperBase uiHelper, string id, PropertyInfo property, object config)
            : base(uiHelper, id, property, config)
        {
        }

        public override void Translate(LocalizationProvider localizationProvider)
        {
            if (localizationProvider == null)
            {
                throw new System.ArgumentNullException(nameof(localizationProvider));
            }

            UIComponent.text = localizationProvider.Translate(UIComponent.name);
            UIComponent.tooltip = localizationProvider.Translate(UIComponent.name + Constants.Tooltip);
        }

        protected override UICheckBox CreateItem(UIHelperBase uiHelper, bool defaultValue)
        {
            if (uiHelper == null)
            {
                throw new System.ArgumentNullException(nameof(uiHelper));
            }

            return (UICheckBox)uiHelper.AddCheckbox(Constants.Placeholder, defaultValue, ValueChanged);
        }
    }
}
