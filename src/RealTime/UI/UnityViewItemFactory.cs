// <copyright file="UnityViewItemFactory.cs" company="dymanoid">Copyright (c) dymanoid. All rights reserved.</copyright>

namespace RealTime.UI
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using ICities;
    using RealTime.Localization;

    /// <summary>The default implementation of the <see cref="IViewItemFactory"/> interface.</summary>
    /// <seealso cref="IViewItemFactory"/>
    internal sealed class UnityViewItemFactory : IViewItemFactory
    {
        private readonly UIHelperBase uiHelper;

        /// <summary>Initializes a new instance of the <see cref="UnityViewItemFactory"/> class.</summary>
        /// <param name="uiHelper">The game's UI helper reference.</param>
        /// <exception cref="ArgumentNullException">Thrown when the argument is null.</exception>
        public UnityViewItemFactory(UIHelperBase uiHelper)
        {
            this.uiHelper = uiHelper ?? throw new ArgumentNullException(nameof(uiHelper));
        }

        /// <summary>Creates a new group view item.</summary>
        /// <param name="id">The ID of the group to create.</param>
        /// <returns>A newly created <see cref="IContainerViewItem"/> instance representing a group.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="id"/> is null or empty string.</exception>
        public IContainerViewItem CreateGroup(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentException("The group ID cannot be null or empty string", nameof(id));
            }

            return new UnityPageViewItem(uiHelper.AddGroup(Constants.Placeholder), id);
        }

        /// <summary>Creates a new check box view item.</summary>
        /// <param name="container">The parent container for the created item.</param>
        /// <param name="id">The ID of the item to create.</param>
        /// <param name="property">The property description that specifies the target property where to store the value.</param>
        /// <param name="config">The configuration storage object for the value.</param>
        /// <returns>A newly created <see cref="IViewItem"/> instance representing a check box.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when any argument is null.</exception>
        /// <exception cref="System.ArgumentException">Thrown when <paramref name="id"/> is an empty string.</exception>
        public IViewItem CreateCheckBox(IContainerViewItem container, string id, PropertyInfo property, object config)
        {
            return new UnityCheckBoxItem(container.Container, id, property, config);
        }

        /// <summary>Creates a new slider view item.</summary>
        /// <param name="container">The parent container for the created item.</param>
        /// <param name="id">The ID of the item to create.</param>
        /// <param name="property">The property description that specifies the target property where to store the value.</param>
        /// <param name="config">The configuration storage object for the value.</param>
        /// <param name="min">The minimum slider value.</param>
        /// <param name="max">The maximum slider value.</param>
        /// <param name="step">The slider step value.</param>
        /// <param name="valueType">Type of the value to be represented by the slider.</param>
        /// <returns>A newly created <see cref="IViewItem"/> instance representing a slider.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when any argument is null.</exception>
        /// <exception cref="System.ArgumentException">Thrown when <paramref name="id"/> is an empty string.</exception>
        /// <exception cref="System.ArgumentException">
        /// Thrown when the <paramref name="max"/> value is less or equal to the <paramref name="min"/> value.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// Thrown when the <paramref name="step"/> value is less or equal to zero.
        /// </exception>
        public IViewItem CreateSlider(
            IContainerViewItem container,
            string id,
            PropertyInfo property,
            object config,
            float min,
            float max,
            float step,
            SliderValueType valueType)
        {
            return new UnitySliderItem(container.Container, id, property, config, min, max, step, valueType);
        }

        /// <summary>Creates a new combo box view item.</summary>
        /// <param name="container">The parent container for the created item.</param>
        /// <param name="id">The ID of the item to create.</param>
        /// <param name="property">The property description that specifies the target property where to store the value.</param>
        /// <param name="config">The configuration storage object for the value.</param>
        /// <param name="itemIds">A collection of the item IDs for the combo box values.</param>
        /// <returns>A newly created <see cref="IViewItem"/> instance representing a combo box.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when any argument is null.</exception>
        /// <exception cref="System.ArgumentException">Thrown when <paramref name="id"/> is an empty string.</exception>
        public IViewItem CreateComboBox(
            IContainerViewItem container,
            string id,
            PropertyInfo property,
            object config,
            IEnumerable<string> itemIds)
        {
            return new UnityComboBoxItem(container.Container, id, property, config, itemIds);
        }
    }
}