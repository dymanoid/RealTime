// <copyright file="ISimulationManagerConnection.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.GameConnection
{
    using ColossalFramework.Math;

    internal interface ISimulationManagerConnection
    {
        // TODO: implement randomizer abstraction to get rid of refs and ColossalFramework.Math dependency
        ref Randomizer GetRandomizer();
    }
}
