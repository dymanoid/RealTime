// <copyright file="GameConnections.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.GameConnection
{
    using System;

    internal sealed class GameConnections<T>
        where T : class
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GameConnections{T}"/> class.
        /// </summary>
        /// <param name="residentAI">A proxy object that provides a way to call the game-specific methods of the <see cref="ResidentAI"/> class.</param>
        /// <param name="citizenManager">A proxy object that provides a way to call the game-specific methods of the <see cref="CitizenManager"/> class.</param>
        /// <param name="buildingManager">A proxy object that provides a way to call the game-specific methods of the <see cref="BuildingManager"/> class.</param>
        /// <param name="eventManager">A proxy object that provides a way to call the game-specific methods of the <see cref="EventManager"/> class.</param>
        public GameConnections(
            ResidentAIConnection<T> residentAI,
            ICitizenManagerConnection citizenManager,
            IBuildingManagerConnection buildingManager,
            IEventManagerConnection eventManager)
        {
            ResidentAI = residentAI ?? throw new ArgumentNullException(nameof(residentAI));
            CitizenManager = citizenManager ?? throw new ArgumentNullException(nameof(citizenManager));
            BuildingManager = buildingManager ?? throw new ArgumentNullException(nameof(buildingManager));
            EventManager = eventManager ?? throw new ArgumentNullException(nameof(eventManager));
        }

        public ResidentAIConnection<T> ResidentAI { get; }

        public ICitizenManagerConnection CitizenManager { get; }

        public IBuildingManagerConnection BuildingManager { get; }

        public IEventManagerConnection EventManager { get; }
    }
}
