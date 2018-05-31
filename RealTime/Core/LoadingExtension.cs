// <copyright file="LoadingExtension.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.Core
{
    using ICities;

    public sealed class LoadingExtension : LoadingExtensionBase
    {
        private RealTimeCore core;

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

            core = RealTimeCore.Enable();
        }

        public override void OnLevelUnloading()
        {
            if (core != null)
            {
                core.Disable();
                core = null;
            }
        }
    }
}