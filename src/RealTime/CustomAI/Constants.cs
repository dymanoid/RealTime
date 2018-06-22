// <copyright file="Constants.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.CustomAI
{
    internal static class Constants
    {
        public const int ShoppingGoodsAmount = 100;

        public const float LocalSearchDistance = 500f;
        public const float FullSearchDistance = 270f * 64f / 2f;

        public const uint AbandonTourChance = 25;
        public const uint AbandonTransportWaitChance = 80;
        public const uint GoShoppingChance = 50;
        public const uint ReturnFromShoppingChance = 25;
        public const uint ReturnFromVisitChance = 40;
        public const uint FindHotelChance = 80;
        public const uint TouristShoppingChance = 50;

        public const int TouristDoNothingProbability = 5000;

        public const float MaxHoursOnTheWay = 2.5f;
        public const float MinHoursOnTheWay = 0.5f;
        public const float OnTheWayDistancePerHour = 500f;
    }
}
