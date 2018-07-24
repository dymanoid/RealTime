// <copyright file="Constants.cs" company="dymanoid">Copyright (c) dymanoid. All rights reserved.</copyright>

namespace RealTime.CustomAI
{
    /// <summary>A static class containing various constant values for the custom logic classes.</summary>
    internal static class Constants
    {
        /// <summary>The goods amount to buy for one citizens at one time.</summary>
        public const int ShoppingGoodsAmount = 100;

        /// <summary>A distance in game units where to search a 'local' building.</summary>
        public const float LocalSearchDistance = BuildingManager.BUILDINGGRID_RESOLUTION * 2;

        /// <summary>A distance in game units where to search a leisure building.</summary>
        public const float LeisureSearchDistance = BuildingManager.BUILDINGGRID_RESOLUTION * 3;

        /// <summary>A distance in game units that corresponds to the complete map.</summary>
        public const float FullSearchDistance = BuildingManager.BUILDINGGRID_RESOLUTION * BuildingManager.BUILDINGGRID_CELL_SIZE / 2f;

        /// <summary>A chance in percent for an unemployed citizen to stay home until next morning.</summary>
        public const uint StayHomeAllDayChance = 15;

        /// <summary>A chance in percent for a citizen to go shopping.</summary>
        public const uint GoShoppingChance = 80;

        /// <summary>A chance in percent for a citizen to go to sleep when he or she is at home and doesn't go out.</summary>
        public const uint GoSleepingChance = 75;

        /// <summary>A chance in percent for a tourist to find a hotel for sleepover.</summary>
        public const uint FindHotelChance = 80;

        /// <summary>A chance in percent for a tourist to go shopping.</summary>
        public const uint TouristShoppingChance = 50;

        /// <summary>A chance in percent for a tourist to go to an event.</summary>
        public const uint TouristEventChance = 70;

        /// <summary>A hard coded game value describing a 'do nothing' probability for a tourist.</summary>
        public const int TouristDoNothingProbability = 5000;

        /// <summary>The amount of hours the citizen will spend preparing to work and not going out.</summary>
        public const float PrepareToWorkHours = 1f;

        /// <summary>An assumed maximum travel time to a target building.</summary>
        public const float MaxTravelTime = 2.5f;

        /// <summary>An assumed minimum travel to a target building.</summary>
        public const float MinTravelTime = 0.25f;

        /// <summary>An earliest hour when citizens wake up at home.</summary>
        public const float EarliestWakeUp = 5.5f;

        /// <summary>
        /// An assumed average speed of a citizen when moving to target (this is not just a walking speed, but also takes
        /// into account moving by car or public transport).
        /// </summary>
        // TODO: calculate this dynamically depending on time speed
        public const float OnTheWayDistancePerHour = 500f;

        /// <summary>A chance in percent that a virtual citizen will not be realized in 'few virtual citizens' mode.</summary>
        public const uint FewVirtualCitizensChance = 20;

        /// <summary>A chance in percent that a virtual citizen will not be realized in 'many virtual citizen' mode.</summary>
        public const uint ManyVirtualCitizensChance = 50;

        /// <summary>A precipitation threshold that indicates when the citizens think it's a bad weather.</summary>
        public const float BadWeatherPrecipitationThreshold = 0.05f;

        /// <summary>The minimum probability that a citizen will stay inside a building on any precipitation.</summary>
        public const uint MinimumStayInsideChanceOnPrecipitation = 75u;

        /// <summary>The interval in minutes for the buildings problem timers.</summary>
        public const int ProblemTimersInterval = 10;
    }
}