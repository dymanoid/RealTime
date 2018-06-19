// <copyright file="SimulationManagerConnection.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.GameConnection
{
    using ColossalFramework.Math;

    internal class SimulationManagerConnection : ISimulationManagerConnection
    {
        public ref Randomizer GetRandomizer()
        {
            return ref SimulationManager.instance.m_randomizer;
        }
    }
}
