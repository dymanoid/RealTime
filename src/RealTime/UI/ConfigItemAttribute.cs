// <copyright file="ConfigItemAttribute.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.UI
{
    using System;

    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    internal sealed class ConfigItemAttribute : Attribute
    {
        public ConfigItemAttribute(string groupId, uint order)
        {
            if (string.IsNullOrEmpty(groupId))
            {
                throw new ArgumentException("The config group ID cannot be null or an empty string");
            }

            GroupId = groupId;
            Order = order;
        }

        public string GroupId { get; }

        public uint Order { get; }
    }
}
