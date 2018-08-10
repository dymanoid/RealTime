// <copyright file="CitizenProcessor.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.Simulation
{
    using System;
    using RealTime.CustomAI;

    /// <summary>
    /// A class that executes various citizen simulation logic that is not related to movement.
    /// </summary>
    /// <typeparam name="TAI">The type of the citizen AI.</typeparam>
    /// <typeparam name="TCitizen">The type of the citizen objects.</typeparam>
    internal sealed class CitizenProcessor<TAI, TCitizen>
        where TAI : class
        where TCitizen : struct
    {
        private const uint StepSize = 256;
        private const uint StepMask = 0xFFF;

        private readonly RealTimeResidentAI<TAI, TCitizen> residentAI;
        private readonly SpareTimeBehavior spareTimeBehavior;
        private readonly TravelBehavior travelBehavior;
        private readonly ITimeInfo timeInfo;
        private int cycleStartFrame;
        private int cycleHour;

        /// <summary>Initializes a new instance of the <see cref="CitizenProcessor{TAI, TCitizen}"/> class.</summary>
        /// <param name="residentAI">The custom resident AI implementation.</param>
        /// <param name="timeInfo">An object that provides the game time information.</param>
        /// <param name="spareTimeBehavior">A behavior that provides simulation info for the citizens spare time.</param>
        /// <param name="travelBehavior">A behavior that provides simulation info for citizens travelling.</param>
        /// <exception cref="ArgumentNullException">Thrown when any argument is null.</exception>
        public CitizenProcessor(
            RealTimeResidentAI<TAI, TCitizen> residentAI,
            ITimeInfo timeInfo,
            SpareTimeBehavior spareTimeBehavior,
            TravelBehavior travelBehavior)
        {
            this.residentAI = residentAI ?? throw new ArgumentNullException(nameof(residentAI));
            this.spareTimeBehavior = spareTimeBehavior ?? throw new ArgumentNullException(nameof(spareTimeBehavior));
            this.travelBehavior = travelBehavior ?? throw new ArgumentNullException(nameof(travelBehavior));
            this.timeInfo = timeInfo ?? throw new ArgumentNullException(nameof(timeInfo));
            cycleStartFrame = int.MinValue;
        }

        /// <summary>Notifies this simulation object that a particular day hour begins.</summary>
        /// <param name="hour">The day time hour.</param>
        public void TriggerHour(int hour)
        {
            if (hour % 8 == 0)
            {
                cycleHour = hour;
                cycleStartFrame = int.MinValue;
            }
        }

        /// <summary>Re-calculates the duration of a simulation frame.</summary>
        public void UpdateFrameDuration()
        {
            float cyclePeriod = timeInfo.HoursPerFrame * (StepMask + 1);
            residentAI.SetSimulationCyclePeriod(cyclePeriod);
            spareTimeBehavior.SetSimulationCyclePeriod(cyclePeriod);
            travelBehavior.SetSimulationCyclePeriod(cyclePeriod);
        }

        /// <summary>Processes the simulation tick.</summary>
        public void ProcessTick()
        {
            spareTimeBehavior.RefreshChances();
        }

        /// <summary>Processes the simulation frame.</summary>
        /// <param name="frameIndex">The index of the simulation frame to process.</param>
        public void ProcessFrame(uint frameIndex)
        {
            if (cycleStartFrame == -1)
            {
                return;
            }

            uint step = frameIndex & StepMask;
            if (cycleStartFrame == int.MinValue)
            {
                residentAI.BeginNewHourCycleProcessing(cycleHour);
                cycleStartFrame = (int)step;
            }
            else if (step == cycleStartFrame)
            {
                residentAI.EndHourCycleProcessing();
                cycleStartFrame = -1;
                return;
            }

            uint idFrom = step * StepSize;
            uint idTo = ((step + 1) * StepSize) - 1;

            for (uint i = idFrom; i <= idTo; i++)
            {
                if ((CitizenManager.instance.m_citizens.m_buffer[i].m_flags & Citizen.Flags.Created) == 0)
                {
                    continue;
                }

                residentAI.BeginNewDayForCitizen(i);
            }
        }
    }
}
