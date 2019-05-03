// <copyright file="ToolManagerConnection.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.GameConnection
{
    /// <summary>
    /// The default implementation of the <see cref="IToolManagerConnection"/> interface.
    /// </summary>
    /// <seealso cref="IToolManagerConnection" />
    internal sealed class ToolManagerConnection : IToolManagerConnection
    {
        /// <summary>Gets the current game mode.</summary>
        /// <returns>The current game mode.</returns>
        public ItemClass.Availability GetCurrentMode() => ToolManager.instance.m_properties.m_mode;
    }
}
