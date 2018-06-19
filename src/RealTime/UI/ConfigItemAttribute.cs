// <copyright file="ConfigItemAttribute.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.UI
{
    using System;

    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    internal sealed class ConfigItemAttribute : Attribute
    {
        public ConfigItemAttribute(string pageId, uint groupNumber, uint order)
        {
            if (string.IsNullOrEmpty(pageId))
            {
                throw new ArgumentException("The config page ID cannot be null or an empty string");
            }

            PageId = pageId;
            GroupNumber = groupNumber;
            Order = order;
        }

        public string PageId { get; }

        public uint GroupNumber { get; }

        public uint Order { get; }
    }
}
