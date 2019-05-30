// <copyright file="Constants.cs" company="dymanoid">Copyright (c) dymanoid. All rights reserved.</copyright>

namespace RealTime.CustomAI
{
    /// <summary>A static class containing various constant values for the custom logic classes.</summary>
    internal static class Constants
    {
        /// <summary>The goods amount to buy for one citizens at one time.</summary>
        public const int ShoppingGoodsAmount = 100;

        /// <summary>A distance in game units where to search a 'local' building.</summary>
        public const float LocalSearchDistance = BuildingManager.BUILDINGGRID_CELL_SIZE * 5;

        /// <summary>A distance in game units where to search a leisure building.</summary>
        public const float LeisureSearchDistance = BuildingManager.BUILDINGGRID_CELL_SIZE * 10;

        /// <summary>A distance in game units where to search a hotel.</summary>
        public const float HotelSearchDistance = BuildingManager.BUILDINGGRID_CELL_SIZE * 20;

        /// <summary>A chance in percent for a citizen to stay home until next scheduled action.</summary>
        public const uint StayHomeAllDayChance = 2u;

        /// <summary>A chance in percent that a citizen going on vacation will cause his/her family members to go on vacation too.</summary>
        public const uint FamilyVacationChance = 30u;

        /// <summary>A chance in percent for a citizen to fin dome other facility if they continue shopping/relaxing.</summary>
        public const uint FindAnotherShopOrEntertainmentChance = 50u;

        /// <summary>A chance in percent for a citizen to go shopping in the night.</summary>
        public const uint NightShoppingChance = 20u;

        /// <summary>A chance in percent for a citizen to go to a leisure building in the night instead of usual entertainment.</summary>
        public const uint NightLeisureChance = 70u;

        /// <summary>A chance in percent for a tourist to find a hotel for sleepover.</summary>
        public const uint FindHotelChance = 60;

        /// <summary>A chance in percent for a tourist to go shopping.</summary>
        public const uint TouristShoppingChance = 50;

        /// <summary>A chance in percent for a tourist to go to an event.</summary>
        public const uint TouristEventChance = 70;

        /// <summary>A hard coded game value describing a 'do nothing' probability for a tourist.</summary>
        public const int TouristDoNothingProbability = 5000;

        /// <summary>The amount of hours the citizen will spend preparing to work and not going out.</summary>
        public const float PrepareToWorkHours = 1f;

        /// <summary>An assumed maximum travel time to a target building (in hours).</summary>
        public const float MaxTravelTime = 4f;

        /// <summary>An assumed minimum travel time to a target building (in hours).</summary>
        public const float MinTravelTime = 0.5f;

        /// <summary>An earliest hour when citizens wake up at home.</summary>
        public const float EarliestWakeUp = 5.5f;

        /// <summary>A chance in percent that a virtual citizen will not be realized in 'few virtual citizens' mode.</summary>
        public const uint FewVirtualCitizensChance = 20;

        /// <summary>A chance in percent that a virtual citizen will not be realized in 'many virtual citizen' mode.</summary>
        public const uint ManyVirtualCitizensChance = 50;

        /// <summary>A precipitation threshold that indicates when the citizens think it's a bad weather.</summary>
        public const float BadWeatherPrecipitationThreshold = 0.05f;

        /// <summary>The minimum probability that a citizen will stay inside a building on any precipitation.</summary>
        public const uint MinimumStayInsideChanceOnPrecipitation = 85u;

        /// <summary>The interval in minutes for the buildings problem timers.</summary>
        public const int ProblemTimersInterval = 10;

        /// <summary>The chance of a young female to get pregnant.</summary>
        public const uint YoungFemalePregnancyChance = 50u;

        /// <summary>The average distance a citizen can travel for (walking, by car, by public transport) during a single simulation cycle.
        /// This value was determined empirically.</summary>
        public const float AverageTravelDistancePerCycle = 450f;

        /// <summary>The maximum number of buildings (of one zone type) that are in construction or upgrading process.</summary>
        public const int MaximumBuildingsInConstruction = 50;
    }
}