// <copyright file="RealTimePrivateBuildingAI.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.AI
{
    using Redirection;

    internal sealed class RealTimePrivateBuildingAI
    {
        [RedirectFrom(typeof(PrivateBuildingAI))]
        private static int GetConstructionTime(PrivateBuildingAI instance)
        {
            if ((ToolManager.instance.m_properties.m_mode & ItemClass.Availability.AssetEditor) != 0)
            {
                return 0;
            }

            // This causes the constuction to not advance in the night time
            return SimulationManager.instance.m_isNightTime ? 10880 : 1088;
        }
    }
}
