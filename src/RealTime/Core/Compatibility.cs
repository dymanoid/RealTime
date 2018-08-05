// <copyright file="Compatibility.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.Core
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using ColossalFramework.Plugins;
    using ColossalFramework.UI;
    using ICities;
    using RealTime.Localization;
    using SkyTools.Localization;
    using SkyTools.Tools;
    using SkyTools.UI;

    /// <summary>
    /// An utility class for checking the compatibility with other installed mods.
    /// </summary>
    internal static class Compatibility
    {
        /// <summary>The Workshop ID of the 'CitizenLifecycleRebalance' mod.</summary>
        public const ulong CitizenLifecycleRebalanceId = 654707599;

        private const string UIInfoPanel = "InfoPanel";
        private const string UIPanelTime = "PanelTime";

        private static readonly HashSet<ulong> IncompatibleModIds = new HashSet<ulong>
        {
            605590542, 672248733, 814698320, 629713122, 702070768, 649522495, 1181352643
        };

        /// <summary>Checks for any enabled incompatible mods and notifies the player when any found.</summary>
        /// <param name="modName">The name of the current mod.</param>
        /// <param name="localizationProvider">The localization provider to use for translation.</param>
        /// <param name="additionalInfo">Additional information to be added to the pop up or message box.</param>
        /// <returns><c>true</c> if the check was successful and no messages were shown; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="modName"/> is null or an empty string.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="localizationProvider"/> is null.</exception>
        public static bool CheckAndNotify(string modName, ILocalizationProvider localizationProvider, string additionalInfo = null)
        {
            if (string.IsNullOrEmpty(modName))
            {
                throw new ArgumentException("The mod name cannot be null or an empty string", nameof(modName));
            }

            if (localizationProvider == null)
            {
                throw new ArgumentNullException(nameof(localizationProvider));
            }

            List<string> incompatibleMods = GetIncompatibleModNames();
            if (incompatibleMods.Count == 0)
            {
                return true;
            }

            string separator = Environment.NewLine + " - ";
            string caption = modName + " - " + localizationProvider.Translate(TranslationKeys.Warning);
            string text = localizationProvider.Translate(TranslationKeys.IncompatibleModsFoundMessage)
                + Environment.NewLine + separator
                + string.Join(separator, incompatibleMods.ToArray())
                + Environment.NewLine + additionalInfo;

            Notify(caption, text);
            return false;
        }

        /// <summary>Notifies the user either with a pop up or with a message box.</summary>
        /// <param name="caption">The caption of the pop up or message box.</param>
        /// <param name="text">The notification text.</param>
        /// <exception cref="ArgumentException">Thrown when any argument is null or an empty string.</exception>
        public static void Notify(string caption, string text)
        {
            if (string.IsNullOrEmpty(caption))
            {
                throw new ArgumentException("The caption cannot be null or an empty string", nameof(caption));
            }

            if (string.IsNullOrEmpty(text))
            {
                throw new ArgumentException("The text cannot be null or an empty string.", nameof(text));
            }

            if (!NotifyWithPopup(caption, text))
            {
                NotifyWithDialog(caption, text);
            }
        }

        /// <summary>
        /// Determines whether a mod with specified Workshop ID is currently installed and enabled.
        /// </summary>
        /// <param name="modId">The mod ID to check.</param>
        /// <returns><c>true</c> if a mod with specified Workshop ID is currently installed and enabled; otherwise, <c>false</c>.</returns>
        public static bool IsModActive(ulong modId)
        {
            return PluginManager.instance.GetPluginsInfo().Any(m => m.isEnabled && m.publishedFileID.AsUInt64 == modId);
        }

        private static void NotifyWithDialog(string caption, string text)
        {
            MessageBox.Show(caption, text);
        }

        private static bool NotifyWithPopup(string caption, string text)
        {
            UIPanel infoPanel = UIView.Find<UIPanel>(UIInfoPanel);
            if (infoPanel == null)
            {
                Log.Warning("No UIPanel found: " + UIInfoPanel);
                return false;
            }

            UIPanel panelTime = infoPanel.Find<UIPanel>(UIPanelTime);
            if (panelTime == null)
            {
                Log.Warning("No UIPanel found: " + UIPanelTime);
                return false;
            }

            Popup.Show(panelTime, caption, text);
            return true;
        }

        private static List<string> GetIncompatibleModNames()
        {
            var result = new List<string>();

            IEnumerable<PluginManager.PluginInfo> incompatibleMods = PluginManager.instance.GetPluginsInfo()
                .Where(m => m.isEnabled && IncompatibleModIds.Contains(m.publishedFileID.AsUInt64));

            try
            {
                foreach (PluginManager.PluginInfo mod in incompatibleMods)
                {
                    var userMod = mod.userModInstance as IUserMod;
                    result.Add(userMod == null ? mod.publishedFileID.AsUInt64.ToString() : userMod.Name);
                }
            }
            catch (Exception ex)
            {
                Log.Warning($"The 'Real Time' mod wanted to check compatibility but failed, error message: {ex}");
            }

            return result;
        }
    }
}
