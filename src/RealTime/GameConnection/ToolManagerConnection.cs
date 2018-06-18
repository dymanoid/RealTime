// <copyright file="ToolManagerConnection.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.GameConnection
{
    internal sealed class ToolManagerConnection : IToolManagerConnection
    {
        public ItemClass.Availability GetCurrentMode()
        {
            return ToolManager.instance.m_properties.m_mode;
        }
    }
}
