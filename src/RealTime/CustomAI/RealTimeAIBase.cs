// <copyright file="RealTimeAIBase.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.CustomAI
{
    using ColossalFramework.Math;

    internal abstract class RealTimeAIBase
    {
        private Randomizer randomizer;

        protected RealTimeAIBase(ref Randomizer randomizer)
        {
            this.randomizer = randomizer;
        }

        protected ref Randomizer Randomizer => ref randomizer;

        protected bool IsChance(uint chance)
        {
            return Randomizer.UInt32(100u) < chance;
        }
    }
}
