﻿// <copyright file="CitizenProcessor.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.Simulation
{
    using System;
    using RealTime.CustomAI;

    /// <summary>
    /// A class that executes various citizen simulation logic that is not related to movement.
    /// </summary>
    internal sealed class CitizenProcessor
    {
        private const uint StepSize = 256;
        private const uint StepMask = 0xFFF;

        private readonly RealTimeResidentAI<ResidentAI, Citizen> residentAI;
        private readonly SpareTimeBehavior spareTimeBehavior;
        private readonly ITimeInfo timeInfo;
        private int dayStartFrame;

        /// <summary>Initializes a new instance of the <see cref="CitizenProcessor"/> class.</summary>
        /// <param name="residentAI">The custom resident AI implementation.</param>
        /// <param name="spareTimeBehavior">A behavior that provides simulation info for the citizens spare time.</param>
        /// <param name="timeInfo">An object that provides the game time information.</param>
        /// <exception cref="ArgumentNullException">Thrown when any argument is null.</exception>
        public CitizenProcessor(RealTimeResidentAI<ResidentAI, Citizen> residentAI, SpareTimeBehavior spareTimeBehavior, ITimeInfo timeInfo)
        {
            this.residentAI = residentAI ?? throw new ArgumentNullException(nameof(residentAI));
            this.spareTimeBehavior = spareTimeBehavior ?? throw new ArgumentNullException(nameof(spareTimeBehavior));
            this.timeInfo = timeInfo ?? throw new ArgumentNullException(nameof(timeInfo));
            dayStartFrame = int.MinValue;
        }

        /// <summary>Notifies this simulation object that a new game day begins.</summary>
        public void StartNewDay()
        {
            dayStartFrame = int.MinValue;
            residentAI.BeginNewDay();
        }

        /// <summary>Re-calculates the duration of a simulation frame.</summary>
        public void UpdateFrameDuration()
        {
            float cyclePeriod = timeInfo.HoursPerFrame * (StepMask + 1);
            residentAI.SetSimulationCyclePeriod(cyclePeriod);
            spareTimeBehavior.SetSimulationCyclePeriod(cyclePeriod);
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
            if (dayStartFrame == -1)
            {
                return;
            }

            uint step = frameIndex & StepMask;
            if (dayStartFrame == int.MinValue)
            {
                dayStartFrame = (int)step;
            }
            else if (step == dayStartFrame)
            {
                dayStartFrame = -1;
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
