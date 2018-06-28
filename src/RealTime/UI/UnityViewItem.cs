// <copyright file="UnityViewItem.cs" company="dymanoid">
//     Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.UI
{
    using System;
    using System.Reflection;
    using ColossalFramework.UI;
    using ICities;
    using RealTime.Localization;

    /// <summary>A base class for the view items that can be displayed via the game's UI.</summary>
    /// <typeparam name="TItem">The type of the view item.</typeparam>
    /// <typeparam name="TValue">The type of the item's value.</typeparam>
    /// <seealso cref="IViewItem"/>
    internal abstract class UnityViewItem<TItem, TValue> : IViewItem
        where TItem : UIComponent
    {
        private readonly PropertyInfo property;
        private readonly object config;

        /// <summary>
        /// Initializes a new instance of the <see cref="UnityViewItem{TItem, TValue}"/> class.
        /// </summary>
        /// <param name="uiHelper">The game's UI helper reference.</param>
        /// <param name="id">The view item's unique ID.</param>
        /// <param name="property">
        /// The property description that specifies the target property where to store the value.
        /// </param>
        /// <param name="config">The configuration storage object for the value.</param>
        /// <exception cref="ArgumentNullException">Thrown when any argument is null.</exception>
        /// <exception cref="ArgumentException">
        /// thrown when the <paramref name="id"/> is an empty string.
        /// </exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors", Justification = "Pure methods invoked")]
        protected UnityViewItem(UIHelperBase uiHelper, string id, PropertyInfo property, object config)
        {
            if (uiHelper == null)
            {
                throw new ArgumentNullException(nameof(uiHelper));
            }

            this.property = property ?? throw new ArgumentNullException(nameof(property));
            this.config = config ?? throw new ArgumentNullException(nameof(config));

            TItem component = CreateItem(uiHelper, Value);
            component.name = id ?? throw new ArgumentNullException(nameof(id));
            if (id.Length == 0)
            {
                throw new ArgumentException("The view item ID cannot be an empty string", nameof(id));
            }

            UIComponent = component;
        }

        /// <summary>Gets the created UI component.</summary>
        protected TItem UIComponent { get; }

        /// <summary>Gets current configuration item value.</summary>
        protected TValue Value
        {
            get => (TValue)Convert.ChangeType(property.GetValue(config, null), typeof(TValue));
            private set => property.SetValue(config, Convert.ChangeType(value, property.PropertyType), null);
        }

        /// <summary>
        /// When overridden in derived classes, translates this view item using the specified
        /// localization provider.
        /// </summary>
        /// <param name="localizationProvider">The localization provider to use for translation.</param>
        /// <exception cref="ArgumentNullException">Thrown when the argument is null.</exception>
        public abstract void Translate(LocalizationProvider localizationProvider);

        /// <summary>Creates the view item using the provided <see cref="UIHelperBase"/>.</summary>
        /// <param name="uiHelper">The UI helper to use for item creation.</param>
        /// <param name="defaultValue">The item's default value.</param>
        /// <returns>A newly created view item.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the argument is null.</exception>
        protected abstract TItem CreateItem(UIHelperBase uiHelper, TValue defaultValue);

        /// <summary>Updates the current configuration item value.</summary>
        /// <param name="newValue">The new item value.</param>
        protected virtual void ValueChanged(TValue newValue)
        {
            Value = newValue;
        }
    }
}