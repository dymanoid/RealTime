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
        /// <param name="groupId">The ID of the group to place the item into.</param>
        /// <param name="order">The item's sort order.</param>
        /// <exception cref="ArgumentException">
        /// Thrown when the <paramref name="groupId"/> is null or an empty string.
        /// </exception>
        public ConfigItemAttribute(string groupId, uint order)
        {
            if (string.IsNullOrEmpty(groupId))
            {
                throw new ArgumentException("The config group ID cannot be null or an empty string");
            }

            GroupId = groupId;
            Order = order;
        }

        /// <summary>Gets the ID of the group the configuration item is located.</summary>
        public string GroupId { get; }

        /// <summary>Gets the configuration item's sort order.</summary>
        public uint Order { get; }
    }
}