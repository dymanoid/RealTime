// <copyright file="WorkBehavior.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.CustomAI
{
    using System;
    using RealTime.Config;
    using RealTime.GameConnection;
    using RealTime.Simulation;
    using SkyTools.Tools;
    using static Constants;

    /// <summary>
    /// A class containing methods for managing the citizens' work behavior.
    /// </summary>
    internal sealed class WorkBehavior : IWorkBehavior
    {
        private readonly RealTimeConfig config;
        private readonly IRandomizer randomizer;
        private readonly IBuildingManagerConnection buildingManager;
        private readonly ITimeInfo timeInfo;
        private readonly ITravelBehavior travelBehavior;

        private DateTime lunchBegin;
        private DateTime lunchEnd;

        /// <summary>Initializes a new instance of the <see cref="WorkBehavior"/> class.</summary>
        /// <param name="config">The configuration to run with.</param>
        /// <param name="randomizer">The randomizer implementation.</param>
        /// <param name="buildingManager">The building manager implementation.</param>
        /// <param name="timeInfo">The time information source.</param>
        /// <param name="travelBehavior">A behavior that provides simulation info for the citizens traveling.</param>
        /// <exception cref="ArgumentNullException">Thrown when any argument is null.</exception>
        public WorkBehavior(
            RealTimeConfig config,
            IRandomizer randomizer,
            IBuildingManagerConnection buildingManager,
            ITimeInfo timeInfo,
            ITravelBehavior travelBehavior)
        {
            this.config = config ?? throw new ArgumentNullException(nameof(config));
            this.randomizer = randomizer ?? throw new ArgumentNullException(nameof(randomizer));
            this.buildingManager = buildingManager ?? throw new ArgumentNullException(nameof(buildingManager));
            this.timeInfo = timeInfo ?? throw new ArgumentNullException(nameof(timeInfo));
            this.travelBehavior = travelBehavior ?? throw new ArgumentNullException(nameof(travelBehavior));
        }

        /// <summary>
        /// Determines whether a building of specified <paramref name="service"/> and <paramref name="subService"/>
        /// currently has working hours. Note that this method always returns <c>true</c> for residential buildings.
        /// </summary>
        /// <param name="service">The building service.</param>
        /// <param name="subService">The building sub-service.</param>
        /// <returns>
        ///   <c>true</c> if a building of specified <paramref name="service"/> and <paramref name="subService"/>
        /// currently has working hours; otherwise, <c>false</c>.
        /// </returns>
        public bool IsBuildingWorking(ItemClass.Service service, ItemClass.SubService subService)
        {
            switch (subService)
            {
                case ItemClass.SubService.CommercialLow:
                case ItemClass.SubService.IndustrialOil:
                case ItemClass.SubService.IndustrialOre:
                    return true;
            }

            switch (service)
            {
                case ItemClass.Service.Residential:
                case ItemClass.Service.Tourism:
                case ItemClass.Service.PoliceDepartment:
                case ItemClass.Service.FireDepartment:
                case ItemClass.Service.PublicTransport:
                case ItemClass.Service.Disaster:
                case ItemClass.Service.Electricity:
                case ItemClass.Service.Water:
                case ItemClass.Service.HealthCare:
                case ItemClass.Service.Garbage:
                case ItemClass.Service.Road:
                    return true;
            }

            if (config.IsWeekendEnabled && timeInfo.Now.IsWeekend() && !IsBuildingActiveOnWeekend(service, subService))
            {
                return false;
            }

            return IsBuildingWorking(
                GetBuildingWorkShiftCount(service, subService),
                HasExtendedFirstWorkShift(service, subService));
        }

        /// <summary>Notifies this object that a new game day starts.</summary>
        public void BeginNewDay()
        {
            DateTime today = timeInfo.Now.Date;
            lunchBegin = today.AddHours(config.LunchBegin);
            lunchEnd = today.AddHours(config.LunchEnd);
        }

        /// <summary>Updates the citizen's work shift parameters in the specified citizen's <paramref name="schedule"/>.</summary>
        /// <param name="schedule">The citizen's schedule to update the work shift in.</param>
        /// <param name="citizenAge">The age of the citizen.</param>
        public void UpdateWorkShift(ref CitizenSchedule schedule, Citizen.AgeGroup citizenAge)
        {
            if (schedule.WorkBuilding == 0 || citizenAge == Citizen.AgeGroup.Senior)
            {
                schedule.UpdateWorkShift(WorkShift.Unemployed, 0, 0, false);
                return;
            }

            ItemClass.Service service = buildingManager.GetBuildingService(schedule.WorkBuilding);
            ItemClass.SubService subService = buildingManager.GetBuildingSubService(schedule.WorkBuilding);

            float workBegin, workEnd;
            WorkShift workShift = schedule.WorkShift;

            switch (citizenAge)
            {
                case Citizen.AgeGroup.Child:
                case Citizen.AgeGroup.Teen:
                    workShift = WorkShift.First;
                    workBegin = config.SchoolBegin;
                    workEnd = config.SchoolEnd;
                    break;

                case Citizen.AgeGroup.Young:
                case Citizen.AgeGroup.Adult:
                    if (workShift == WorkShift.Unemployed)
                    {
                        workShift = GetWorkShift(GetBuildingWorkShiftCount(service, subService));
                    }

                    workBegin = config.WorkBegin;
                    workEnd = config.WorkEnd;
                    break;

                default:
                    return;
            }

            switch (workShift)
            {
                case WorkShift.First when HasExtendedFirstWorkShift(service, subService):
                    workBegin = Math.Min(config.WakeUpHour, EarliestWakeUp);
                    break;

                case WorkShift.Second:
                    workBegin = workEnd;
                    workEnd = 0;
                    break;

                case WorkShift.Night:
                    workEnd = workBegin;
                    workBegin = 0;
                    break;
            }

            schedule.UpdateWorkShift(workShift, workBegin, workEnd, IsBuildingActiveOnWeekend(service, subService));
        }

        /// <summary>Updates the citizen's work schedule by determining the time for going to work.</summary>
        /// <param name="schedule">The citizen's schedule to update.</param>
        /// <param name="currentBuilding">The ID of the building where the citizen is currently located.</param>
        /// <param name="simulationCycle">The duration (in hours) of a full citizens simulation cycle.</param>
        /// <returns><c>true</c> if work was scheduled; otherwise, <c>false</c>.</returns>
        public bool ScheduleGoToWork(ref CitizenSchedule schedule, ushort currentBuilding, float simulationCycle)
        {
            if (schedule.CurrentState == ResidentState.AtSchoolOrWork)
            {
                return false;
            }

            DateTime now = timeInfo.Now;
            if (config.IsWeekendEnabled && now.IsWeekend() && !schedule.WorksOnWeekends)
            {
                return false;
            }

            float travelTime = GetTravelTimeToWork(ref schedule, currentBuilding);

            DateTime workEndTime = now.FutureHour(schedule.WorkShiftEndHour);
            DateTime departureTime = now.FutureHour(schedule.WorkShiftStartHour - travelTime - simulationCycle);
            if (departureTime > workEndTime && now.AddHours(travelTime + simulationCycle) < workEndTime)
            {
                departureTime = now;
            }

            schedule.Schedule(ResidentState.AtSchoolOrWork, departureTime);
            return true;
        }

        /// <summary>Updates the citizen's work schedule by determining the lunch time.</summary>
        /// <param name="schedule">The citizen's schedule to update.</param>
        /// <param name="citizenAge">The citizen's age.</param>
        /// <returns><c>true</c> if a lunch time was scheduled; otherwise, <c>false</c>.</returns>
        public bool ScheduleLunch(ref CitizenSchedule schedule, Citizen.AgeGroup citizenAge)
        {
            if (timeInfo.Now < lunchBegin
                && schedule.WorkStatus == WorkStatus.Working
                && schedule.WorkShift == WorkShift.First
                && WillGoToLunch(citizenAge))
            {
                schedule.Schedule(ResidentState.Shopping, lunchBegin);
                return true;
            }

            return false;
        }

        /// <summary>Updates the citizen's work schedule by determining the returning from lunch time.</summary>
        /// <param name="schedule">The citizen's schedule to update.</param>
        public void ScheduleReturnFromLunch(ref CitizenSchedule schedule)
        {
            if (schedule.WorkStatus == WorkStatus.Working)
            {
                schedule.Schedule(ResidentState.AtSchoolOrWork, lunchEnd);
            }
        }

        /// <summary>Updates the citizen's work schedule by determining the time for returning from work.</summary>
        /// <param name="schedule">The citizen's schedule to update.</param>
        /// <param name="citizenAge">The age of the citizen.</param>
        public void ScheduleReturnFromWork(ref CitizenSchedule schedule, Citizen.AgeGroup citizenAge)
        {
            if (schedule.WorkStatus != WorkStatus.Working)
            {
                return;
            }

            float departureHour = schedule.WorkShiftEndHour + GetOvertime(citizenAge);
            schedule.Schedule(ResidentState.Unknown, timeInfo.Now.FutureHour(departureHour));
        }

        private static bool IsBuildingActiveOnWeekend(ItemClass.Service service, ItemClass.SubService subService)
        {
            switch (service)
            {
                case ItemClass.Service.Commercial
                    when subService != ItemClass.SubService.CommercialHigh && subService != ItemClass.SubService.CommercialEco:
                case ItemClass.Service.Industrial when subService != ItemClass.SubService.IndustrialGeneric:
                case ItemClass.Service.Tourism:
                case ItemClass.Service.Electricity:
                case ItemClass.Service.Water:
                case ItemClass.Service.Beautification:
                case ItemClass.Service.HealthCare:
                case ItemClass.Service.PoliceDepartment:
                case ItemClass.Service.FireDepartment:
                case ItemClass.Service.PublicTransport:
                case ItemClass.Service.Disaster:
                case ItemClass.Service.Monument:
                case ItemClass.Service.Garbage:
                case ItemClass.Service.Road:
                    return true;

                default:
                    return false;
            }
        }

        private static int GetBuildingWorkShiftCount(ItemClass.Service service, ItemClass.SubService subService)
        {
            switch (service)
            {
                case ItemClass.Service.Office:
                case ItemClass.Service.Education:
                case ItemClass.Service.Industrial
                    when subService == ItemClass.SubService.IndustrialForestry || subService == ItemClass.SubService.IndustrialFarming:
                    return 1;

                case ItemClass.Service.Beautification:
                case ItemClass.Service.Monument:
                case ItemClass.Service.Citizen:
                    return 2;

                case ItemClass.Service.Commercial:
                case ItemClass.Service.Industrial:
                case ItemClass.Service.Tourism:
                case ItemClass.Service.Electricity:
                case ItemClass.Service.Water:
                case ItemClass.Service.HealthCare:
                case ItemClass.Service.PoliceDepartment:
                case ItemClass.Service.FireDepartment:
                case ItemClass.Service.PublicTransport:
                case ItemClass.Service.Disaster:
                case ItemClass.Service.Natural:
                case ItemClass.Service.Garbage:
                case ItemClass.Service.Road:
                    return 3;

                default:
                    return 1;
            }
        }

        private static bool HasExtendedFirstWorkShift(ItemClass.Service service, ItemClass.SubService subService)
        {
            switch (service)
            {
                case ItemClass.Service.Commercial when subService == ItemClass.SubService.CommercialLow:
                case ItemClass.Service.Beautification:
                case ItemClass.Service.Education:
                case ItemClass.Service.Industrial
                    when subService == ItemClass.SubService.IndustrialFarming || subService == ItemClass.SubService.IndustrialForestry:
                    return true;

                default:
                    return false;
            }
        }

        private bool IsBuildingWorking(int workShiftCount, bool extendedFirstShift)
        {
            float endHour;
            switch (workShiftCount)
            {
                case 1:
                    endHour = config.WorkEnd;
                    break;

                case 2:
                    endHour = 24f;
                    break;

                default:
                    return true;
            }

            float startHour = extendedFirstShift ? Math.Min(config.WakeUpHour, EarliestWakeUp) : config.WorkBegin;
            float currentHour = timeInfo.CurrentHour;
            return currentHour >= startHour && currentHour < endHour;
        }

        private WorkShift GetWorkShift(int workShiftCount)
        {
            switch (workShiftCount)
            {
                case 1:
                    return WorkShift.First;

                case 2:
                    return randomizer.ShouldOccur(config.SecondShiftQuota)
                        ? WorkShift.Second
                        : WorkShift.First;

                case 3:
                    int random = randomizer.GetRandomValue(100u);
                    if (random < config.NightShiftQuota)
                    {
                        return WorkShift.Night;
                    }
                    else if (random < config.SecondShiftQuota + config.NightShiftQuota)
                    {
                        return WorkShift.Second;
                    }

                    return WorkShift.First;

                default:
                    return WorkShift.Unemployed;
            }
        }

        private float GetTravelTimeToWork(ref CitizenSchedule schedule, ushort buildingId)
        {
            float result = schedule.CurrentState == ResidentState.AtHome
                ? schedule.TravelTimeToWork
                : 0;

            if (result <= 0)
            {
                result = travelBehavior.GetEstimatedTravelTime(buildingId, schedule.WorkBuilding);
            }

            return result;
        }

        private bool WillGoToLunch(Citizen.AgeGroup citizenAge)
        {
            if (!config.IsLunchtimeEnabled)
            {
                return false;
            }

            switch (citizenAge)
            {
                case Citizen.AgeGroup.Child:
                case Citizen.AgeGroup.Teen:
                case Citizen.AgeGroup.Senior:
                    return false;
            }

            return randomizer.ShouldOccur(config.LunchQuota);
        }

        private float GetOvertime(Citizen.AgeGroup citizenAge)
        {
            switch (citizenAge)
            {
                case Citizen.AgeGroup.Young:
                case Citizen.AgeGroup.Adult:
                    return randomizer.ShouldOccur(config.OnTimeQuota)
                        ? 0
                        : config.MaxOvertime * randomizer.GetRandomValue(100u) / 100f;

                default:
                    return 0;
            }
        }
    }
}
