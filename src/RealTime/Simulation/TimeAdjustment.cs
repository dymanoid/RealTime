// <copyright file="TimeAdjustment.cs" company="dymanoid">Copyright (c) dymanoid. All rights reserved.</copyright>

namespace RealTime.Simulation
{
    using System;
    using RealTime.Config;
    using UnityEngine;

    /// <summary>
    /// Manages the customized time adjustment. This class depends on the <see cref="SimulationManager"/> class.
    /// </summary>
    internal sealed class TimeAdjustment
    {
        private const int RealtimeSpeed = 23;
        private readonly uint vanillaFramesPerDay;
        private readonly TimeSpan vanillaTimePerFrame;
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
            vanillaTimePerFrame = SimulationManager.instance.m_timePerFrame;
        }

        /// <summary>Enables the customized time adjustment.</summary>
        /// <param name="setDefaultTime"><c>true</c> to initialize the game time to a default value (real world date and city wake up hour);
        /// <c>false</c> to leave the game time unchanged.</param>
        /// <returns>The current game date and time.</returns>
        public DateTime Enable(bool setDefaultTime)
        {
            dayTimeSpeed = config.DayTimeSpeed;
            nightTimeSpeed = config.NightTimeSpeed;
            isNightEnabled = SimulationManager.instance.m_enableDayNight;

            DateTime now = SimulationManager.instance.m_ThreadingWrapper.simulationTime;
            if (setDefaultTime)
            {
                now = now.Date.AddHours(config.WakeUpHour);
                SetGameDateTime(now);
            }

            float currentHour = now.TimeOfDay.Hours;
            isNightTime = currentHour < config.WakeUpHour || currentHour >= config.GoToSleepHour;

            return UpdateTimeSimulationValues(CalculateFramesPerDay(), useCustomTimePerFrame: true);
        }

        /// <summary>Updates the time adjustment to be synchronized with the configuration and the daytime.</summary>
        /// <param name="force"><c>true</c> to force the update.</param>
        /// <returns><c>true</c> if the time adjustment was updated; otherwise, <c>false</c>.</returns>
        public bool Update(bool force)
        {
            SimulationManager sm = SimulationManager.instance;
            if (sm.m_enableDayNight != isNightEnabled || force)
            {
                isNightEnabled = SimulationManager.instance.m_enableDayNight;
            }
            else if (!sm.m_enableDayNight
                || sm.m_isNightTime == isNightTime && dayTimeSpeed == config.DayTimeSpeed && nightTimeSpeed == config.NightTimeSpeed)
            {
                return false;
            }

            float currentHour = SimulationManager.instance.m_currentGameTime.TimeOfDay.Hours;
            isNightTime = currentHour < config.WakeUpHour || currentHour >= config.GoToSleepHour;
            dayTimeSpeed = config.DayTimeSpeed;
            nightTimeSpeed = config.NightTimeSpeed;

            uint currentFramesPerDay = SimulationManager.DAYTIME_FRAMES;
            uint newFramesPerDay = CalculateFramesPerDay();
            UpdateTimeSimulationValues(newFramesPerDay, useCustomTimePerFrame: true);
            return currentFramesPerDay != newFramesPerDay;
        }

        /// <summary>Disables the customized time adjustment restoring the default vanilla values.</summary>
        public void Disable() => UpdateTimeSimulationValues(vanillaFramesPerDay, useCustomTimePerFrame: false);

        /// <summary>Gets the original time represented by the frame index.
        /// This method can be used to convert frame-based times after time adjustments.</summary>
        /// <param name="frameIndex">A frame index representing a time point.</param>
        /// <returns>A <see cref="DateTime"/> object for the specified <paramref name="frameIndex"/>.</returns>
        public DateTime GetOriginalTime(uint frameIndex) => new DateTime(frameIndex * originalTimePerFrame.Ticks + originalTimeOffsetTicks);

        /// <summary>Updates the sun position by recalculating the relative day time.</summary>
        public void UpdateSunPosition()
        {
            if (!config.IsDynamicDayLengthEnabled)
            {
                return;
            }

            float time = Mathf.Clamp(SimulationManager.instance.m_currentDayTimeHour, 0f, 24f);
            float sunrise = SimulationManager.SUNRISE_HOUR;
            float sunset = SimulationManager.SUNSET_HOUR;

            float interpolated;
            if (time >= sunrise && time <= sunset)
            {
                 interpolated = Mathf.Lerp(6f, 18f, (time - sunrise) / (sunset - sunrise));
            }
            else
            {
                if (time < sunset)
                {
                    time += 24f;
                }

                interpolated = Mathf.Lerp(18f, 30f, (time - sunset) / (24f - sunset + sunrise));
                if (interpolated >= 24f)
                {
                    interpolated -= 24f;
                }
            }

            DayNightProperties.instance.m_TimeOfDay = interpolated;
        }

        private static void SetGameDateTime(DateTime dateTime)
        {
            SimulationManager sm = SimulationManager.instance;
            sm.m_timeOffsetTicks = dateTime.Ticks - sm.m_currentFrameIndex * sm.m_timePerFrame.Ticks;
            sm.m_currentGameTime = dateTime;

            sm.m_currentDayTimeHour = (float)sm.m_currentGameTime.TimeOfDay.TotalHours;
            sm.m_dayTimeFrame = (uint)(SimulationManager.DAYTIME_FRAMES * sm.m_currentDayTimeHour / 24f);
            sm.m_dayTimeOffsetFrames = sm.m_dayTimeFrame - sm.m_currentFrameIndex & SimulationManager.DAYTIME_FRAMES - 1;
        }

        private DateTime UpdateTimeSimulationValues(uint framesPerDay, bool useCustomTimePerFrame)
        {
            SimulationManager.DAYTIME_FRAMES = framesPerDay;
            SimulationManager.DAYTIME_FRAME_TO_HOUR = 24f / SimulationManager.DAYTIME_FRAMES;
            SimulationManager.DAYTIME_HOUR_TO_FRAME = SimulationManager.DAYTIME_FRAMES / 24f;

            SimulationManager sm = SimulationManager.instance;

            originalTimePerFrame = sm.m_timePerFrame;
            originalTimeOffsetTicks = sm.m_timeOffsetTicks;

            DateTime originalDate = sm.m_ThreadingWrapper.simulationTime;
            sm.m_timePerFrame = useCustomTimePerFrame
                ? new TimeSpan(24L * 3600L * 10_000_000L / framesPerDay)
                : vanillaTimePerFrame;

            SetGameDateTime(originalDate);

            return sm.m_currentGameTime;
        }

        private uint CalculateFramesPerDay()
        {
            uint offset = isNightTime ? nightTimeSpeed : dayTimeSpeed;
            return 1u << (int)(RealtimeSpeed - offset);
        }
    }
}