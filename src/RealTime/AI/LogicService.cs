// <copyright file="LogicService.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.AI
{
    using System;

    internal static class LogicService
    {
        public static void ProvideLogic(ILogic logic)
        {
            RealTimePrivateBuildingAI.Logic = logic ?? throw new ArgumentNullException(nameof(logic));
            RealTimeResidentAI.Logic = logic;
        }

        public static void RevokeLogic()
        {
            RealTimePrivateBuildingAI.Logic = null;
            RealTimeResidentAI.Logic = null;
        }
    }
}
