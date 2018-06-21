// <copyright file="ConfigUI.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.UI
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using RealTime.Localization;

    internal sealed class ConfigUI
    {
        private readonly IEnumerable<IViewItem> viewItems;

        private ConfigUI(IEnumerable<IViewItem> viewItems)
        {
            this.viewItems = viewItems;
        }

        public static ConfigUI Create(object config, IViewItemFactory itemFactory)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            if (itemFactory == null)
            {
                throw new ArgumentNullException(nameof(itemFactory));
            }

            var properties = config.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Select(p => new { Property = p, Attribute = GetCustomItemAttribute<ConfigItemAttribute>(p) })
                .Where(v => v.Attribute != null);

            var viewItems = new List<IViewItem>();

            foreach (var group in properties.GroupBy(p => p.Attribute.GroupId).OrderBy(p => p.Key))
            {
                IContainerViewItem groupItem = itemFactory.CreateGroup(group.Key);
                viewItems.Add(groupItem);

                foreach (var item in group.OrderBy(i => i.Attribute.Order))
                {
                    IViewItem viewItem = CreateViewItem(groupItem, item.Property, config, itemFactory);
                    if (viewItem != null)
                    {
                        viewItems.Add(viewItem);
                    }
                }
            }

            return new ConfigUI(viewItems);
        }

        public void Translate(LocalizationProvider localizationProvider)
        {
            foreach (IViewItem item in viewItems)
            {
                item.Translate(localizationProvider);
            }
        }

        private static IViewItem CreateViewItem(IContainerViewItem container, PropertyInfo property, object config, IViewItemFactory itemFactory)
        {
            switch (GetCustomItemAttribute<ConfigItemUIBaseAttribute>(property))
            {
                case ConfigItemSliderAttribute slider when property.PropertyType.IsPrimitive:
                    if (property.PropertyType == typeof(bool) || property.PropertyType == typeof(IntPtr)
                        || property.PropertyType == typeof(UIntPtr) || property.PropertyType == typeof(char))
                    {
                        goto default;
                    }

                    return itemFactory.CreateSlider(container, property.Name, property, config, slider.Min, slider.Max, slider.Step, slider.ValueType);

                case ConfigItemCheckBoxAttribute checkbox when property.PropertyType == typeof(bool):
                    return itemFactory.CreateCheckBox(container, property.Name, property, config);

                default:
                    return null;
            }
        }

        private static T GetCustomItemAttribute<T>(PropertyInfo property, bool inherit = false)
            where T : Attribute
        {
            return (T)property.GetCustomAttributes(typeof(T), inherit).FirstOrDefault();
        }
    }
}
