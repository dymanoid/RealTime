// <copyright file="SerializationTools.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.Tools
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Xml.Serialization;

    /// <summary>
    /// Various tools for the XML serialization.
    /// </summary>
    internal static class SerializationTools
    {
        /// <summary>Gets a <see cref="XmlAttributeOverrides"/> instance that tells the XML serializer to
        /// ignore the obsolete properties of the specified object.</summary>
        /// <param name="item">The object to process.</param>
        /// <returns>A <see cref="XmlAttributeOverrides"/> instance that can be consumed by the XML serializer.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the argument is null.</exception>
        public static XmlAttributeOverrides IgnoreObsoleteProperties(object item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            Type itemType = item.GetType();

            IEnumerable<PropertyInfo> properties = itemType
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(p => p.GetCustomAttributes(typeof(ObsoleteAttribute), false).Length != 0);

            var result = new XmlAttributeOverrides();

            foreach (PropertyInfo property in properties)
            {
                var attributes = new XmlAttributes
                {
                    XmlIgnore = true
                };

                attributes.XmlElements.Add(new XmlElementAttribute(property.Name));
                result.Add(itemType, property.Name, attributes);
            }

            return result;
        }
    }
}
