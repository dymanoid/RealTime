// <copyright file="RealTimeResidentAI.cs" company="dymanoid">Copyright (c) dymanoid. All rights reserved.</copyright>

namespace RealTime.CustomAI
{
    using System;
    using System.IO;
    using RealTime.Config;
    using RealTime.Core;
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
        private readonly WorkBehavior workBehavior;
        private readonly SpareTimeBehavior spareTimeBehavior;
        private readonly TravelBehavior travelBehavior;
        private readonly CitizenSchedule[] residentSchedules;
        private float simulationCycle;

        /// <summary>Initializes a new instance of the <see cref="RealTimeResidentAI{TAI, TCitizen}"/> class.</summary>
        /// <exception cref="ArgumentNullException">Thrown when any argument is null.</exception>
        /// <param name="config">A <see cref="RealTimeConfig"/> instance containing the mod's configuration.</param>
        /// <param name="connections">A <see cref="GameConnections{T}"/> instance that provides the game connection implementation.</param>
        /// <param name="residentAI">A connection to the game's resident AI.</param>
        /// <param name="eventManager">A <see cref="RealTimeEventManager"/> instance.</param>
        /// <param name="workBehavior">A behavior that provides simulation info for the citizens work time.</param>
        /// <param name="spareTimeBehavior">A behavior that provides simulation info for the citizens spare time.</param>
        /// <param name="travelBehavior">A behavior that provides simulation info for the citizens traveling.</param>
        public RealTimeResidentAI(
            RealTimeConfig config,
            GameConnections<TCitizen> connections,
            ResidentAIConnection<TAI, TCitizen> residentAI,
            RealTimeEventManager eventManager,
            WorkBehavior workBehavior,
            SpareTimeBehavior spareTimeBehavior,
            TravelBehavior travelBehavior)
            : base(config, connections, eventManager)
        {
            this.residentAI = residentAI ?? throw new ArgumentNullException(nameof(residentAI));
            this.workBehavior = workBehavior ?? throw new ArgumentNullException(nameof(workBehavior));
            this.spareTimeBehavior = spareTimeBehavior ?? throw new ArgumentNullException(nameof(spareTimeBehavior));
            this.travelBehavior = travelBehavior ?? throw new ArgumentNullException(nameof(travelBehavior));

            residentSchedules = new CitizenSchedule[CitizenMgr.GetMaxCitizensCount()];
        }

        /// <summary>The entry method of the custom AI.</summary>
        /// <param name="instance">A reference to an object instance of the original AI.</param>
        /// <param name="citizenId">The ID of the citizen to process.</param>
        /// <param name="citizen">A <typeparamref name="TCitizen"/> reference to process.</param>
        public void UpdateLocation(TAI instance, uint citizenId, ref TCitizen citizen)
        {
            if (!EnsureCitizenCanBeProcessed(citizenId, ref citizen))
            {
                residentSchedules[citizenId] = default;
                return;
            }

            ref CitizenSchedule schedule = ref residentSchedules[citizenId];
            if (CitizenProxy.IsDead(ref citizen))
            {
                ProcessCitizenDead(instance, citizenId, ref citizen);
                schedule.Schedule(ResidentState.Unknown, default);
                return;
            }

            if ((CitizenProxy.IsSick(ref citizen) && ProcessCitizenSick(instance, citizenId, ref citizen))
                || (CitizenProxy.IsArrested(ref citizen) && ProcessCitizenArrested(ref citizen)))
            {
                schedule.Schedule(ResidentState.Unknown, default);
                return;
            }

            ScheduleAction actionType = UpdateCitizenState(citizenId, ref citizen, ref schedule);
            switch (actionType)
            {
                case ScheduleAction.Ignore:
                    return;

                case ScheduleAction.ProcessTransition when ProcessCitizenMoving(ref schedule, instance, citizenId, ref citizen):
                    return;
            }

            if (schedule.CurrentState == ResidentState.Unknown)
            {
                Log.Debug(TimeInfo.Now, $"WARNING: {GetCitizenDesc(citizenId, ref citizen)} is in an UNKNOWN state! Changing to 'moving'");
                CitizenProxy.SetLocation(ref citizen, Citizen.Location.Moving);
                return;
            }

            if (TimeInfo.Now < schedule.ScheduledStateTime)
            {
                return;
            }

            UpdateCitizenSchedule(ref schedule, citizenId, ref citizen);
            ExecuteCitizenSchedule(ref schedule, instance, citizenId, ref citizen);
        }

        /// <summary>Notifies that a citizen has arrived their destination.</summary>
        /// <param name="citizenId">The citizen ID to process.</param>
        public void RegisterCitizenArrival(uint citizenId)
        {
            if (citizenId == 0 || citizenId >= residentSchedules.Length)
            {
                return;
            }

            ref CitizenSchedule schedule = ref residentSchedules[citizenId];
            switch (CitizenMgr.GetCitizenLocation(citizenId))
            {
                case Citizen.Location.Work:
                    schedule.UpdateTravelTimeToWork(TimeInfo.Now);
                    Log.Debug($"The citizen {citizenId} arrived at work at {TimeInfo.Now} and needs {schedule.TravelTimeToWork} hours to get to work");
                    break;

                case Citizen.Location.Moving:
                    return;
            }

            schedule.DepartureToWorkTime = default;
        }

        /// <summary>Performs simulation for starting a new day for all citizens.</summary>
        public void BeginNewDay()
        {
            workBehavior.UpdateLunchTime();
            todayWakeup = TimeInfo.Now.Date.AddHours(Config.WakeupHour);
        }

        /// <summary>Performs simulation for starting a new day for a citizen with specified ID.</summary>
        /// <param name="citizenId">The citizen ID to process.</param>
        public void BeginNewDayForCitizen(uint citizenId)
        {
            // TODO: use this method
            if (citizenId == 0)
            {
                return;
            }
        }

        /// <summary>Sets the duration (in hours) of a full simulation cycle for all citizens.
        /// The game calls the simulation methods for a particular citizen with this period.</summary>
        /// <param name="cyclePeriod">The citizens simulation cycle period, in game hours.</param>
        public void SetSimulationCyclePeriod(float cyclePeriod)
        {
            simulationCycle = cyclePeriod;
            Log.Debug($"SIMULATION CYCLE PERIOD: {cyclePeriod} hours");
        }

        /// <summary>Gets an instance of the storage service that can read and write the custom schedule data.</summary>
        /// <returns>An object that implements the <see cref="IStorageData"/> interface.</returns>
        public IStorageData GetStorageService()
        {
            return new CitizenScheduleStorage(residentSchedules, CitizenMgr.GetCitizensArray(), TimeInfo);
        }
    }
}