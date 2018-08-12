// <copyright file="RealTimeResidentAI.cs" company="dymanoid">Copyright (c) dymanoid. All rights reserved.</copyright>

namespace RealTime.CustomAI
{
    using System;
    using RealTime.Config;
    using RealTime.Events;
    using RealTime.GameConnection;
    using SkyTools.Storage;
    using SkyTools.Tools;

    /// <summary>A class incorporating the custom logic for a city resident.</summary>
    /// <typeparam name="TAI">The type of the citizen AI.</typeparam>
    /// <typeparam name="TCitizen">The type of the citizen objects.</typeparam>
    /// <seealso cref="RealTimeHumanAIBase{TCitizen}"/>
    internal sealed partial class RealTimeResidentAI<TAI, TCitizen> : RealTimeHumanAIBase<TCitizen>
        where TAI : class
        where TCitizen : struct
    {
        private readonly ResidentAIConnection<TAI, TCitizen> residentAI;
        private readonly IRealTimeBuildingAI buildingAI;
        private readonly IWorkBehavior workBehavior;
        private readonly ISpareTimeBehavior spareTimeBehavior;
        private readonly ITravelBehavior travelBehavior;

        private readonly CitizenSchedule[] residentSchedules;

        private float simulationCycle;

        /// <summary>Initializes a new instance of the <see cref="RealTimeResidentAI{TAI, TCitizen}"/> class.</summary>
        /// <exception cref="ArgumentNullException">Thrown when any argument is null.</exception>
        /// <param name="config">A <see cref="RealTimeConfig"/> instance containing the mod's configuration.</param>
        /// <param name="connections">A <see cref="GameConnections{T}"/> instance that provides the game connection implementation.</param>
        /// <param name="residentAI">A connection to the game's resident AI.</param>
        /// <param name="eventManager">An <see cref="IRealTimeEventManager"/> instance.</param>
        /// <param name="buildingAI">The custom building AI.</param>
        /// <param name="workBehavior">A behavior that provides simulation info for the citizens work time.</param>
        /// <param name="spareTimeBehavior">A behavior that provides simulation info for the citizens spare time.</param>
        /// <param name="travelBehavior">A behavior that provides simulation info for the citizens traveling.</param>
        public RealTimeResidentAI(
            RealTimeConfig config,
            GameConnections<TCitizen> connections,
            ResidentAIConnection<TAI, TCitizen> residentAI,
            IRealTimeEventManager eventManager,
            IRealTimeBuildingAI buildingAI,
            IWorkBehavior workBehavior,
            ISpareTimeBehavior spareTimeBehavior,
            ITravelBehavior travelBehavior)
            : base(config, connections, eventManager)
        {
            this.residentAI = residentAI ?? throw new ArgumentNullException(nameof(residentAI));
            this.buildingAI = buildingAI ?? throw new ArgumentNullException(nameof(buildingAI));
            this.workBehavior = workBehavior ?? throw new ArgumentNullException(nameof(workBehavior));
            this.spareTimeBehavior = spareTimeBehavior ?? throw new ArgumentNullException(nameof(spareTimeBehavior));
            this.travelBehavior = travelBehavior ?? throw new ArgumentNullException(nameof(travelBehavior));

            residentSchedules = new CitizenSchedule[CitizenMgr.GetMaxCitizensCount()];
        }

        /// <summary>Gets a value indicating whether the citizens can grow up in the current game time.</summary>
        public bool CanCitizensGrowUp { get; private set; }

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
                schedule.Schedule(ResidentState.Unknown);
                return;
            }

            if ((CitizenProxy.IsSick(ref citizen) && ProcessCitizenSick(instance, citizenId, ref citizen))
                || (CitizenProxy.IsArrested(ref citizen) && ProcessCitizenArrested(ref citizen)))
            {
                schedule.Schedule(ResidentState.Unknown);
                return;
            }

            ScheduleAction actionType = UpdateCitizenState(ref citizen, ref schedule);
            switch (actionType)
            {
                case ScheduleAction.Ignore:
                    return;

                case ScheduleAction.ProcessTransition when ProcessCitizenMoving(ref schedule, instance, citizenId, ref citizen):
                    return;
            }

            if (schedule.CurrentState == ResidentState.Unknown)
            {
                Log.Debug(LogCategory.State, TimeInfo.Now, $"WARNING: {GetCitizenDesc(citizenId, ref citizen)} is in an UNKNOWN state! Changing to 'moving'");
                CitizenProxy.SetLocation(ref citizen, Citizen.Location.Moving);
                return;
            }

