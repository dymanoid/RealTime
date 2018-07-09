// <copyright file="CitiesContainerItemBase.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.UI
{
    using System;
    using ICities;
    using RealTime.Localization;

    /// <summary>
    /// A base class for container items like tab item or group.
    /// </summary>
    /// <seealso cref="IContainerViewItem" />
    internal abstract class CitiesContainerItemBase : IContainerViewItem
    {
        /// <summary>Initializes a new instance of the <see cref="CitiesContainerItemBase"/> class.</summary>
        /// <param name="panel">The game's view item that represents this container.</param>
        /// <param name="id">The item's unique ID.</param>
        /// <exception cref="ArgumentNullException">Thrown when any argument is null.</exception>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="id"/> is an empty string.
        /// </exception>
        protected CitiesContainerItemBase(UIHelperBase panel, string id)
        {
            Container = panel ?? throw new ArgumentNullException(nameof(panel));
            ItemId = id ?? throw new ArgumentNullException(nameof(id));
            if (id.Length == 0)
            {
                throw new ArgumentException("The container item ID cannot be an empty string", nameof(id));
            }
        }

        /// <summary>Gets this object's container game object.</summary>
        public UIHelperBase Container { get; }

        /// <summary>Gets this item's ID.</summary>
        protected string ItemId { get; }

        /// <summary>Translates this view item using the specified localization provider.</summary>
        /// <param name="localizationProvider">The localization provider to use for translation.</param>
        /// <exception cref="ArgumentNullException">Thrown when the argument is null.</exception>
        public void Translate(ILocalizationProvider localizationProvider)
        {
            if (localizationProvider == null)
            {
                throw new ArgumentNullException(nameof(localizationProvider));
            }

            TranslateImpl(localizationProvider);
        }

        /// <summary>When overridden in derived classes, performs the actual view item translation.</summary>
        /// <param name="localizationProvider">The localization provider to use for translation. Guaranteed to be not null.</param>
        protected abstract void TranslateImpl(ILocalizationProvider localizationProvider);
    }
}
