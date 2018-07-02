// <copyright file="RealTimeResidentAI.cs" company="dymanoid">Copyright (c) dymanoid. All rights reserved.</copyright>

namespace RealTime.CustomAI
{
    using System;
    using RealTime.Config;
    using RealTime.Events;
    using RealTime.GameConnection;
    using RealTime.Tools;

    /// <summary>A class incorporating the custom logic for a city resident.</summary>
    /// <typeparam name="TAI">The type of the citizen AI.</typeparam>
    /// <typeparam name="TCitizen">The type of the citizen objects.</typeparam>
    /// <seealso cref="RealTimeHumanAIBase{TCitizen}"/>
    internal sealed partial class RealTimeResidentAI<TAI, TCitizen> : RealTimeHumanAIBase<TCitizen>
        where TAI : class
        where TCitizen : struct
    {
        private readonly ResidentAIConnection<TAI, TCitizen> residentAI;

        /// <summary>Initializes a new instance of the <see cref="RealTimeResidentAI{TAI, TCitizen}"/> class.</summary>
        /// <exception cref="ArgumentNullException">Thrown when any argument is null.</exception>
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

        /// <summary>The entry method of the custom AI.</summary>
        /// <param name="instance">A reference to an object instance of the original AI.</param>
        /// <param name="citizenId">The ID of the citizen to process.</param>
        /// <param name="citizen">A <typeparamref name="TCitizen"/> reference to process.</param>
        public void UpdateLocation(TAI instance, uint citizenId, ref TCitizen citizen)
        {
            if (!EnsureCitizenCanBeProcessed(citizenId, ref citizen))
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
            bool isVirtual;

            switch (residentState)
            {
                case ResidentState.MovingHome:
                    ProcessCitizenMoving(instance, citizenId, ref citizen, false);
                    break;

                case ResidentState.AtHome:
                    isVirtual = IsCitizenVirtual(instance, ref citizen, ShouldRealizeCitizen);
                    ProcessCitizenAtHome(instance, citizenId, ref citizen, isVirtual);
                    break;

                case ResidentState.MovingToTarget:
                    ProcessCitizenMoving(instance, citizenId, ref citizen, true);
                    break;

                case ResidentState.AtSchoolOrWork:
                    isVirtual = IsCitizenVirtual(instance, ref citizen, ShouldRealizeCitizen);
                    ProcessCitizenAtSchoolOrWork(instance, citizenId, ref citizen, isVirtual);
                    break;

                case ResidentState.AtLunch:
                case ResidentState.Shopping:
                case ResidentState.AtLeisureArea:
                case ResidentState.Visiting:
                    isVirtual = IsCitizenVirtual(instance, ref citizen, ShouldRealizeCitizen);
                    ProcessCitizenVisit(instance, residentState, citizenId, ref citizen, isVirtual);
                    break;

                case ResidentState.OnTour:
                    ProcessCitizenOnTour(instance, citizenId, ref citizen);
                    break;

                case ResidentState.Evacuating:
                    ProcessCitizenEvacuation(instance, citizenId, ref citizen);
                    break;

                case ResidentState.InShelter:
                    isVirtual = IsCitizenVirtual(instance, ref citizen, ShouldRealizeCitizen);
                    CitizenReturnsFromShelter(instance, citizenId, ref citizen, isVirtual);
                    break;

                case ResidentState.Unknown:
                    Log.Debug(TimeInfo.Now, $"WARNING: {GetCitizenDesc(citizenId, ref citizen, null)} is in an UNKNOWN state! Teleporting back home");
                    if (CitizenProxy.GetHomeBuilding(ref citizen) != 0)
                    {
                        CitizenProxy.SetLocation(ref citizen, Citizen.Location.Home);
                    }

                    break;
            }
        }

        private bool ShouldRealizeCitizen(TAI ai)
        {
            return residentAI.DoRandomMove(ai);
        }
    }
}