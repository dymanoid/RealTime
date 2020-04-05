// <copyright file="TouristAIPatch.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.GameConnection.Patches
{
    using System;
    using System.Reflection;
    using RealTime.CustomAI;
    using SkyTools.Patching;
    using SkyTools.Tools;
    using static HumanAIConnectionBase<TouristAI, Citizen>;
    using static TouristAIConnection<TouristAI, Citizen>;

    /// <summary>
    /// A static class that provides the patch objects and the game connection objects for the tourist AI .
    /// </summary>
    internal static class TouristAIPatch
    {
        /// <summary>Gets or sets the custom AI object for tourists.</summary>
        public static RealTimeTouristAI<TouristAI, Citizen> RealTimeAI { get; set; }

        /// <summary>Gets the patch object for the location method.</summary>
        public static IPatch Location { get; } = new TouristAI_UpdateLocation();

        /// <summary>Creates a game connection object for the tourist AI class.</summary>
        /// <returns>A new <see cref="TouristAIConnection{TouristAI, Citizen}"/> object.</returns>
        public static TouristAIConnection<TouristAI, Citizen> GetTouristAIConnection()
        {
            try
            {
                var getRandomTargetType
                    = FastDelegateFactory.Create<GetRandomTargetTypeDelegate>(typeof(TouristAI), "GetRandomTargetType", instanceMethod: true);

                var getLeavingReason
                    = FastDelegateFactory.Create<GetLeavingReasonDelegate>(typeof(TouristAI), "GetLeavingReason", instanceMethod: true);

                var addTouristVisit
                    = FastDelegateFactory.Create<AddTouristVisitDelegate>(typeof(TouristAI), "AddTouristVisit", instanceMethod: true);

                var doRandomMove
                    = FastDelegateFactory.Create<DoRandomMoveDelegate>(typeof(TouristAI), "DoRandomMove", instanceMethod: true);

                var findEvacuationPlace
                    = FastDelegateFactory.Create<FindEvacuationPlaceDelegate>(typeof(TouristAI), "FindEvacuationPlace", instanceMethod: true);

                var findVisitPlace
                    = FastDelegateFactory.Create<FindVisitPlaceDelegate>(typeof(TouristAI), "FindVisitPlace", instanceMethod: true);

                var getEntertainmentReason
                    = FastDelegateFactory.Create<GetEntertainmentReasonDelegate>(typeof(TouristAI), "GetEntertainmentReason", instanceMethod: true);

                var getEvacuationReason
                    = FastDelegateFactory.Create<GetEvacuationReasonDelegate>(typeof(TouristAI), "GetEvacuationReason", instanceMethod: true);

                var getShoppingReason
                    = FastDelegateFactory.Create<GetShoppingReasonDelegate>(typeof(TouristAI), "GetShoppingReason", instanceMethod: true);

                var startMoving
                    = FastDelegateFactory.Create<StartMovingDelegate>(typeof(TouristAI), "StartMoving", instanceMethod: true);

                return new TouristAIConnection<TouristAI, Citizen>(
                    getRandomTargetType,
                    getLeavingReason,
                    addTouristVisit,
                    doRandomMove,
                    findEvacuationPlace,
                    findVisitPlace,
                    getEntertainmentReason,
                    getEvacuationReason,
                    getShoppingReason,
                    startMoving);
            }
            catch (Exception e)
            {
                Log.Error("The 'Real Time' mod failed to create a delegate for type 'TouristAI', no method patching for the class: " + e);
                return null;
            }
        }

        private sealed class TouristAI_UpdateLocation : PatchBase
        {
            protected override MethodInfo GetMethod() =>
                typeof(TouristAI).GetMethod(
                    "UpdateLocation",
                    BindingFlags.Instance | BindingFlags.NonPublic,
                    null,
                    new[] { typeof(uint), typeof(Citizen).MakeByRefType() },
                    new ParameterModifier[0]);

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Redundancy", "RCS1213", Justification = "Harmony patch")]
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming Rules", "SA1313", Justification = "Harmony patch")]
            private static bool Prefix(TouristAI __instance, uint citizenID, ref Citizen data)
            {
                RealTimeAI.UpdateLocation(__instance, citizenID, ref data);
                return false;
            }
        }
    }
}
