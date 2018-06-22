// <copyright file="IEventProvider.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.Events
{
    internal interface IEventProvider
    {
        IRealTimeEvent GetRandomEvent(string buildingClass);
    }
}
