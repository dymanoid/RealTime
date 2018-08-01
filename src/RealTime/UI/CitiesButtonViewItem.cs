// <copyright file="CitiesButtonViewItem.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.UI
{
    using System;
    using ColossalFramework.UI;
    using ICities;
    using RealTime.Localization;

    /// <summary>
    /// A push button view item.
    /// </summary>
    /// <seealso cref="IViewItem" />
    internal sealed class CitiesButtonViewItem : IViewItem
    {
        private readonly UIButton button;

        /// <summary>Initializes a new instance of the <see cref="CitiesButtonViewItem"/> class.</summary>
        /// <param name="uiHelper">The game's UI helper reference.</param>
        /// <param name="id">The view item's unique ID.</param>
        /// <param name="clickHandler">A method that will be called when the button is clicked.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="uiHelper"/> or <paramref name="clickHandler"/> is null.</exception>
        /// <exception cref="ArgumentException">
        /// thrown when the <paramref name="id"/> is null or an empty string.
        /// </exception>
        public CitiesButtonViewItem(UIHelperBase uiHelper, string id, Action clickHandler)
        {
            if (uiHelper == null)
            {
                throw new ArgumentNullException(nameof(uiHelper));
            }

            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentException("The button ID cannot be null or an empty string", nameof(id));
            }

            if (clickHandler == null)
            {
                throw new ArgumentNullException(nameof(clickHandler));
            }

            button = uiHelper.AddButton(Constants.Placeholder, new OnButtonClicked(clickHandler)) as UIButton;
            Id = id;
        }

        /// <summary>Gets this item's ID.</summary>
        public string Id { get; }

        /// <summary>Translates this view item using the specified localization provider.</summary>
        /// <param name="localizationProvider">The localization provider to use for translation.</param>
        /// <exception cref="ArgumentNullException">Thrown when the argument is null.</exception>
        public void Translate(ILocalizationProvider localizationProvider)
        {
            if (localizationProvider == null)
            {
                throw new ArgumentNullException(nameof(localizationProvider));
            }

            if (button != null)
            {
                button.text = localizationProvider.Translate(Id);
            }
        }
    }
}
