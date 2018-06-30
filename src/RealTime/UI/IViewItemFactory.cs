// <copyright file="IViewItemFactory.cs" company="dymanoid">Copyright (c) dymanoid. All rights reserved.</copyright>

namespace RealTime.UI
{
    using System.Collections.Generic;
    using System.Reflection;

    /// <summary>An interface for a factory of view items.</summary>
    internal interface IViewItemFactory
    {
        /// <summary>Creates a new group view item.</summary>
        /// <param name="id">The ID of the group to create.</param>
        /// <returns>A newly created <see cref="IContainerViewItem"/> instance representing a group.</returns>
        /// <exception cref="System.ArgumentException">Thrown when <paramref name="id"/> is null or empty string.</exception>
        IContainerViewItem CreateGroup(string id);

        /// <summary>Creates a new check box view item.</summary>
        /// <param name="container">The parent container for the created item.</param>
        /// <param name="id">The ID of the item to create.</param>
        /// <param name="property">The property description that specifies the target property where to store the value.</param>
        /// <param name="config">The configuration storage object for the value.</param>
        /// <returns>A newly created <see cref="IViewItem"/> instance representing a check box.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when any argument is null.</exception>
        /// <exception cref="System.ArgumentException">Thrown when <paramref name="id"/> is an empty string.</exception>
        IViewItem CreateCheckBox(IContainerViewItem container, string id, PropertyInfo property, object config);

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
        IViewItem CreateSlider(
            IContainerViewItem container,
            string id,
            PropertyInfo property,
            object config,
            float min,
            float max,
            float step,
            SliderValueType valueType);

        /// <summary>Creates a new combo box view item.</summary>
        /// <param name="container">The parent container for the created item.</param>
        /// <param name="id">The ID of the item to create.</param>
        /// <param name="property">The property description that specifies the target property where to store the value.</param>
        /// <param name="config">The configuration storage object for the value.</param>
        /// <param name="itemIds">A collection of the item IDs for the combo box values.</param>
        /// <returns>A newly created <see cref="IViewItem"/> instance representing a combo box.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when any argument is null.</exception>
        /// <exception cref="System.ArgumentException">Thrown when <paramref name="id"/> is an empty string.</exception>
        IViewItem CreateComboBox(
            IContainerViewItem container,
            string id,
            PropertyInfo property,
            object config,
            IEnumerable<string> itemIds);
    }
}