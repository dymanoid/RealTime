// <copyright file="RealTimeMod.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.Core
{
    using ICities;
    using RealTime.Tools;

    /// <summary>
    /// The main class of the Real Time mod.
    /// </summary>
    public sealed class RealTimeMod : IUserMod
    {
        private readonly string modVersion = GitVersion.GetAssemblyVersion(typeof(RealTimeMod).Assembly);

        /// <summary>
        /// Gets the name of this mod.
        /// </summary>
        public string Name => "Real Time v" + modVersion;

        /// <summary>
        /// Gets the description string of this mod.
        /// </summary>
        // TODO: add localization
        public string Description => "Adjusts the time flow in the game to make it more real";

        /// <summary>
        /// Called when this mod is enabled.
        /// </summary>
        public void OnEnabled()
        {
            Log.Info("The 'Real Time' mod has been enabled, version: " + modVersion);
        }

        /// <summary>
        /// Called when this mod is disabled.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Must be instance method due to C:S API")]
        public void OnDisabled()
        {
            Log.Info("The 'Real Time' mod has been disabled.");
        }

        /// <summary>
        /// Called when this mod's settings page needs to be displayed.
        /// </summary>
        ///
        /// <param name="helper">An <see cref="UIHelperBase"/> reference that can be used
        /// to construct the mod's settings page.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Must be instance method due to C:S API")]
        public void OnSettingsUI(UIHelperBase helper)
        {
            // TODO: imlement the options page
        }
    }
}
