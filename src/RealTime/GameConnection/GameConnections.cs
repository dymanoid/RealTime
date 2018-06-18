// <copyright file="GameConnections.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.GameConnection
{
    using System;

    internal sealed class GameConnections<TAI, TCitizen>
        where TAI : class
        where TCitizen : struct
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GameConnections{TAI, TCitizen}"/> class.
        /// </summary>
        /// <param name="residentAI">A proxy object that provides a way to call the game-specific methods of the <see cref="ResidentAI"/> class.</param>
        /// <param name="citizenConnection">A proxy object that provides a way to call the game-specific methods of the <see cref="Citizen"/> struct.</param>
        /// <param name="citizenManager">A proxy object that provides a way to call the game-specific methods of the <see cref="CitizenManager"/> class.</param>
        /// <param name="buildingManager">A proxy object that provides a way to call the game-specific methods of the <see cref="BuildingManager"/> class.</param>
        /// <param name="eventManager">A proxy object that provides a way to call the game-specific methods of the <see cref="EventManager"/> class.</param>
        public GameConnections(
            ResidentAIConnection<TAI, TCitizen> residentAI,
            ICitizenConnection<TCitizen> citizenConnection,
            ICitizenManagerConnection citizenManager,
            IBuildingManagerConnection buildingManager,
            IEventManagerConnection eventManager)
        {
            ResidentAI = residentAI ?? throw new ArgumentNullException(nameof(residentAI));
            CitizenConnection = citizenConnection ?? throw new ArgumentNullException(nameof(citizenConnection));
            CitizenManager = citizenManager ?? throw new ArgumentNullException(nameof(citizenManager));
            BuildingManager = buildingManager ?? throw new ArgumentNullException(nameof(buildingManager));
            EventManager = eventManager ?? throw new ArgumentNullException(nameof(eventManager));
        }

        public ResidentAIConnection<TAI, TCitizen> ResidentAI { get; }

        public ICitizenConnection<TCitizen> CitizenConnection { get; }

        public ICitizenManagerConnection CitizenManager { get; }

        public IBuildingManagerConnection BuildingManager { get; }

        public IEventManagerConnection EventManager { get; }
    }
}
