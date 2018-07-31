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
    using RealTime.Tools;
    using RealTime.UI;

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
        /// <exception cref="ArgumentException">Thrown when <paramref name="modName"/> is null or an empty string.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="localizationProvider"/> is null.</exception>
        public static void CheckAndNotify(string modName, ILocalizationProvider localizationProvider)
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
                return;
            }

            string separator = Environment.NewLine + " - ";
            string caption = modName + " - " + localizationProvider.Translate(TranslationKeys.Warning);
            string text = localizationProvider.Translate(TranslationKeys.IncompatibleModsFoundMessage)
                + Environment.NewLine + separator
                + string.Join(separator, incompatibleMods.ToArray());

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
