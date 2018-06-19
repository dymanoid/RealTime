// <copyright file="Constants.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.CustomAI
{
    using System;

    internal static class Constants
    {
        public const int ShoppingGoodsAmount = 100;

        public const float LocalSearchDistance = 500f;
        public const float FullSearchDistance = 270f * 64f / 2f;

        public const uint AbandonTourChance = 25;
        public const uint GoShoppingChance = 50;
        public const uint ReturnFromShoppingChance = 25;
        public const uint ReturnFromVisitChance = 40;
        public const uint FindHotelChance = 80;
        public const uint TouristShoppingChance = 50;

        public const int TouristDoNothingProbability = 5000;

        public const float MaxHoursOnTheWayToWork = 2.5f;
        public const float MinHoursOnTheWayToWork = 0.5f;
        public const float OnTheWayDistancePerHour = 500f;

        public const float WakeUpHour = 7f;
        public const float LatestTeenEntertainmentHour = 21f;
        public const float LatestAdultEntertainmentHour = 23.99f;

        public static readonly TimeSpan AssumedGoOutDuration = TimeSpan.FromHours(12);
    }
}
