// <copyright file="LocalizationProvider.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.Localization
{
    using System.Collections.Generic;
    using System.IO;
    using System.Xml;
    using static Constants;

    internal sealed class LocalizationProvider
    {
        private readonly string localeStorage;
        private readonly Dictionary<string, string> translation = new Dictionary<string, string>();

        public LocalizationProvider(string rootPath)
        {
            localeStorage = Path.Combine(rootPath, LocaleFolder);
        }

        public string Translate(string id)
        {
            if (translation.TryGetValue(id, out string value))
            {
                return value;
            }

            return NoLocale;
        }

        public void LoadTranslation(string language)
        {
            if (!Load(language))
            {
                Load("en");
            }
        }

        private bool Load(string language)
        {
            translation.Clear();

            string path = Path.Combine(localeStorage, language + FileExtension);
            if (!File.Exists(path))
            {
                return false;
            }

            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(path);

                foreach (XmlNode node in doc.DocumentElement.ChildNodes)
                {
                    translation[node.Attributes[XmlKeyAttribute].Value] = node.Attributes[XmlValueAttribute].Value;
                }
            }
            catch
            {
                translation.Clear();
                return false;
            }

            return true;
        }
    }
}
