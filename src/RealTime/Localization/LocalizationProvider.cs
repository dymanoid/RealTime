// <copyright file="LocalizationProvider.cs" company="dymanoid">Copyright (c) dymanoid. All rights reserved.</copyright>

namespace RealTime.Localization
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Xml;
    using RealTime.Tools;
    using static Constants;

    /// <summary>Manages the mod's localization.</summary>
    internal sealed class LocalizationProvider
    {
        private readonly string localeStorage;
        private readonly Dictionary<string, string> translation = new Dictionary<string, string>();

        /// <summary>Initializes a new instance of the <see cref="LocalizationProvider"/> class.</summary>
        /// <param name="dataPath">The root path.</param>
        /// <exception cref="ArgumentException">Thrown when the argument is null or empty string.</exception>
        public LocalizationProvider(string dataPath)
        {
            if (string.IsNullOrEmpty(dataPath))
            {
                throw new ArgumentException("The data path cannot be null or empty string", nameof(dataPath));
            }

            localeStorage = Path.Combine(dataPath, LocaleFolder);
        }

        private enum LoadingResult
        {
            None,
            Success,
            Failure,
            AlreadyLoaded
        }

        /// <summary>Gets the current culture this object represents.</summary>
        public CultureInfo CurrentCulture { get; private set; } = CultureInfo.CurrentCulture;

        /// <summary>Translates a value that has the specified ID.</summary>
        /// <param name="id">The value ID.</param>
        /// <returns>The translated string value or the <see cref="NoLocale"/> placeholder text on failure.</returns>
        public string Translate(string id)
        {
            if (translation.TryGetValue(id, out string value))
            {
                return value;
            }

            return NoLocale;
        }

        /// <summary>Loads the translation data for the specified language.</summary>
        /// <param name="language">The language to load the translation for.</param>
        /// <returns><c>true</c> when the translation data was loaded; otherwise, <c>false</c>.</returns>
        public bool LoadTranslation(string language)
        {
            if (string.IsNullOrEmpty(language))
            {
                return false;
            }

            LoadingResult result = Load(language);
            if (result == LoadingResult.Failure)
            {
                result = Load("en");
            }

            return result == LoadingResult.Success;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase", Justification = "No security issues here")]
        private static string GetLocaleNameFromLanguage(string language)
        {
            switch (language.ToLowerInvariant())
            {
                case "de":
                    return "de-DE";

                case "es":
                    return "es-ES";

                case "fr":
                    return "fr-FR";

                case "ko":
                    return "ko-KR";

                case "pl":
                    return "pl-PL";

                case "pt":
                    return "pt-PT";

                case "ru":
                    return "ru-RU";

                case "zh":
                    return "zh-CN";

                default:
                    return "en-US";
            }
        }

        private LoadingResult Load(string language)
        {
            if (CurrentCulture.TwoLetterISOLanguageName == language && translation.Count != 0)
            {
                Log.Debug($"The localization data for '{language}' will not be loaded, because it was already loaded");
                return LoadingResult.AlreadyLoaded;
            }

            translation.Clear();

            string path = Path.Combine(localeStorage, language + FileExtension);
            if (!File.Exists(path))
            {
                Log.Error($"The 'Real Time' mod cannot find a required localization file '{path}'");
                return LoadingResult.Failure;
            }

            try
            {
                CurrentCulture = new CultureInfo(GetLocaleNameFromLanguage(language));

                var doc = new XmlDocument();
                doc.Load(path);

                foreach (XmlNode node in doc.DocumentElement.ChildNodes)
                {
                    translation[node.Attributes[XmlKeyAttribute].Value] = node.Attributes[XmlValueAttribute].Value;
                }
            }
            catch (Exception ex)
            {
                Log.Error($"The 'Real Time' cannot load data from localization file '{path}', error message: {ex}");
                translation.Clear();
                return LoadingResult.Failure;
            }

            return LoadingResult.Success;
        }
    }
}