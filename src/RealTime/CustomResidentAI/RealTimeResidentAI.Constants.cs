// <copyright file="RealTimeResidentAI.Constants.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.CustomAI
{
    using System;

    internal partial class RealTimeResidentAI<TAI, TCitizen>
    {
        private const int ShoppingGoodsAmount = 100;

        private const float LocalSearchDistance = 500f;
        private const float FullSearchDistance = 270f * 64f / 2f;

        private const int AbandonTourChance = 25;
        private const int GoShoppingChance = 50;
        private const int ReturnFromShoppingChance = 25;
        private const int ReturnFromVisitChance = 40;

        private const float MaxHoursOnTheWayToWork = 2.5f;
        private const float MinHoursOnTheWayToWork = 0.5f;
        private const float OnTheWayDistancePerHour = 500f;

        private const float WakeUpHour = 7f;
        private const float LatestTeenEntertainmentHour = 21f;
        private const float LatestAdultEntertainmentHour = 23.99f;

        private static readonly TimeSpan AssumedGoOutDuration = TimeSpan.FromHours(12);
    }
}
