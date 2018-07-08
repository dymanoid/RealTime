// <copyright file="CitiesGroupItem.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.UI
{
    using ColossalFramework.UI;
    using ICities;
    using RealTime.Localization;

    /// <summary>A group view item.</summary>
    /// <seealso cref="CitiesContainerItemBase" />
    internal sealed class CitiesGroupItem : CitiesContainerItemBase
    {
        private const string LabelName = "Label";

        /// <summary>Initializes a new instance of the <see cref="CitiesGroupItem"/> class.</summary>
        /// <param name="group">The game's group item that represents this container.</param>
        /// <param name="id">The item's unique ID.</param>
        /// <exception cref="ArgumentNullException">Thrown when any argument is null.</exception>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="id"/> is an empty string.
        /// </exception>
        public CitiesGroupItem(UIHelperBase group, string id)
            : base(group, id)
        {
        }

        /// <summary>Performs the actual view item translation.</summary>
        /// <param name="localizationProvider">The localization provider to use for translation. Guaranteed to be not null.</param>
        protected override void TranslateImpl(ILocalizationProvider localizationProvider)
        {
            var content = Container as UIHelper;
            if (content == null)
            {
                return;
            }

            UIComponent panel = ((UIComponent)content.self).parent;
            if (panel == null)
            {
                return;
            }

            UILabel label = panel.Find<UILabel>(LabelName);
            if (label != null)
            {
                label.text = localizationProvider.Translate(ItemId);
            }
        }
    }
}