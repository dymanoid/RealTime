// <copyright file="PrivateBuildingAIHook.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.GameConnection
{
    using RealTime.CustomAI;
    using Redirection;

    internal static class PrivateBuildingAIHook
    {
        public static RealTimePrivateBuildingAI RealTimeAI { get; set; }

        [RedirectFrom(typeof(PrivateBuildingAI))]
        private static int GetConstructionTime(PrivateBuildingAI instance)
        {
            return RealTimeAI?.GetConstructionTime() ?? 0;
        }
    }
}
