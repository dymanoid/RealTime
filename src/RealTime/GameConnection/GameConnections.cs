// <copyright file="GameConnections.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.GameConnection
{
    using System;
    using RealTime.Simulation;

    internal sealed class GameConnections<TCitizen>
        where TCitizen : struct
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GameConnections{TCitizen}"/> class.
        /// </summary>
        /// <param name="timeInfo">An object that provides the game time information.</param>
        /// <param name="citizenConnection">A proxy object that provides a way to call the game-specific methods of the <see cref="Citizen"/> struct.</param>
        /// <param name="citizenManager">A proxy object that provides a way to call the game-specific methods of the <see cref="CitizenManager"/> class.</param>
        /// <param name="buildingManager">A proxy object that provides a way to call the game-specific methods of the <see cref="BuildingManager"/> class.</param>
        /// <param name="eventManager">A proxy object that provides a way to call the game-specific methods of the <see cref="EventManager"/> class.</param>
        /// <param name="simulationManager">A proxy object that provides a way to call the game-specific methods of the <see cref="SimulationManager"/> class.</param>
        public GameConnections(
            ITimeInfo timeInfo,
            ICitizenConnection<TCitizen> citizenConnection,
            ICitizenManagerConnection citizenManager,
            IBuildingManagerConnection buildingManager,
            IEventManagerConnection eventManager,
            ISimulationManagerConnection simulationManager)
        {
            TimeInfo = timeInfo ?? throw new ArgumentNullException(nameof(timeInfo));
            CitizenConnection = citizenConnection ?? throw new ArgumentNullException(nameof(citizenConnection));
            CitizenManager = citizenManager ?? throw new ArgumentNullException(nameof(citizenManager));
            BuildingManager = buildingManager ?? throw new ArgumentNullException(nameof(buildingManager));
            EventManager = eventManager ?? throw new ArgumentNullException(nameof(eventManager));
            SimulationManager = simulationManager ?? throw new ArgumentNullException(nameof(simulationManager));
        }

        public ITimeInfo TimeInfo { get; }

        public ICitizenConnection<TCitizen> CitizenConnection { get; }

        public ICitizenManagerConnection CitizenManager { get; }

        public IBuildingManagerConnection BuildingManager { get; }

        public IEventManagerConnection EventManager { get; }

        public ISimulationManagerConnection SimulationManager { get; }
    }
}
