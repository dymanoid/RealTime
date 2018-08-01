// <copyright file="CitiesViewItem.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.UI
{
    using System;
    using System.Reflection;
    using ColossalFramework.UI;
    using ICities;
    using RealTime.Localization;
    using RealTime.Tools;

    /// <summary>A base class for the view items that can be displayed via the game's UI.</summary>
    /// <typeparam name="TItem">The type of the view item.</typeparam>
    /// <typeparam name="TValue">The type of the item's value.</typeparam>
    /// <seealso cref="IViewItem"/>
    internal abstract class CitiesViewItem<TItem, TValue> : IViewItem, IValueViewItem
        where TItem : UIComponent
    {
        private readonly PropertyInfo property;
        private readonly Func<object> configProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="CitiesViewItem{TItem, TValue}"/> class.
        /// </summary>
        /// <param name="uiHelper">The game's UI helper reference.</param>
        /// <param name="id">The view item's unique ID.</param>
        /// <param name="property">
        /// The property description that specifies the target property where to store the value.
        /// </param>
        /// <param name="configProvider">A method that provides the configuration storage object for the value.</param>
        /// <exception cref="ArgumentNullException">Thrown when any argument is null.</exception>
        /// <exception cref="ArgumentException">
        /// thrown when the <paramref name="id"/> is an empty string.
        /// </exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors", Justification = "Pure methods invoked")]
        protected CitiesViewItem(UIHelperBase uiHelper, string id, PropertyInfo property, Func<object> configProvider)
        {
            if (uiHelper == null)
            {
                throw new ArgumentNullException(nameof(uiHelper));
            }

            this.property = property ?? throw new ArgumentNullException(nameof(property));
            this.configProvider = configProvider ?? throw new ArgumentNullException(nameof(configProvider));

            if (id == null)
            {
                throw new ArgumentNullException(nameof(id));
            }
            else if (id.Length == 0)
            {
                throw new ArgumentException("The view item ID cannot be an empty string", nameof(id));
            }

            Id = id;

            TItem component = CreateItem(uiHelper, Value);
            component.name = id;
            UIComponent = component;
        }

        /// <summary>Gets this item's ID.</summary>
        public string Id { get; }

        /// <summary>Gets the created UI component.</summary>
        protected TItem UIComponent { get; }

        /// <summary>Gets current configuration item value.</summary>
        protected TValue Value
        {
            get
            {
                try
                {
                    object currentValue = property.GetValue(configProvider(), null);
                    return (TValue)Convert.ChangeType(currentValue, typeof(TValue));
                }
                catch (Exception ex)
                {
                    Log.Error($"The 'Real Time' mod was unable to read a configuration value, error message: {ex}");
                    return default;
                }
            }

            private set
            {
                try
                {
                    object newValue = property.PropertyType.IsEnum
                        ? Enum.ToObject(property.PropertyType, value)
                        : Convert.ChangeType(value, property.PropertyType);
                    property.SetValue(configProvider(), newValue, null);
                }
                catch (Exception ex)
                {
                    Log.Error($"The 'Real Time' mod was unable to update a configuration value, error message: {ex}");
                }
            }
        }

        /// <summary>
        /// Refreshes this view item by re-fetching its value from the bound configuration property.
        /// </summary>
        public abstract void Refresh();

        /// <summary>
        /// When overridden in derived classes, translates this view item using the specified
        /// localization provider.
        /// </summary>
        /// <param name="localizationProvider">The localization provider to use for translation.</param>
        /// <exception cref="ArgumentNullException">Thrown when the argument is null.</exception>
        public abstract void Translate(ILocalizationProvider localizationProvider);

        /// <summary>Creates the view item using the specified <see cref="UIHelperBase"/>.</summary>
        /// <param name="uiHelper">The UI helper to use for item creation.</param>
        /// <param name="defaultValue">The item's default value.</param>
        /// <returns>A newly created view item.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the argument is null.</exception>
        /// <remarks>IMPORTANT: this method must be a pure method, it must not access any class members!
        /// This virtual method is called by the base class' .ctor, thus the class fields might be
        /// not initialized at that time.</remarks>
        protected abstract TItem CreateItem(UIHelperBase uiHelper, TValue defaultValue);

        /// <summary>Updates the current configuration item value.</summary>
        /// <param name="newValue">The new item value.</param>
        protected virtual void ValueChanged(TValue newValue)
        {
            Value = newValue;
        }
    }
}