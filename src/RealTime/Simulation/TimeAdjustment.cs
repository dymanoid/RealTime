// <copyright file="TimeAdjustment.cs" company="dymanoid">Copyright (c) dymanoid. All rights reserved.</copyright>

namespace RealTime.Simulation
{
    using System;
    using RealTime.Config;

    /// <summary>
    /// Manages the customized time adjustment. This class depends on the <see cref="SimulationManager"/> class.
    /// </summary>
    internal sealed class TimeAdjustment
    {
        private const int RealtimeSpeed = 23;
        private readonly uint vanillaFramesPerDay;
        private readonly RealTimeConfig config;
        private uint lastDayTimeSpeed;
        private uint lastNightTimeSpeed;
        private bool isDayTime;

        /// <summary>Initializes a new instance of the <see cref="TimeAdjustment"/> class.</summary>
        /// <param name="config">The configuration to run with.</param>
        /// <exception cref="ArgumentNullException">Thrown when the argument is null.</exception>
        public TimeAdjustment(RealTimeConfig config)
        {
            this.config = config ?? throw new ArgumentNullException(nameof(config));
            lastDayTimeSpeed = config.DayTimeSpeed;
            lastNightTimeSpeed = config.NightTimeSpeed;
            vanillaFramesPerDay = SimulationManager.DAYTIME_FRAMES;
        }

        /// <summary>Enables the customized time adjustment.</summary>
        /// <returns>The current game date and time.</returns>
        public DateTime Enable()
        {
            isDayTime = !SimulationManager.instance.m_isNightTime;
            return UpdateTimeSimulationValues(CalculateFramesPerDay());
        }

        /// <summary>Updates the time adjustment to be synchronized with the configuration and the daytime.</summary>
        public void Update()
        {
            if (SimulationManager.instance.m_isNightTime == isDayTime
                || lastDayTimeSpeed != config.DayTimeSpeed
                || lastNightTimeSpeed != config.NightTimeSpeed)
            {
                isDayTime = !SimulationManager.instance.m_isNightTime;
                lastDayTimeSpeed = config.DayTimeSpeed;
                lastNightTimeSpeed = config.NightTimeSpeed;
                UpdateTimeSimulationValues(CalculateFramesPerDay());
            }
        }

        /// <summary>Disables the customized time adjustment restoring the default vanilla values.</summary>
        public void Disable()
        {
            UpdateTimeSimulationValues(vanillaFramesPerDay);
        }

        private static DateTime UpdateTimeSimulationValues(uint framesPerDay)
        {
            SimulationManager sm = SimulationManager.instance;
            DateTime originalDate = sm.m_ThreadingWrapper.simulationTime;

            SimulationManager.DAYTIME_FRAMES = framesPerDay;
            SimulationManager.DAYTIME_FRAME_TO_HOUR = 24f / SimulationManager.DAYTIME_FRAMES;
            SimulationManager.DAYTIME_HOUR_TO_FRAME = SimulationManager.DAYTIME_FRAMES / 24f;

            sm.m_timePerFrame = new TimeSpan(24L * 3600L * 10_000_000L / framesPerDay);
            sm.m_timeOffsetTicks = originalDate.Ticks - (sm.m_currentFrameIndex * sm.m_timePerFrame.Ticks);
            sm.m_currentGameTime = originalDate;

            sm.m_currentDayTimeHour = (float)sm.m_currentGameTime.TimeOfDay.TotalHours;
            sm.m_dayTimeFrame = (uint)(SimulationManager.DAYTIME_FRAMES * sm.m_currentDayTimeHour / 24f);
            sm.m_dayTimeOffsetFrames = sm.m_dayTimeFrame - sm.m_currentFrameIndex & SimulationManager.DAYTIME_FRAMES - 1;

            return sm.m_currentGameTime;
        }

        private uint CalculateFramesPerDay()
        {
            uint offset = isDayTime ? lastDayTimeSpeed : lastNightTimeSpeed;
            return 1u << (int)(RealtimeSpeed - offset);
        }
    }
}