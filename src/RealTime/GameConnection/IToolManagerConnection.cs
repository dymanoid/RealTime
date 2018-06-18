// <copyright file="IToolManagerConnection.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.GameConnection
{
    internal interface IToolManagerConnection
    {
        ItemClass.Availability GetCurrentMode();
    }
}
