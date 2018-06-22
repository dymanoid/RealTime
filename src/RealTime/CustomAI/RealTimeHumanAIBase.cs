// <copyright file="RealTimeHumanAIBase.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.CustomAI
{
    using System;
    using ColossalFramework.Math;
    using RealTime.Config;
    using RealTime.Events;
    using RealTime.GameConnection;
    using RealTime.Simulation;
    using RealTime.Tools;
    using static Constants;

    internal abstract class RealTimeHumanAIBase<TCitizen>
        where TCitizen : struct
    {
        private Randomizer randomizer;

        protected RealTimeHumanAIBase(RealTimeConfig config, GameConnections<TCitizen> connections, RealTimeEventManager eventManager)
        {
            Config = config ?? throw new ArgumentNullException(nameof(config));
            EventManager = eventManager ?? throw new ArgumentNullException(nameof(eventManager));
            if (connections == null)
            {
                throw new ArgumentNullException(nameof(connections));
            }

            CitizenManager = connections.CitizenManager;
            BuildingManager = connections.BuildingManager;
            CitizenProxy = connections.CitizenConnection;
            TimeInfo = connections.TimeInfo;
            randomizer = connections.SimulationManager.GetRandomizer();
        }

        protected bool IsWeekend => Config.IsWeekendEnabled && TimeInfo.Now.IsWeekend();

        protected bool IsWorkDay => !Config.IsWeekendEnabled || !TimeInfo.Now.IsWeekend();

        protected RealTimeConfig Config { get; }

        protected RealTimeEventManager EventManager { get; }

        protected ICitizenConnection<TCitizen> CitizenProxy { get; }

        protected ICitizenManagerConnection CitizenManager { get; }

        protected IBuildingManagerConnection BuildingManager { get; }

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

        protected bool IsWorkDayMorning(Citizen.AgeGroup citizenAge)
        {
            if (!IsWorkDay)
            {
                return false;
            }

            float workBeginHour;
            switch (citizenAge)
            {
                case Citizen.AgeGroup.Child:
                case Citizen.AgeGroup.Teen:
                    workBeginHour = Config.SchoolBegin;
                    break;

                case Citizen.AgeGroup.Young:
                case Citizen.AgeGroup.Adult:
                    workBeginHour = Config.WorkBegin;
                    break;

                default:
                    return false;
            }

            float currentHour = TimeInfo.CurrentHour;
            return currentHour >= TimeInfo.SunriseHour && currentHour <= workBeginHour;
        }

        protected uint GetGoOutChance(Citizen.AgeGroup citizenAge)
        {
            float currentHour = TimeInfo.CurrentHour;

            uint weekdayModifier;
            if (Config.IsWeekendEnabled)
            {
                weekdayModifier = TimeInfo.Now.IsWeekendTime(GetSpareTimeBeginHour(citizenAge), TimeInfo.SunsetHour)
                    ? 11u
                    : 1u;
            }
            else
            {
                weekdayModifier = 1u;
            }

            bool isDayTime = !TimeInfo.IsNightTime;
            float timeModifier;
            if (isDayTime)
            {
                timeModifier = 5f;
            }
            else
            {
                float nightDuration = TimeInfo.NightDuration;
                float relativeHour = currentHour - TimeInfo.SunsetHour;
                if (relativeHour < 0)
                {
                    relativeHour += 24f;
                }

                timeModifier = 5f / nightDuration * (nightDuration - relativeHour);
            }

            switch (citizenAge)
            {
                case Citizen.AgeGroup.Child when isDayTime:
                case Citizen.AgeGroup.Teen when isDayTime:
                case Citizen.AgeGroup.Young:
                case Citizen.AgeGroup.Adult:
                    return (uint)((timeModifier + weekdayModifier) * timeModifier);

                case Citizen.AgeGroup.Senior when isDayTime:
                    return 80 + weekdayModifier;

                default:
                    return 0;
            }
        }

        protected float GetSpareTimeBeginHour(Citizen.AgeGroup citiztenAge)
        {
            switch (citiztenAge)
            {
                case Citizen.AgeGroup.Child:
                case Citizen.AgeGroup.Teen:
                    return Config.SchoolEnd;

                case Citizen.AgeGroup.Young:
                case Citizen.AgeGroup.Adult:
                    return Config.WorkEnd;

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
                Log.Debug($"{GetCitizenDesc(citizenId, ref citizen)} is collapsed, doing nothing...");
                return false;
            }

            return true;
        }

        protected bool AttendUpcomingEvent(uint citizenId, ref TCitizen citizen, out ushort eventBuildingId)
        {
            eventBuildingId = default;

            ushort currentBuilding = CitizenProxy.GetCurrentBuilding(ref citizen);
            if (EventManager.GetEventState(currentBuilding, DateTime.MaxValue) == EventState.OnGoing)
            {
                return false;
            }

            DateTime earliestStart = TimeInfo.Now.AddHours(MinHoursOnTheWay);
            DateTime latestStart = TimeInfo.Now.AddHours(MaxHoursOnTheWay);

            return EventManager.TryAttendEvent(earliestStart, latestStart, out eventBuildingId);
        }

        protected string GetCitizenDesc(uint citizenId, ref TCitizen citizen)
        {
            string employment = CitizenProxy.GetWorkBuilding(ref citizen) == 0 ? "unempl." : "empl.";
            return $"Citizen {citizenId} ({employment}, {CitizenProxy.GetAge(ref citizen)})";
        }
    }
}
