// <copyright file="CitiesCheckBoxItem.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.UI
{
    using System;
    using System.Reflection;
    using ColossalFramework.UI;
    using ICities;
    using RealTime.Localization;

    /// <summary>A check box item.</summary>
    internal sealed class CitiesCheckBoxItem : CitiesViewItem<UICheckBox, bool>
    {
        /// <summary>Initializes a new instance of the <see cref="CitiesCheckBoxItem"/> class.</summary>
        /// <param name="uiHelper">The game's UI helper reference.</param>
        /// <param name="id">The view item's unique ID.</param>
        /// <param name="property">
        /// The property description that specifies the target property where to store the value.
        /// </param>
        /// <param name="configProvider">A method that provides the configuration storage object for the value.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when any argument is null.</exception>
        /// <exception cref="System.ArgumentException">
        /// thrown when the <paramref name="id"/> is an empty string.
        /// </exception>
        public CitiesCheckBoxItem(UIHelperBase uiHelper, string id, PropertyInfo property, Func<object> configProvider)
            : base(uiHelper, id, property, configProvider)
        {
        }

        /// <summary>Translates this view item using the specified localization provider.</summary>
        /// <param name="localizationProvider">The localization provider to use for translation.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when the argument is null.</exception>
        public override void Translate(ILocalizationProvider localizationProvider)
        {
            if (localizationProvider == null)
            {
                throw new System.ArgumentNullException(nameof(localizationProvider));
            }

            UIComponent.text = localizationProvider.Translate(UIComponent.name);
            UIComponent.tooltip = localizationProvider.Translate(UIComponent.name + TranslationKeys.Tooltip);
        }

        /// <summary>
        /// Refreshes this view item by re-fetching its value from the bound configuration property.
        /// </summary>
        public override void Refresh()
        {
            UIComponent.isChecked = Value;
        }

        /// <summary>Creates the view item using the specified <see cref="UIHelperBase"/>.</summary>
        /// <param name="uiHelper">The UI helper to use for item creation.</param>
        /// <param name="defaultValue">The item's default value.</param>
        /// <returns>A newly created view item.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when the argument is null.</exception>
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