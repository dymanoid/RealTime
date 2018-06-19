// <copyright file="RealTimeResidentAI.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.CustomAI
{
    using System;
    using RealTime.Config;
    using RealTime.GameConnection;

    internal sealed partial class RealTimeResidentAI<TAI, TCitizen> : RealTimeHumanAIBase<TCitizen>
        where TAI : class
        where TCitizen : struct
    {
        private readonly ResidentAIConnection<TAI, TCitizen> residentAI;

        /// <summary>
        /// Initializes a new instance of the <see cref="RealTimeResidentAI{TAI, TCitizen}"/> class.
        /// </summary>
        ///
        /// <exception cref="ArgumentNullException">Thrown when any argument is null.</exception>
        ///
        /// <param name="config">A <see cref="Configuration"/> instance containing the mod's configuration.</param>
        /// <param name="connections">A <see cref="GameConnections{T}"/> instance that provides the game connection implementation.</param>
        /// <param name="residentAI">A connection to the game's resident AI.</param>
        public RealTimeResidentAI(Configuration config, GameConnections<TCitizen> connections, ResidentAIConnection<TAI, TCitizen> residentAI)
            : base(config, connections)
        {
            this.residentAI = residentAI ?? throw new ArgumentNullException(nameof(residentAI));
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
            if (CitizenProxy.GetHomeBuilding(ref citizen) == 0
                && CitizenProxy.GetWorkBuilding(ref citizen) == 0
                && CitizenProxy.GetVisitBuilding(ref citizen) == 0
                && CitizenProxy.GetInstance(ref citizen) == 0
                && CitizenProxy.GetVehicle(ref citizen) == 0)
            {
                CitizenManager.ReleaseCitizen(citizenId);
                return;
            }

            if (CitizenProxy.IsCollapsed(ref citizen))
            {
                return;
            }

            if (CitizenProxy.IsDead(ref citizen))
            {
                ProcessCitizenDead(instance, citizenId, ref citizen);
                return;
            }

            if ((CitizenProxy.IsSick(ref citizen) && ProcessCitizenSick(instance, citizenId, ref citizen))
                || (CitizenProxy.IsArrested(ref citizen) && ProcessCitizenArrested(ref citizen)))
            {
                return;
            }

            CitizenState citizenState = GetCitizenState(ref citizen);

            switch (citizenState)
            {
                case CitizenState.LeftCity:
                    CitizenManager.ReleaseCitizen(citizenId);
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
