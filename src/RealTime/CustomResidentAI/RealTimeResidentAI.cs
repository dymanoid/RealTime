// <copyright file="RealTimeResidentAI.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.CustomAI
{
    using System;
    using RealTime.Config;
    using RealTime.Events;
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
        /// <param name="config">A <see cref="RealTimeConfig"/> instance containing the mod's configuration.</param>
        /// <param name="connections">A <see cref="GameConnections{T}"/> instance that provides the game connection implementation.</param>
        /// <param name="residentAI">A connection to the game's resident AI.</param>
        /// <param name="eventManager">A <see cref="RealTimeEventManager"/> instance.</param>
        public RealTimeResidentAI(
            RealTimeConfig config,
            GameConnections<TCitizen> connections,
            ResidentAIConnection<TAI, TCitizen> residentAI,
            RealTimeEventManager eventManager)
            : base(config, connections, eventManager)
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
            if (!EnsureCitizenValid(citizenId, ref citizen))
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

            ResidentState residentState = GetResidentState(ref citizen);

            switch (residentState)
            {
                case ResidentState.LeftCity:
                    CitizenManager.ReleaseCitizen(citizenId);
                    break;

                case ResidentState.MovingHome:
                    ProcessCitizenMoving(instance, citizenId, ref citizen, false);
                    break;

                case ResidentState.AtHome:
                    ProcessCitizenAtHome(instance, citizenId, ref citizen);
                    break;

                case ResidentState.MovingToTarget:
                    ProcessCitizenMoving(instance, citizenId, ref citizen, true);
                    break;

                case ResidentState.AtSchoolOrWork:
                    ProcessCitizenAtSchoolOrWork(instance, citizenId, ref citizen);
                    break;

                case ResidentState.AtLunch:
                case ResidentState.Shopping:
                case ResidentState.AtLeisureArea:
                case ResidentState.Visiting:
                    ProcessCitizenVisit(instance, residentState, citizenId, ref citizen);
                    break;

                case ResidentState.Evacuating:
                    ProcessCitizenEvacuation(instance, citizenId, ref citizen);
                    break;
            }
        }
    }
}
