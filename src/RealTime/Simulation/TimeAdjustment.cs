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

        private uint dayTimeSpeed;
        private uint nightTimeSpeed;
        private bool isNightTime;
        private bool isNightEnabled;
        private TimeSpan originalTimePerFrame;
        private long originalTimeOffsetTicks;

        /// <summary>Initializes a new instance of the <see cref="TimeAdjustment"/> class.</summary>
        /// <param name="config">The configuration to run with.</param>
        /// <exception cref="ArgumentNullException">Thrown when the argument is null.</exception>
        public TimeAdjustment(RealTimeConfig config)
        {
            this.config = config ?? throw new ArgumentNullException(nameof(config));
            vanillaFramesPerDay = SimulationManager.DAYTIME_FRAMES;
        }

        /// <summary>Enables the customized time adjustment.</summary>
        /// <returns>The current game date and time.</returns>
        public DateTime Enable()
        {
            dayTimeSpeed = config.DayTimeSpeed;
            nightTimeSpeed = config.NightTimeSpeed;
            isNightTime = SimulationManager.instance.m_isNightTime;
            isNightEnabled = SimulationManager.instance.m_enableDayNight;
            return UpdateTimeSimulationValues(CalculateFramesPerDay());
        }

        /// <summary>Updates the time adjustment to be synchronized with the configuration and the daytime.</summary>
        /// <returns><c>true</c> if the time adjustment was updated; otherwise, <c>false</c>.</returns>
        public bool Update()
        {
            if (SimulationManager.instance.m_enableDayNight != isNightEnabled)
            {
                isNightEnabled = SimulationManager.instance.m_enableDayNight;
            }
            else if (!SimulationManager.instance.m_enableDayNight ||
                (SimulationManager.instance.m_isNightTime == isNightTime
                && dayTimeSpeed == config.DayTimeSpeed
                && nightTimeSpeed == config.NightTimeSpeed))
            {
                return false;
            }

            isNightTime = SimulationManager.instance.m_isNightTime;
            dayTimeSpeed = config.DayTimeSpeed;
            nightTimeSpeed = config.NightTimeSpeed;

            UpdateTimeSimulationValues(CalculateFramesPerDay());
            return true;
        }

        /// <summary>Disables the customized time adjustment restoring the default vanilla values.</summary>
        public void Disable()
        {
            UpdateTimeSimulationValues(vanillaFramesPerDay);
        }

        /// <summary>Gets the original time represented by the frame index.
        /// This method can be used to convert frame-based times after time adjustments.</summary>
        /// <param name="frameIndex">A frame index representing a time point.</param>
        /// <returns>A <see cref="DateTime"/> object for the specified <paramref name="frameIndex"/>.</returns>
        public DateTime GetOriginalTime(uint frameIndex)
        {
            return new DateTime((frameIndex * originalTimePerFrame.Ticks) + originalTimeOffsetTicks);
        }

        private DateTime UpdateTimeSimulationValues(uint framesPerDay)
        {
            SimulationManager sm = SimulationManager.instance;
            DateTime originalDate = sm.m_ThreadingWrapper.simulationTime;

            SimulationManager.DAYTIME_FRAMES = framesPerDay;
            SimulationManager.DAYTIME_FRAME_TO_HOUR = 24f / SimulationManager.DAYTIME_FRAMES;
            SimulationManager.DAYTIME_HOUR_TO_FRAME = SimulationManager.DAYTIME_FRAMES / 24f;

            originalTimePerFrame = sm.m_timePerFrame;
            originalTimeOffsetTicks = sm.m_timeOffsetTicks;
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
            uint offset = isNightTime ? nightTimeSpeed : dayTimeSpeed;
            return 1u << (int)(RealtimeSpeed - offset);
        }
    }
}