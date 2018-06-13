// <copyright file="TimeAdjustment.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.Simulation
{
    using System;
    using RealTime.Tools;

    /// <summary>
    /// Manages the customized time adjustment. This class depends on the <see cref="SimulationManager"/> class.
    /// </summary>
    internal sealed class TimeAdjustment
    {
        private const int CustomFramesPerDay = 1 << 16;
        private static readonly TimeSpan CustomTimePerFrame = new TimeSpan(24L * 3600L * 10_000_000L / CustomFramesPerDay);

        private readonly uint vanillaFramesPerDay;
        private readonly TimeSpan vanillaTimePerFrame;

        /// <summary>
        /// Initializes a new instance of the <see cref="TimeAdjustment"/> class.
        /// </summary>
        public TimeAdjustment()
        {
            vanillaFramesPerDay = SimulationManager.DAYTIME_FRAMES;
            vanillaTimePerFrame = SimulationManager.instance.m_timePerFrame;
        }

        /// <summary>
        /// Enables the customized time adjustment.
        /// </summary>
        ///
        /// <returns>The current game date and time.</returns>
        public DateTime Enable()
        {
            if (vanillaTimePerFrame == CustomTimePerFrame)
            {
                Log.Warning("The 'Real Time' mod has not been properly deactivated! Check the TimeAdjustment.Disable() calls.");
            }

            return UpdateTimeSimulationValues(CustomFramesPerDay, CustomTimePerFrame);
        }

        /// <summary>
        /// Disables the customized time adjustment restoring the default vanilla values.
        /// </summary>
        public void Disable()
        {
            UpdateTimeSimulationValues(vanillaFramesPerDay, vanillaTimePerFrame);
        }

        private static DateTime UpdateTimeSimulationValues(uint framesPerDay, TimeSpan timePerFrame)
        {
            SimulationManager sm = SimulationManager.instance;
            DateTime originalDate = sm.m_ThreadingWrapper.simulationTime;

            SimulationManager.DAYTIME_FRAMES = framesPerDay;
            SimulationManager.DAYTIME_FRAME_TO_HOUR = 24f / SimulationManager.DAYTIME_FRAMES;
            SimulationManager.DAYTIME_HOUR_TO_FRAME = SimulationManager.DAYTIME_FRAMES / 24f;

            sm.m_timePerFrame = timePerFrame;
            sm.m_timeOffsetTicks = originalDate.Ticks - (sm.m_currentFrameIndex * sm.m_timePerFrame.Ticks);
            sm.m_currentGameTime = originalDate;

            sm.m_currentDayTimeHour = (float)sm.m_currentGameTime.TimeOfDay.TotalHours;
            sm.m_dayTimeFrame = (uint)(SimulationManager.DAYTIME_FRAMES * sm.m_currentDayTimeHour / 24f);
            sm.m_dayTimeOffsetFrames = sm.m_dayTimeFrame - sm.m_currentFrameIndex & SimulationManager.DAYTIME_FRAMES - 1;

            return sm.m_currentGameTime;
        }
    }
}
