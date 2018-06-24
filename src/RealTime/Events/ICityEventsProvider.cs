// <copyright file="ICityEventsProvider.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.Events
{
    using RealTime.Events.Storage;

    internal interface ICityEventsProvider
    {
        ICityEvent GetRandomEvent(string buildingClass);

        CityEventTemplate GetEventTemplate(string eventName, string buildingClassName);
    }
}
