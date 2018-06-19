// <copyright file="RealTimeHumanAIBase.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.CustomAI
{
    using System;
    using ColossalFramework.Math;
    using RealTime.Config;
    using RealTime.GameConnection;
    using RealTime.Simulation;

    internal abstract class RealTimeHumanAIBase<TCitizen>
        where TCitizen : struct
    {
        private Randomizer randomizer;

        protected RealTimeHumanAIBase(Configuration config, GameConnections<TCitizen> connections)
        {
            Config = config ?? throw new ArgumentNullException(nameof(config));

            if (connections == null)
            {
                throw new ArgumentNullException(nameof(connections));
            }

            CitizenManager = connections.CitizenManager;
            BuildingManager = connections.BuildingManager;
            EventManager = connections.EventManager;
            CitizenProxy = connections.CitizenConnection;
            TimeInfo = connections.TimeInfo;
            randomizer = connections.SimulationManager.GetRandomizer();
        }

        protected Configuration Config { get; }

        protected ICitizenConnection<TCitizen> CitizenProxy { get; }

        protected ICitizenManagerConnection CitizenManager { get; }

        protected IBuildingManagerConnection BuildingManager { get; }

        protected IEventManagerConnection EventManager { get; }

        protected ITimeInfo TimeInfo { get; }

        protected ref Randomizer Randomizer => ref randomizer;

        protected bool IsChance(uint chance)
        {
            return Randomizer.UInt32(100u) < chance;
        }
    }
}
