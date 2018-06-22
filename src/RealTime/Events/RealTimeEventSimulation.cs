// <copyright file="RealTimeEventSimulation.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.Events
{
    using ICities;

    public sealed class RealTimeEventSimulation : ThreadingExtensionBase
    {
        internal static RealTimeEventManager EventManager { get; set; }

        public override void OnBeforeSimulationTick()
        {
            EventManager?.ProcessEvents();
        }
    }
}
