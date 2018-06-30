// <copyright file="UnityPageViewItem.cs" company="dymanoid">
//     Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.UI
{
    using System;
    using ColossalFramework.UI;
    using ICities;
    using RealTime.Localization;

    /// <summary>A page view item.</summary>
    /// <seealso cref="IContainerViewItem"/>
    internal sealed class UnityPageViewItem : IContainerViewItem
    {
        private const string LabelName = "Label";
        private readonly string id;

        /// <summary>Initializes a new instance of the <see cref="UnityPageViewItem"/> class.</summary>
        /// <param name="page">The game's page item.</param>
        /// <param name="id">The page's unique ID.</param>
        /// <exception cref="ArgumentException">Thrown when any argument is null.</exception>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="id"/> is an empty string.
        /// </exception>
        public UnityPageViewItem(UIHelperBase page, string id)
        {
            Container = page ?? throw new ArgumentNullException(nameof(page));
            this.id = id ?? throw new ArgumentNullException(nameof(id));
            if (id.Length == 0)
            {
                throw new ArgumentException("The page ID cannot be an empty string", nameof(id));
            }
        }

        /// <summary>Gets this object's container game object.</summary>
        public UIHelperBase Container { get; }

        /// <summary>Translates this view item using the specified localization provider.</summary>
        /// <param name="localizationProvider">The localization provider to use for translation.</param>
        /// <exception cref="ArgumentNullException">Thrown when the argument is null.</exception>
        public void Translate(LocalizationProvider localizationProvider)
        {
            if (localizationProvider == null)
            {
                throw new ArgumentNullException(nameof(localizationProvider));
            }

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
                label.text = localizationProvider.Translate(id);
            }
        }
    }
}