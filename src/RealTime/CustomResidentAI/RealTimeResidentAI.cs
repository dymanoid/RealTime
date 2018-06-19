// <copyright file="RealTimeResidentAI.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.CustomAI
{
    using System;
    using ColossalFramework.Math;
    using RealTime.Config;
    using RealTime.CustomAI;
    using RealTime.GameConnection;
    using RealTime.Simulation;

    internal sealed partial class RealTimeResidentAI<TAI, TCitizen> : RealTimeAIBase
        where TAI : class
        where TCitizen : struct
    {
        private readonly Configuration config;
        private readonly ITimeInfo timeInfo;
        private readonly ResidentAIConnection<TAI, TCitizen> residentAI;
        private readonly ICitizenConnection<TCitizen> citizenProxy;
        private readonly ICitizenManagerConnection citizenManager;
        private readonly IBuildingManagerConnection buildingManager;
        private readonly IEventManagerConnection eventManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="RealTimeResidentAI{TAI, TCitizen}"/> class.
        /// </summary>
        ///
        /// <exception cref="ArgumentNullException">Thrown when any argument is null.</exception>
        ///
        /// <param name="config">A <see cref="Configuration"/> instance containing the mod's configuration.</param>
        /// <param name="connections">A <see cref="GameConnections{T}"/> instance that provides the game connection implementation.</param>
        /// <param name="timeInfo">An object implementing the <see cref="ITimeInfo"/> interface that provides
        /// the current game date and time information.</param>
        /// <param name="randomizer">A <see cref="Randomizer"/> instance to use for randomization.</param>
        public RealTimeResidentAI(Configuration config, GameConnections<TAI, TCitizen> connections, ITimeInfo timeInfo, ref Randomizer randomizer)
            : base(ref randomizer)
        {
            this.config = config ?? throw new ArgumentNullException(nameof(config));
            if (connections == null)
            {
                throw new ArgumentNullException(nameof(connections));
            }

            residentAI = connections.ResidentAI;
            citizenManager = connections.CitizenManager;
            buildingManager = connections.BuildingManager;
            eventManager = connections.EventManager;
            citizenProxy = connections.CitizenConnection;

            this.timeInfo = timeInfo ?? throw new ArgumentNullException(nameof(timeInfo));
        }

        /// <summary>
        /// The main method of the custom AI.
        /// </summary>
        ///
        /// <param name="instance">A reference to an object instance of the original AI.</param>
        /// <param name="citizenId">The ID of the citizen to process.</param>
        /// <param name="citizen">A <see cref="Citizen"/> reference to process.</param>
        public void UpdateLocation(TAI instance, uint citizenId, ref TCitizen citizen)
        {
            if (citizenProxy.GetHomeBuilding(ref citizen) == 0
                && citizenProxy.GetWorkBuilding(ref citizen) == 0
                && citizenProxy.GetVisitBuilding(ref citizen) == 0
                && citizenProxy.GetInstance(ref citizen) == 0
                && citizenProxy.GetVehicle(ref citizen) == 0)
            {
                citizenManager.ReleaseCitizen(citizenId);
                return;
            }

            if (citizenProxy.IsCollapsed(ref citizen))
            {
                return;
            }

            if (citizenProxy.IsDead(ref citizen))
            {
                ProcessCitizenDead(instance, citizenId, ref citizen);
                return;
            }

            if ((citizenProxy.IsSick(ref citizen) && ProcessCitizenSick(instance, citizenId, ref citizen))
                || (citizenProxy.IsArrested(ref citizen) && ProcessCitizenArrested(ref citizen)))
            {
                return;
            }

            CitizenState citizenState = GetCitizenState(ref citizen);

            switch (citizenState)
            {
                case CitizenState.LeftCity:
                    citizenManager.ReleaseCitizen(citizenId);
                    break;

                case CitizenState.MovingHome:
                    ProcessCitizenMoving(instance, citizenId, ref citizen, false);
                    break;

                case CitizenState.AtHome:
                    ProcessCitizenAtHome(instance, citizenId, ref citizen);
                    break;

                case CitizenState.MovingToTarget:
                    ProcessCitizenMoving(instance, citizenId, ref citizen, true);
                    break;

                case CitizenState.AtSchoolOrWork:
                    ProcessCitizenAtSchoolOrWork(instance, citizenId, ref citizen);
                    break;

                case CitizenState.AtLunch:
                case CitizenState.Shopping:
                case CitizenState.AtLeisureArea:
                case CitizenState.Visiting:
                    ProcessCitizenVisit(instance, citizenState, citizenId, ref citizen);
                    break;

                case CitizenState.Evacuating:
                    ProcessCitizenEvacuation(instance, citizenId, ref citizen);
                    break;
            }
        }
    }
}
