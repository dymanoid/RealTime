// <copyright file="ISimulationManagerConnection.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.GameConnection
{
    using ColossalFramework.Math;

    internal interface ISimulationManagerConnection
    {
        ref Randomizer GetRandomizer();
    }
}
