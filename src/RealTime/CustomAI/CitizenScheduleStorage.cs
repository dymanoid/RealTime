// <copyright file="CitizenScheduleStorage.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.CustomAI
{
    using System;
    using System.IO;
    using RealTime.Simulation;
    using SkyTools.Storage;

    /// <summary>
    /// A helper class that enables loading and saving of the custom citizen schedules.
    /// This class accesses the <see cref="CitizenManager"/> directly for better performance.
    /// </summary>
    /// <seealso cref="IStorageData" />
    internal sealed class CitizenScheduleStorage : IStorageData
    {
        private const string StorageDataId = "RealTimeCitizenSchedule";

        private readonly CitizenSchedule[] residentSchedules;
        private readonly Citizen[] citizens;
        private readonly ITimeInfo timeInfo;

        /// <summary>Initializes a new instance of the <see cref="CitizenScheduleStorage"/> class.</summary>
        /// <param name="residentSchedules">The resident schedules to store or load.</param>
        /// <param name="citizensProvider">A method that returns the game's citizens array.</param>
        /// <param name="timeInfo">An object that provides the game time information.</param>
        /// <exception cref="ArgumentNullException">Thrown when any argument is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="residentSchedules"/> and the array returned by
        /// <paramref name="citizensProvider"/> have different lengths.</exception>
        public CitizenScheduleStorage(CitizenSchedule[] residentSchedules, Func<Citizen[]> citizensProvider, ITimeInfo timeInfo)
        {
            this.residentSchedules = residentSchedules ?? throw new ArgumentNullException(nameof(residentSchedules));
            if (citizensProvider == null)
            {
                throw new ArgumentNullException(nameof(citizensProvider));
            }

            this.timeInfo = timeInfo ?? throw new ArgumentNullException(nameof(timeInfo));

            citizens = citizensProvider();
            if (citizens == null || residentSchedules.Length != citizens.Length)
            {
                throw new ArgumentException($"{nameof(residentSchedules)} and citizens arrays must have equal length");
            }
        }

        /// <summary>Gets an unique ID of this storage data set.</summary>
        string IStorageData.StorageDataId => StorageDataId;

        /// <summary>Reads the data set from the specified <see cref="Stream" />.</summary>
        /// <param name="source">A <see cref="Stream" /> to read the data set from.</param>
        void IStorageData.ReadData(Stream source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            byte[] buffer = new byte[CitizenSchedule.DataRecordSize];
            long referenceTime = timeInfo.Now.Date.Ticks;

            for (int i = 0; i < citizens.Length; ++i)
            {
                var flags = citizens[i].m_flags;
                if ((flags & Citizen.Flags.Created) == 0
                    || (flags & Citizen.Flags.DummyTraffic) != 0)
                {
                    continue;
                }

                source.Read(buffer, 0, buffer.Length);
                residentSchedules[i].Read(buffer, referenceTime);
            }
        }

        /// <summary>Reads the data set to the specified <see cref="Stream" />.</summary>
        /// <param name="target">A <see cref="Stream" /> to write the data set to.</param>
        void IStorageData.StoreData(Stream target)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            byte[] buffer = new byte[CitizenSchedule.DataRecordSize];
            long referenceTime = timeInfo.Now.Date.Ticks;

            for (int i = 0; i < citizens.Length; ++i)
            {
                var flags = citizens[i].m_flags;
                if ((flags & Citizen.Flags.Created) == 0
                    || (flags & Citizen.Flags.DummyTraffic) != 0)
                {
                    continue;
                }

                residentSchedules[i].Write(buffer, referenceTime);
                target.Write(buffer, 0, buffer.Length);
            }
        }
    }
}
