// <copyright file="LoadingExtension.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.Core
{
    using ICities;

    /// <summary>
    /// Activates this mod for a loaded game level. This is the main entry point of the mod's logic.
    /// </summary>
    public sealed class LoadingExtension : LoadingExtensionBase
    {
        private RealTimeCore core;

        /// <summary>
        /// Calles when a game level is loaded. If applicable, activates the Real Time mod
        /// for the loaded level.
        /// </summary>
        ///
        /// <param name="mode">The <see cref="LoadMode"/> a game level is loaded in.</param>
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

            core = RealTimeCore.Run();
        }

        /// <summary>
        /// Calles when a game level is about to be unloaded. If the Real Time mod was activated
        /// for this level, deactivates the mod for this level.
        /// </summary>
        public override void OnLevelUnloading()
        {
            if (core != null)
            {
                core.Stop();
                core = null;
            }
        }
    }
}