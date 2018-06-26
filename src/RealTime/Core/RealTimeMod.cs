// <copyright file="RealTimeMod.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.Core
{
    using System;
    using System.Linq;
    using System.Security.Permissions;
    using ColossalFramework;
    using ColossalFramework.Globalization;
    using ColossalFramework.Plugins;
    using ICities;
    using RealTime.Config;
    using RealTime.Localization;
    using RealTime.Tools;
    using RealTime.UI;

    /// <summary>
    /// The main class of the Real Time mod.
    /// </summary>
    public sealed class RealTimeMod : LoadingExtensionBase, IUserMod
    {
        private const long WorkshopId = 1420955187;

        private readonly string modVersion = GitVersion.GetAssemblyVersion(typeof(RealTimeMod).Assembly);
        private readonly string modPath = GetModPath();

        private RealTimeConfig config;
        private RealTimeCore core;
        private ConfigUI configUI;
        private LocalizationProvider localizationProvider;

        /// <summary>
        /// Gets the name of this mod.
        /// </summary>
        public string Name => "Real Time";

        /// <summary>
        /// Gets the description string of this mod.
        /// </summary>
        public string Description => "Adjusts the time flow and the Cims behavior to make them more real. Version: " + modVersion;

        /// <summary>
        /// Called when this mod is enabled.
        /// </summary>
        public void OnEnabled()
        {
            Log.Info("The 'Real Time' mod has been enabled, version: " + modVersion);
            config = ConfigurationProvider.LoadConfiguration();
            localizationProvider = new LocalizationProvider(modPath);
        }

        /// <summary>
        /// Called when this mod is disabled.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Must be instance method due to C:S API")]
        public void OnDisabled()
        {
            Log.Info("The 'Real Time' mod has been disabled.");
            ConfigurationProvider.SaveConfiguration(config);
            config = null;
            configUI = null;
        }

        /// <summary>
        /// Called when this mod's settings page needs to be created.
        /// </summary>
        ///
        /// <param name="helper">An <see cref="UIHelperBase"/> reference that can be used
        /// to construct the mod's settings page.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Must be instance method due to C:S API")]
        public void OnSettingsUI(UIHelperBase helper)
        {
            IViewItemFactory itemFactory = new UnityViewItemFactory(helper);
            configUI = ConfigUI.Create(config, itemFactory);
            ApplyLanguage();
        }

        /// <summary>
        /// Calles when a game level is loaded. If applicable, activates the Real Time mod
        /// for the loaded level.
        /// </summary>
        ///
        /// <param name="mode">The <see cref="LoadMode"/> a game level is loaded in.</param>
        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public override void OnLevelLoaded(LoadMode mode)
        {
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

            core = RealTimeCore.Run(config, modPath, localizationProvider);
        }

        /// <summary>
        /// Calles when a game level is about to be unloaded. If the Real Time mod was activated
        /// for this level, deactivates the mod for this level.
        /// </summary>
        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public override void OnLevelUnloading()
        {
            if (core != null)
            {
                core.Stop();
                core = null;
            }

            ConfigurationProvider.SaveConfiguration(config);
        }

        private static string GetModPath()
        {
            string assemblyName = typeof(RealTimeMod).Assembly.GetName().Name;

            PluginManager.PluginInfo pluginInfo = PluginManager.instance.GetPluginsInfo()
                .FirstOrDefault(pi => pi.name == assemblyName || pi.publishedFileID.AsUInt64 == WorkshopId);

            return pluginInfo == null
                ? Environment.CurrentDirectory
                : pluginInfo.modPath;
        }

        private void ApplyLanguage()
        {
            if (!SingletonLite<LocaleManager>.exists)
            {
                return;
            }

            if (localizationProvider.LoadTranslation(LocaleManager.instance.language))
            {
                core?.Translate(localizationProvider);
            }

            configUI?.Translate(localizationProvider);
        }
    }
}
