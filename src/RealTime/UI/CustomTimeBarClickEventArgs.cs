// <copyright file="CustomTimeBarClickEventArgs.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.UI
{
    using System;

    internal sealed class CustomTimeBarClickEventArgs : EventArgs
    {
        public CustomTimeBarClickEventArgs(ushort cityEventBuildingId)
        {
            CityEventBuildingId = cityEventBuildingId;
        }

        public ushort CityEventBuildingId { get; }
    }
}
