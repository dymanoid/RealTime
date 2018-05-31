// <copyright file="RealTimeMod.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.Core
{
    using ICities;
    using RealTime.Tools;

    public sealed class RealTimeMod : IUserMod
    {
        private readonly string modVersion = GitVersion.GetAssemblyVersion(typeof(RealTimeMod).Assembly);

        public string Name => "Real Time v" + modVersion;

        // TODO: add localization
        public string Description => "Adjusts the time flow in the game to make it more real";

        public void OnEnabled()
        {
            Log.Info("The 'Real Time' mod has been enabled, version: " + modVersion);
        }

        public void OnDisabled()
        {
            Log.Info("The 'Real Time' mod has been disabled.");
        }

        public void OnSettingsUI(UIHelperBase helper)
        {
            // TODO: imlement the options page
        }
    }
}
