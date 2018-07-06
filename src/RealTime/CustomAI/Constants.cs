// <copyright file="Constants.cs" company="dymanoid">Copyright (c) dymanoid. All rights reserved.</copyright>

namespace RealTime.CustomAI
{
    /// <summary>A static class containing various constant values for the custom logic classes.</summary>
    internal static class Constants
    {
        /// <summary>The goods amount to buy for one citizens at one time.</summary>
        public const int ShoppingGoodsAmount = 100;

        /// <summary>A distance in game units where to search a 'local' building.</summary>
        public const float LocalSearchDistance = 270f * 2;

        /// <summary>A distance in game units where to search a leisure building.</summary>
        public const float LeisureSearchDistance = 270f * 3;

        /// <summary>A distance in game units that corresponds to the complete map.</summary>
        public const float FullSearchDistance = 270f * 64f / 2f;

        /// <summary>A chance in percent for a citizen to abandon the transport waiting if it lasts too long.</summary>
        public const uint AbandonTransportWaitChance = 80;

        /// <summary>A chance in percent for a citizen to go shopping.</summary>
        public const uint GoShoppingChance = 50;

        /// <summary>A chance in percent for a citizen to return shopping.</summary>
        public const uint ReturnFromShoppingChance = 60;

        /// <summary>A chance in percent for a citizen to return from a visited building.</summary>
        public const uint ReturnFromVisitChance = 40;

        /// <summary>A chance in percent for a tourist to find a hotel for sleepover.</summary>
        public const uint FindHotelChance = 80;

        /// <summary>A chance in percent for a tourist to go shopping.</summary>
        public const uint TouristShoppingChance = 50;

        /// <summary>A chance in percent for a tourist to go to an event.</summary>
        public const uint TouristEventChance = 70;

        /// <summary>A hard coded game value describing a 'do nothing' probability for a tourist.</summary>
        public const int TouristDoNothingProbability = 5000;

        /// <summary>An assumed maximum on-the-way time to a target building.</summary>
        public const float MaxHoursOnTheWay = 2.5f;

        /// <summary>An assumed minimum on-the-way time to a target building.</summary>
        public const float MinHoursOnTheWay = 0.5f;

        /// <summary>A minimum work shift duration in hours.</summary>
        public const float MinimumWorkShiftDuration = 2f;

        /// <summary>An earliest hour when citizens wake up at home.</summary>
        public const float EarliestWakeUp = 5.5f;

        /// <summary>
        /// An assumed average speed of a citizen when moving to target (this is not just a walking speed, but also takes
        /// into account moving by car or public transport).
        /// </summary>
        public const float OnTheWayDistancePerHour = 500f;

        /// <summary>A chance in percent that a virtual citizen will not be realized in 'few virtual citizens' mode.</summary>
        public const uint FewVirtualCitizensChance = 10;

        /// <summary>A chance in percent that a virtual citizen will not be realized in 'many virtual citizen' mode.</summary>
        public const uint ManyVirtualCitizensChance = 50;

        /// <summary>A precipitation threshold that indicates when the citizens think it's a bad weather.</summary>
        public const float BadWeatherPrecipitationThreshold = 0.05f;

        /// <summary>The minimum probability that a citizen will stay inside a building on any precipitation.</summary>
        public const uint MinimumStayInsideChanceOnPrecipitation = 75u;
    }
}