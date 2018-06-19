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
    using RealTime.Tools;
    using UnityEngine;
    using static Constants;

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

        protected bool IsWeekend => Config.IsWeekendEnabled && TimeInfo.Now.IsWeekend();

        protected bool IsWorkDay => !Config.IsWeekendEnabled || !TimeInfo.Now.IsWeekend();

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

        protected bool IsWorkDayAndBetweenHours(float fromInclusive, float toExclusive)
        {
            float currentHour = TimeInfo.CurrentHour;
            return IsWorkDay && (currentHour >= fromInclusive && currentHour < toExclusive);
        }

        protected bool IsWorkDayMorning()
        {
            if (!IsWorkDay)
            {
                return false;
            }

            float currentHour = TimeInfo.CurrentHour;
            return currentHour >= TimeInfo.SunriseHour && currentHour < Math.Min(Config.WorkBegin, Config.SchoolBegin);
        }

        protected uint GetGoOutChance(Citizen.AgeGroup citizenAge, bool isDayTime)
        {
            float currentHour = TimeInfo.CurrentHour;
            uint multiplier;

            if ((IsWeekend && TimeInfo.Now.IsWeekendAfter(AssumedGoOutDuration)) || TimeInfo.Now.DayOfWeek == DayOfWeek.Friday)
            {
                multiplier = isDayTime
                    ? 5u
                    : (uint)Mathf.Clamp(Mathf.Abs(TimeInfo.SunriseHour - currentHour), 0f, 5f);
            }
            else
            {
                multiplier = 1u;
            }

            switch (citizenAge)
            {
                case Citizen.AgeGroup.Child when isDayTime:
                    return 60 + (4 * multiplier);

                case Citizen.AgeGroup.Teen when isDayTime:
                case Citizen.AgeGroup.Young:
                    return 50 + (8 * multiplier);

                case Citizen.AgeGroup.Adult:
                    return 30 + (6 * multiplier);

                case Citizen.AgeGroup.Senior when isDayTime:
                    return 90;

                default:
                    return 0;
            }
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
