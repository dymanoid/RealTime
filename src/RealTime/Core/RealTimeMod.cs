﻿// <copyright file="RealTimeMod.cs" company="dymanoid">Copyright (c) dymanoid. All rights reserved.</copyright>

namespace RealTime.Core
{
    using System.Linq;
    using ColossalFramework;
    using ColossalFramework.Globalization;
    using ColossalFramework.Plugins;
    using ICities;
    using RealTime.Config;
    using RealTime.Localization;
    using RealTime.UI;
    using SkyTools.Configuration;
    using SkyTools.Localization;
    using SkyTools.Tools;
    using SkyTools.UI;

    /// <summary>The main class of the Real Time mod.</summary>
    public sealed class RealTimeMod : LoadingExtensionBase, IUserMod
    {
        private const long WorkshopId = 1420955187;
        private const string NoWorkshopMessage = "Real Time can only run when subscribed to in Steam Workshop";

        private readonly string modVersion = GitVersion.GetAssemblyVersion(typeof(RealTimeMod).Assembly);
        private readonly string modPath = GetModPath();

        private ConfigurationProvider<RealTimeConfig> configProvider;
        private RealTimeCore core;
        private ConfigUI configUI;
        private LocalizationProvider localizationProvider;

#if BENCHMARK
        /// <summary>
        /// Initializes a new instance of the <see cref="RealTimeMod"/> class.
        /// </summary>
        public RealTimeMod()
        {
            RealTimeBenchmark.Setup();
        }
#endif

        /// <summary>Gets the name of this mod.</summary>
        public string Name => "Real Time";

        /// <summary>Gets the description string of this mod.</summary>
        public string Description => "Adjusts the time flow and the Cims behavior to make them more real. Version: " + modVersion;

        /// <summary>Called when this mod is enabled.</summary>
        public void OnEnabled()
        {
#if DEBUG
            Log.Setup(LogCategory.Generic, LogCategory.Simulation);
#endif

            if (string.IsNullOrEmpty(modPath))
            {
                Log.Info($"The 'Real Time' mod version {modVersion} cannot be started because of no Steam Workshop");
                return;
            }

            Log.Info("The 'Real Time' mod has been enabled, version: " + modVersion);
            configProvider = new ConfigurationProvider<RealTimeConfig>(RealTimeConfig.StorageId, Name, () => new RealTimeConfig(latestVersion: true));
            configProvider.LoadDefaultConfiguration();
            localizationProvider = new LocalizationProvider(Name, modPath);
        }

        /// <summary>Called when this mod is disabled.</summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Must be instance method due to C:S API")]
        public void OnDisabled()
        {
            if (string.IsNullOrEmpty(modPath))
            {
                return;
            }

            CloseConfigUI();
            if (configProvider?.IsDefault == true)
            {
                configProvider.SaveDefaultConfiguration();
            }

            Log.Info("The 'Real Time' mod has been disabled.");
        }

        /// <summary>Called when this mod's settings page needs to be created.</summary>
        /// <param name="helper">
        /// An <see cref="UIHelperBase"/> reference that can be used to construct the mod's settings page.
        /// </param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Must be instance method due to C:S API")]
        public void OnSettingsUI(UIHelperBase helper)
        {
            if (string.IsNullOrEmpty(modPath))
            {
                helper?.AddGroup(NoWorkshopMessage);
                return;
            }

            if (helper == null || configProvider == null)
            {
                return;
            }

            if (configProvider.Configuration == null)
            {
                Log.Warning("The 'Real Time' mod wants to display the configuration page, but the configuration is unexpectedly missing.");
                configProvider.LoadDefaultConfiguration();
            }

            IViewItemFactory itemFactory = new CitiesViewItemFactory(helper);
            CloseConfigUI();
            configUI = ConfigUI.Create(configProvider, itemFactory);
            ApplyLanguage();
        }

        /// <summary>
        /// Called when a game level is loaded. If applicable, activates the Real Time mod for the loaded level.
        /// </summary>
        /// <param name="mode">The <see cref="LoadMode"/> a game level is loaded in.</param>
        public override void OnLevelLoaded(LoadMode mode)
        {
            if (string.IsNullOrEmpty(modPath))
            {
                MessageBox.Show("Sorry", NoWorkshopMessage);
                return;
            }

            switch (mode)
            {
                case LoadMode.LoadGame:
                case LoadMode.NewGame:
                case LoadMode.LoadScenario:
                case LoadMode.NewGameFromScenario:
                    break;

                default:
                    return;
            }

            Log.Info($"The 'Real Time' mod starts, game mode {mode}.");
            core?.Stop();

            var compatibility = Compatibility.Create(localizationProvider);

            bool isNewGame = mode == LoadMode.NewGame || mode == LoadMode.NewGameFromScenario;
            core = RealTimeCore.Run(configProvider, modPath, localizationProvider, isNewGame, compatibility);
            if (core == null)
            {
                Log.Warning("Showing a warning message to user because the mod isn't working");
                MessageBox.Show(
                    localizationProvider.Translate(TranslationKeys.Warning),
                    localizationProvider.Translate(TranslationKeys.ModNotWorkingMessage));
            }
            else
            {
                CheckCompatibility(compatibility);
            }
        }

        /// <summary>
        /// Called when a game level is about to be unloaded. If the Real Time mod was activated for this level,
        /// deactivates the mod for this level.
        /// </summary>
        public override void OnLevelUnloading()
        {
            if (string.IsNullOrEmpty(modPath))
            {
                return;
            }

            if (core != null)
            {
                Log.Info("The 'Real Time' mod stops.");
                core.Stop();
                core = null;
            }

            configProvider.LoadDefaultConfiguration();
        }

        private static string GetModPath()
        {
            var pluginInfo = PluginManager.instance.GetPluginsInfo()
                .FirstOrDefault(pi => pi.publishedFileID.AsUInt64 == WorkshopId);

            return pluginInfo?.modPath;
        }

        private void CheckCompatibility(Compatibility compatibility)
        {
            if (core == null)
            {
                return;
            }

            string message = null;
            bool incompatibilitiesDetected = configProvider.Configuration.ShowIncompatibilityNotifications
                && compatibility.AreAnyIncompatibleModsActive(out message);

            if (core.IsRestrictedMode)
            {
                message += localizationProvider.Translate(TranslationKeys.RestrictedMode);
            }

            if (incompatibilitiesDetected || core.IsRestrictedMode)
            {
                Notification.Notify(Name + " - " + localizationProvider.Translate(TranslationKeys.Warning), message);
            }
        }

        private void ApplyLanguage()
        {
            if (!SingletonLite<LocaleManager>.exists)
            {
                return;
            }

            if (localizationProvider.LoadTranslation(LocaleManager.instance.language))
            {
                localizationProvider.SetEnglishUSFormatsState(configProvider.Configuration.UseEnglishUSFormats);
                core?.Translate(localizationProvider);
            }

            configUI?.Translate(localizationProvider);
        }

        private void CloseConfigUI()
        {
            if (configUI != null)
            {
                configUI.Close();
                configUI = null;
            }
        }
    }
}