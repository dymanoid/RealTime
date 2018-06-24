// <copyright file="UnityViewItem.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.UI
{
    using System;
    using System.Reflection;
    using ColossalFramework.UI;
    using ICities;
    using RealTime.Localization;

    internal abstract class UnityViewItem<TItem, TValue> : IViewItem
        where TItem : UIComponent
    {
        private readonly PropertyInfo property;
        private readonly object config;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors", Justification = "Pure methods invoked")]
        protected UnityViewItem(UIHelperBase uiHelper, string id, PropertyInfo property, object config)
        {
            if (uiHelper == null)
            {
                throw new ArgumentNullException(nameof(uiHelper));
            }

            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentException("The view item ID cannot be null", nameof(id));
            }

            this.property = property ?? throw new ArgumentNullException(nameof(property));
            this.config = config ?? throw new ArgumentNullException(nameof(config));

            TItem component = CreateItem(uiHelper, Value);
            component.name = id;
            UIComponent = component;
        }

        protected TItem UIComponent { get; }

        protected TValue Value
        {
            get => (TValue)Convert.ChangeType(property.GetValue(config, null), typeof(TValue));
            private set => property.SetValue(config, Convert.ChangeType(value, property.PropertyType), null);
        }

        public abstract void Translate(LocalizationProvider localizationProvider);

        protected abstract TItem CreateItem(UIHelperBase uiHelper, TValue defaultValue);

        protected virtual void ValueChanged(TValue newValue)
        {
            Value = newValue;
        }
    }
}
