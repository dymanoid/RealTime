// <copyright file="ConfigItemAttribute.cs" company="dymanoid">
//     Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.UI
{
    using System;

    /// <summary>
    /// An attribute for the mod's configuration items. Specifies the placement of the item on the
    /// mod's configuration page.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    internal sealed class ConfigItemAttribute : Attribute
    {
        /// <summary>Initializes a new instance of the <see cref="ConfigItemAttribute"/> class.</summary>
        /// <param name="tabId">The ID of the tab item to place the item into.</param>
        /// <param name="groupId">The ID of the group to place the item into. If null or empty string,
        /// the item will be placed directly in the tab item.</param>
        /// <param name="order">The item's sort order.</param>
        /// <exception cref="ArgumentException">
        /// Thrown when the <paramref name="tabId"/> is null or an empty string.
        /// </exception>
        public ConfigItemAttribute(string tabId, string groupId, uint order)
        {
            if (string.IsNullOrEmpty(tabId))
            {
                throw new ArgumentException("The config tab item ID cannot be null or an empty string");
            }

            TabId = tabId;
            GroupId = groupId;
            Order = order;
        }

        /// <summary>Initializes a new instance of the <see cref="ConfigItemAttribute"/> class.</summary>
        /// <param name="tabId">The ID of the tab item to place the item into.</param>
        /// <param name="order">The item's sort order.</param>
        /// <exception cref="ArgumentException">
        /// Thrown when the <paramref name="tabId"/> is null or an empty string.
        /// </exception>
        public ConfigItemAttribute(string tabId, uint order)
            : this(tabId, null, order)
        {
        }

        /// <summary>Gets the ID of the tab item the configuration item is located.</summary>
        public string TabId { get; }

        /// <summary>Gets the ID of the group the configuration item is located.
        /// If not set, the item is located directly in the tab item.</summary>
        public string GroupId { get; }

        /// <summary>Gets the configuration item's sort order.</summary>
        public uint Order { get; }
    }
}