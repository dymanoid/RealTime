// <copyright file="CitiesViewItemFactory.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.UI
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using ColossalFramework.UI;
    using ICities;
    using RealTime.Localization;

    /// <summary>The default implementation of the <see cref="IViewItemFactory"/> interface.</summary>
    /// <seealso cref="IViewItemFactory"/>
    internal sealed class CitiesViewItemFactory : IViewItemFactory
    {
        private readonly UIHelperBase uiHelper;
        private readonly UITabstrip tabStrip;

        /// <summary>Initializes a new instance of the <see cref="CitiesViewItemFactory"/> class.</summary>
        /// <param name="uiHelper">The game's UI helper reference.</param>
        /// <exception cref="ArgumentNullException">Thrown when the argument is null.</exception>
        public CitiesViewItemFactory(UIHelperBase uiHelper)
        {
            this.uiHelper = uiHelper ?? throw new ArgumentNullException(nameof(uiHelper));

            var root = (uiHelper as UIHelper)?.self as UIComponent;
            if (root != null)
            {
                tabStrip = root.AddUIComponent<UITabstrip>();

                UITabContainer tabContainer = root.AddUIComponent<UITabContainer>();
                tabContainer.width = root.width - 20f;
                tabContainer.height = root.height - tabStrip.height - 20f;

                tabStrip.tabPages = tabContainer;
            }
        }

        /// <summary>Creates a new tab item. If it cannot be created, returns a group instead.</summary>
        /// <param name="id">The ID of the tab to create.</param>
        /// <returns>A newly created <see cref="IContainerViewItem"/> instance representing a tab item.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="id"/> is null or empty string.</exception>
        public IContainerViewItem CreateTabItem(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentException("The tab ID cannot be null or empty string", nameof(id));
            }

            IContainerViewItem result = null;
            if (tabStrip != null)
            {
                result = CitiesTabItem.Create(tabStrip, id);
                tabStrip.selectedIndex = tabStrip.tabCount - 1;
            }

            return result ?? new CitiesGroupItem(uiHelper.AddGroup(Constants.Placeholder), id);
        }

        /// <summary>Creates a new group view item.</summary>
        /// <param name="container">The parent container for the created item.</param>
        /// <param name="id">The ID of the group to create.</param>
        /// <returns>A newly created <see cref="IContainerViewItem"/> instance representing a group.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="container"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="id"/> is null or an empty string.</exception>
        public IContainerViewItem CreateGroup(IContainerViewItem container, string id)
        {
            if (container == null)
            {
                throw new ArgumentNullException(nameof(container));
            }

            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentException("The group ID cannot be null or empty string", nameof(id));
            }

            return new CitiesGroupItem(container.Container.AddGroup(Constants.Placeholder), id);
        }

        /// <summary>Creates a new button. If <paramref name="container"/> is not specified, the button is placed into the root element.</summary>
        /// <param name="container">The container.</param>
        /// <param name="id">The ID of the button to create.</param>
        /// <param name="clickHandler">A method that will be called when the button is clicked.</param>
        /// <returns>A newly created <see cref="IViewItem"/> instance representing a button item.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="clickHandler"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="id"/> is null or empty string.</exception>
        public IViewItem CreateButton(IContainerViewItem container, string id, Action clickHandler)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentException("The button ID cannot be null or empty string", nameof(id));
            }

            return new CitiesButtonViewItem(container?.Container ?? uiHelper, id, clickHandler);
        }

        /// <summary>Creates a new check box view item.</summary>
        /// <param name="container">The parent container for the created item.</param>
        /// <param name="id">The ID of the item to create.</param>
        /// <param name="property">The property description that specifies the target property where to store the value.</param>
        /// <param name="configProvider">A method that provides the configuration storage object for the value.</param>
        /// <returns>A newly created <see cref="IViewItem"/> instance representing a check box.</returns>
        /// <exception cref="ArgumentNullException">Thrown when any argument is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="id"/> is an empty string.</exception>
        public IViewItem CreateCheckBox(IContainerViewItem container, string id, PropertyInfo property, Func<object> configProvider)
        {
            return new CitiesCheckBoxItem(container.Container, id, property, configProvider);
        }

        /// <summary>Creates a new slider view item.</summary>
        /// <param name="container">The parent container for the created item.</param>
        /// <param name="id">The ID of the item to create.</param>
        /// <param name="property">The property description that specifies the target property where to store the value.</param>
        /// <param name="configProvider">A method that provides the configuration storage object for the value.</param>
        /// <param name="min">The minimum slider value.</param>
        /// <param name="max">The maximum slider value.</param>
        /// <param name="step">The slider step value.</param>
        /// <param name="valueType">Type of the value to be represented by the slider.</param>
        /// <param name="displayMultiplier">A value that will be multiplied with original values for displaying purposes.</param>
        /// <returns>A newly created <see cref="IViewItem"/> instance representing a slider.</returns>
        /// <exception cref="ArgumentNullException">Thrown when any argument is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="id"/> is an empty string.</exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the <paramref name="max"/> value is less or equal to the <paramref name="min"/> value.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the <paramref name="step"/> value is less or equal to zero.
        /// </exception>
        public IViewItem CreateSlider(
            IContainerViewItem container,
            string id,
            PropertyInfo property,
            Func<object> configProvider,
            float min,
            float max,
            float step,
            SliderValueType valueType,
            float displayMultiplier)
        {
            return new CitiesSliderItem(container.Container, id, property, configProvider, min, max, step, valueType, displayMultiplier);
        }

        /// <summary>Creates a new combo box view item.</summary>
        /// <param name="container">The parent container for the created item.</param>
        /// <param name="id">The ID of the item to create.</param>
        /// <param name="property">The property description that specifies the target property where to store the value.</param>
        /// <param name="configProvider">A method that provides the configuration storage object for the value.</param>
        /// <param name="itemIds">A collection of the item IDs for the combo box values.</param>
        /// <returns>A newly created <see cref="IViewItem"/> instance representing a combo box.</returns>
        /// <exception cref="ArgumentNullException">Thrown when any argument is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="id"/> is an empty string.</exception>
        public IViewItem CreateComboBox(
            IContainerViewItem container,
            string id,
            PropertyInfo property,
            Func<object> configProvider,
            IEnumerable<string> itemIds)
        {
            return new CitiesComboBoxItem(container.Container, id, property, configProvider, itemIds);
        }
    }
}