            if (TimeInfo.Now < schedule.ScheduledStateTime)
            {
                return;
            }

            bool updated = UpdateCitizenSchedule(ref schedule, citizenId, ref citizen);
            ExecuteCitizenSchedule(ref schedule, instance, citizenId, ref citizen, updated);
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
                    Log.Debug(LogCategory.Movement, $"The citizen {citizenId} arrived at work at {TimeInfo.Now} and needs {schedule.TravelTimeToWork} hours to get to work");
                    break;

                case Citizen.Location.Moving:
                    return;
            }

            schedule.DepartureToWorkTime = default;
        }

        /// <summary>Performs simulation for starting a day cycle beginning with specified hour.
        /// Enables the logic to perform the 'new cycle processing' for the citizens.</summary>
        /// <param name="hour">The hour of the cycle.</param>
        public void BeginNewHourCycleProcessing(int hour)
        {
            Log.Debug(LogCategory.Generic, TimeInfo.Now, "Starting of the 'new cycle' processing for each citizen...");
            if (hour == 0)
            {
                workBehavior.BeginNewDay();
                todayWakeUp = TimeInfo.Now.Date.AddHours(Config.WakeUpHour);
            }

            CanCitizensGrowUp = true;
        }

        /// <summary>Disables the 'new cycle processing' for the citizens.</summary>
        public void EndHourCycleProcessing()
        {
            if (Config.UseSlowAging)
            {
                CanCitizensGrowUp = false;
            }

            Log.Debug(LogCategory.Generic, TimeInfo.Now, "The 'new cycle' processing for the citizens is now completed.");
        }

        /// <summary>Performs simulation for starting a new day for a citizen with specified ID.</summary>
        /// <param name="citizenId">The citizen ID to process.</param>
        public void BeginNewDayForCitizen(uint citizenId)
        {
            if (citizenId != 0)
            {
                ProcessVacation(citizenId);
            }
        }

        /// <summary>
        /// Determines whether the specified <paramref name="citizen"/> can give life to a new citizen.
        /// </summary>
        /// <param name="citizenId">The ID of the citizen to check.</param>
        /// <param name="citizen">The citizen to check.</param>
        /// <returns>
        ///   <c>true</c> if the specified <paramref name="citizen"/> can make babies; otherwise, <c>false</c>.
        /// </returns>
        public bool CanMakeBabies(uint citizenId, ref TCitizen citizen)
        {
            uint idFlag = citizenId % 3;
            uint timeFlag = (uint)TimeInfo.CurrentHour % 3;
            if (!Config.UseSlowAging)
            {
                idFlag = 0;
                timeFlag = 0;
            }

            if (timeFlag != idFlag || CitizenProxy.IsDead(ref citizen) || CitizenProxy.HasFlags(ref citizen, Citizen.Flags.MovingIn))
            {
                return false;
            }

            switch (CitizenProxy.GetAge(ref citizen))
            {
                case Citizen.AgeGroup.Young:
                    return CitizenProxy.GetGender(citizenId) == Citizen.Gender.Male
                        || Random.ShouldOccur(Constants.YoungFemalePregnancyChance);

                case Citizen.AgeGroup.Adult:
                    return true;

                default:
                    return false;
            }
        }

        /// <summary>Sets the duration (in hours) of a full simulation cycle for all citizens.
        /// The game calls the simulation methods for a particular citizen with this period.</summary>
        /// <param name="cyclePeriod">The citizens simulation cycle period, in game hours.</param>
        public void SetSimulationCyclePeriod(float cyclePeriod)
        {
            simulationCycle = cyclePeriod;
            Log.Debug(LogCategory.Simulation, $"SIMULATION CYCLE PERIOD: {cyclePeriod} hours");
        }

        /// <summary>Gets an instance of the storage service that can read and write the custom schedule data.</summary>
        /// <returns>An object that implements the <see cref="IStorageData"/> interface.</returns>
        public IStorageData GetStorageService()
        {
            return new CitizenScheduleStorage(residentSchedules, CitizenMgr.GetCitizensArray(), TimeInfo);
        }

        /// <summary>Gets the citizen schedule. Note that the method returns the reference
        /// and thus doesn't prevent changing the schedule.</summary>
        /// <param name="citizenId">The ID of the citizen to get the schedule for.</param>
        /// <returns>The original schedule of the citizen.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="citizenId"/> is 0.</exception>
        public ref CitizenSchedule GetCitizenSchedule(uint citizenId)
        {
            if (citizenId == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(citizenId), citizenId, "The citizen ID cannot be 0");
            }

            return ref residentSchedules[citizenId];
        }
    }
}