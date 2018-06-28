// <copyright file="GameConnections.cs" company="dymanoid">Copyright (c) dymanoid. All rights reserved.</copyright>

namespace RealTime.GameConnection
{
    using System;
    using RealTime.Simulation;

    /// <summary>A container class for accessing the individual game connection objects.</summary>
    /// <typeparam name="TCitizen">The type of the citizen.</typeparam>
    internal sealed class GameConnections<TCitizen>
        where TCitizen : struct
    {
        /// <summary>Initializes a new instance of the <see cref="GameConnections{TCitizen}"/> class.</summary>
        /// <param name="timeInfo">An object that provides the game time information.</param>
        /// <param name="citizenConnection">
        /// A proxy object that provides a way to call the game-specific methods of the <see cref="Citizen"/> struct.
        /// </param>
        /// <param name="citizenManager">
        /// A proxy object that provides a way to call the game-specific methods of the <see cref="global::CitizenManager"/> class.
        /// </param>
        /// <param name="buildingManager">
        /// A proxy object that provides a way to call the game-specific methods of the <see cref="global::BuildingManager"/> class.
        /// </param>
        /// <param name="simulationManager">
        /// A proxy object that provides a way to call the game-specific methods of the <see cref="global::SimulationManager"/> class.
        /// </param>
        /// <param name="transferManager">
        /// A proxy object that provides a way to call the game-specific methods of the <see cref="global::TransferManager"/> class.
        /// </param>
        public GameConnections(
            ITimeInfo timeInfo,
            ICitizenConnection<TCitizen> citizenConnection,
            ICitizenManagerConnection citizenManager,
            IBuildingManagerConnection buildingManager,
            ISimulationManagerConnection simulationManager,
            ITransferManagerConnection transferManager)
        {
            TimeInfo = timeInfo ?? throw new ArgumentNullException(nameof(timeInfo));
            CitizenConnection = citizenConnection ?? throw new ArgumentNullException(nameof(citizenConnection));
            CitizenManager = citizenManager ?? throw new ArgumentNullException(nameof(citizenManager));
            BuildingManager = buildingManager ?? throw new ArgumentNullException(nameof(buildingManager));
            SimulationManager = simulationManager ?? throw new ArgumentNullException(nameof(simulationManager));
            TransferManager = transferManager ?? throw new ArgumentNullException(nameof(transferManager));
        }

        /// <summary>Gets the <see cref="ITimeInfo"/> implementation.</summary>
        public ITimeInfo TimeInfo { get; }

        /// <summary>Gets the <see cref="ICitizenConnection{TCitizen}"/> implementation.</summary>
        public ICitizenConnection<TCitizen> CitizenConnection { get; }

        /// <summary>Gets the <see cref="ICitizenManagerConnection"/> implementation.</summary>
        public ICitizenManagerConnection CitizenManager { get; }

        /// <summary>Gets the <see cref="IBuildingManagerConnection"/> implementation.</summary>
        public IBuildingManagerConnection BuildingManager { get; }

        /// <summary>Gets the <see cref="ISimulationManagerConnection"/> implementation.</summary>
        public ISimulationManagerConnection SimulationManager { get; }

        /// <summary>Gets the <see cref="ITransferManagerConnection"/> implementation.</summary>
        public ITransferManagerConnection TransferManager { get; }
    }
}