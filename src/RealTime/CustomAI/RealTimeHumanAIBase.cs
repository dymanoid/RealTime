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
        protected const int ShoppingGoodsAmount = 100;

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

        protected bool EnsureCitizenValid(uint citizenId, ref TCitizen citizen)
        {
            if (CitizenProxy.GetHomeBuilding(ref citizen) == 0
                && CitizenProxy.GetWorkBuilding(ref citizen) == 0
                && CitizenProxy.GetVisitBuilding(ref citizen) == 0
                && CitizenProxy.GetInstance(ref citizen) == 0
                && CitizenProxy.GetVehicle(ref citizen) == 0)
            {
                CitizenManager.ReleaseCitizen(citizenId);
                return false;
            }

            if (CitizenProxy.IsCollapsed(ref citizen))
            {
                return false;
            }

            return true;
        }
    }
}
