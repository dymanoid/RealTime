// <copyright file="ILocalizationProvider.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.Localization
{
    using System.Collections.Generic;
    using System.Globalization;

    /// <summary>
    /// An interface for a class that can handle the localization.
    /// </summary>
    internal interface ILocalizationProvider
    {
        /// <summary>Gets the current culture that is used for translation.</summary>
        CultureInfo CurrentCulture { get; }

        /// <summary>Translates a value that has the specified ID.</summary>
        /// <param name="id">The value ID.</param>
        /// <returns>The translated string value or the <see cref="Constants.NoLocale"/> placeholder text on failure.</returns>
        string Translate(string id);

        /// <summary>Gets a dictionary representing the game's translations that should be overridden
        /// by this mod. Can return null.</summary>
        /// <param name="type">The overridden translations type string.</param>
        /// <returns>A map of key-value pairs for translations to override, or null.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when the argument is null.</exception>
        IDictionary<string, string> GetOverriddenTranslations(string type);
    }
}