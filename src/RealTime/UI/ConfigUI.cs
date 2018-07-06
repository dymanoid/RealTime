// <copyright file="ConfigUI.cs" company="dymanoid">
//     Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.UI
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using RealTime.Localization;

    /// <summary>Manages the mod's configuration page.</summary>
    internal sealed class ConfigUI
    {
        private readonly IEnumerable<IViewItem> viewItems;

        private ConfigUI(IEnumerable<IViewItem> viewItems)
        {
            this.viewItems = viewItems;
        }

        /// <summary>
        /// Creates the mod's configuration page using the specified object as data source.
        /// </summary>
        /// <param name="config">The configuration object to use as data source.</param>
        /// <param name="itemFactory">The view item factory to use for creating the UI elements.</param>
        /// <returns>A configured instance of the <see cref="ConfigUI"/> class.</returns>
        /// <exception cref="ArgumentNullException">Thrown when any argument is null.</exception>
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

            foreach (var tab in properties.GroupBy(p => p.Attribute.TabId).OrderBy(p => p.Key))
            {
                IContainerViewItem tabItem = itemFactory.CreateTabItem(tab.Key);
                viewItems.Add(tabItem);

                foreach (var group in tab.GroupBy(p => p.Attribute.GroupId).OrderBy(p => p.Key))
                {
                    IContainerViewItem containerItem;
                    if (string.IsNullOrEmpty(group.Key))
                    {
                        containerItem = tabItem;
                    }
                    else
                    {
                        containerItem = itemFactory.CreateGroup(tabItem, group.Key);
                        viewItems.Add(containerItem);
                    }

                    foreach (var item in group.OrderBy(i => i.Attribute.Order))
                    {
                        IViewItem viewItem = CreateViewItem(containerItem, item.Property, config, itemFactory);
                        if (viewItem != null)
                        {
                            viewItems.Add(viewItem);
                        }
                    }
                }
            }

            return new ConfigUI(viewItems);
        }

        /// <summary>Translates the UI using the specified localization provider.</summary>
        /// <param name="localizationProvider">The localization provider to use for translation.</param>
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

                    return itemFactory.CreateSlider(
                        container,
                        property.Name,
                        property,
                        config,
                        slider.Min,
                        slider.Max,
                        slider.Step,
                        slider.ValueType,
                        slider.DisplayMultiplier);

                case ConfigItemCheckBoxAttribute _ when property.PropertyType == typeof(bool):
                    return itemFactory.CreateCheckBox(container, property.Name, property, config);

                case ConfigItemComboBoxAttribute _ when property.PropertyType.IsEnum:
                    return itemFactory.CreateComboBox(container, property.Name, property, config, Enum.GetNames(property.PropertyType));

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