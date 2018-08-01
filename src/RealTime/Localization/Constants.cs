﻿// <copyright file="Constants.cs" company="dymanoid">Copyright (c) dymanoid. All rights reserved.</copyright>

namespace RealTime.Localization
{
    /// <summary>A container class for the common constants.</summary>
    internal static class Constants
    {
        /// <summary>The placeholder string.</summary>
        public const string Placeholder = "Placeholder";

        /// <summary>The 'no locale' placeholder string.</summary>
        public const string NoLocale = "No Locale found";

        /// <summary>The directory name containing the localization files.</summary>
        public const string LocaleFolder = "Localization";

        /// <summary>The localization files extension.</summary>
        public const string FileExtension = ".xml";

        /// <summary>The XML item key attribute name.</summary>
        public const string XmlKeyAttribute = "id";

        /// <summary>The XML translation node name.</summary>
        public const string XmlTranslationNodeName = "translation";

        /// <summary>The XML item value attribute name.</summary>
        public const string XmlValueAttribute = "value";

        /// <summary>The XML override node name.</summary>
        public const string XmlOverrideNodeName = "overrides";

        /// <summary>The XML override node 'type' attribute name.</summary>
        public const string XmlOverrideTypeAttribute = "type";
    }
}