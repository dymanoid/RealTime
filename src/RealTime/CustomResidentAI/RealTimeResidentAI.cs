// <copyright file="RealTimeResidentAI.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.CustomResidentAI
{
    using System;
    using ColossalFramework.Math;
    using RealTime.Config;
    using RealTime.CustomAI;
    using RealTime.GameConnection;
    using RealTime.Simulation;

    internal sealed partial class RealTimeResidentAI<T> : RealTimeAIBase
        where T : class
    {
        private readonly Configuration config;
        private readonly ITimeInfo timeInfo;
        private readonly ResidentAIConnection<T> residentAI;
        private readonly ICitizenManagerConnection citizenManager;
        private readonly IBuildingManagerConnection buildingManager;
        private readonly IEventManagerConnection eventManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="RealTimeResidentAI{T}"/> class.
        /// </summary>
        ///
        /// <exception cref="ArgumentNullException">Thrown when any argument is null.</exception>
        ///
        /// <param name="config">A <see cref="Configuration"/> instance containing the mod's configuration.</param>
        /// <param name="connections">A <see cref="GameConnections{T}"/> instance that provides the game connection implementation.</param>
        /// <param name="timeInfo">An object implementing the <see cref="ITimeInfo"/> interface that provides
        /// the current game date and time information.</param>
        /// <param name="randomizer">A <see cref="Randomizer"/> instance to use for randomization.</param>
        public RealTimeResidentAI(Configuration config, GameConnections<T> connections, ITimeInfo timeInfo, ref Randomizer randomizer)
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

            this.timeInfo = timeInfo ?? throw new ArgumentNullException(nameof(timeInfo));
        }

        /// <summary>
        /// The main method of the custom AI.
        /// </summary>
        ///
        /// <param name="instance">A reference to an object instance of the original AI.</param>
        /// <param name="citizenId">The ID of the citizen to process.</param>
        /// <param name="citizen">A <see cref="Citizen"/> reference to process.</param>
        public void UpdateLocation(T instance, uint citizenId, ref Citizen citizen)
        {
            if (citizen.m_homeBuilding == 0 && citizen.m_workBuilding == 0 && citizen.m_visitBuilding == 0
                && citizen.m_instance == 0 && citizen.m_vehicle == 0)
            {
                citizenManager.ReleaseCitizen(citizenId);
                return;
            }

            if (citizen.Collapsed)
            {
                return;
            }

            if (citizen.Dead)
            {
                ProcessCitizenDead(instance, citizenId, ref citizen);
                return;
            }

            if ((citizen.Sick && ProcessCitizenSick(instance, citizenId, ref citizen))
                || (citizen.Arrested && ProcessCitizenArrested(ref citizen)))
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
