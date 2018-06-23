// <copyright file="ICityEventsProvider.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.Events
{
    internal interface ICityEventsProvider
    {
        ICityEvent GetRandomEvent(string buildingClass);
    }
}
