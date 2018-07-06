// <copyright file="PrivateBuildingAIHook.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.GameConnection
{
    using Harmony;
    using RealTime.CustomAI;

    internal static class PrivateBuildingAIHook
    {
        public static RealTimePrivateBuildingAI RealTimeAI { get; set; }

        [HarmonyPatch(typeof(PrivateBuildingAI), nameof(GetConstructionTime))]
        private static class GetConstructionTime
        {
#pragma warning disable SA1313 // Parameter names must begin with lower-case letter
            private static bool Prefix(ref int __result)
#pragma warning restore SA1313 // Parameter names must begin with lower-case letter
            {
                __result = RealTimeAI?.GetConstructionTime() ?? 0;
                return false;
            }
        }
    }
}
