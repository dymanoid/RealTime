// <copyright file="TouristAIPatch.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.GameConnection.Patches
{
    using System;
    using System.Reflection;
    using RealTime.CustomAI;
    using RealTime.Patching;
    using RealTime.Tools;
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
                GetRandomTargetTypeDelegate getRandomTargetType
                    = FastDelegate.Create<TouristAI, GetRandomTargetTypeDelegate>("GetRandomTargetType");

                GetLeavingReasonDelegate getLeavingReason
                    = FastDelegate.Create<TouristAI, GetLeavingReasonDelegate>("GetLeavingReason");

                AddTouristVisitDelegate addTouristVisit
                    = FastDelegate.Create<TouristAI, AddTouristVisitDelegate>("AddTouristVisit");

                DoRandomMoveDelegate doRandomMove
                    = FastDelegate.Create<TouristAI, DoRandomMoveDelegate>("DoRandomMove");

                FindEvacuationPlaceDelegate findEvacuationPlace
                    = FastDelegate.Create<TouristAI, FindEvacuationPlaceDelegate>("FindEvacuationPlace");

                FindVisitPlaceDelegate findVisitPlace
                    = FastDelegate.Create<TouristAI, FindVisitPlaceDelegate>("FindVisitPlace");

                GetEntertainmentReasonDelegate getEntertainmentReason
                    = FastDelegate.Create<TouristAI, GetEntertainmentReasonDelegate>("GetEntertainmentReason");

                GetEvacuationReasonDelegate getEvacuationReason
                    = FastDelegate.Create<TouristAI, GetEvacuationReasonDelegate>("GetEvacuationReason");

                GetShoppingReasonDelegate getShoppingReason
                    = FastDelegate.Create<TouristAI, GetShoppingReasonDelegate>("GetShoppingReason");

                StartMovingDelegate startMoving
                    = FastDelegate.Create<TouristAI, StartMovingDelegate>("StartMoving");

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
            protected override MethodInfo GetMethod()
            {
                return typeof(TouristAI).GetMethod(
                    "UpdateLocation",
                    BindingFlags.Instance | BindingFlags.NonPublic,
                    null,
                    new[] { typeof(uint), typeof(Citizen).MakeByRefType() },
                    new ParameterModifier[0]);
            }

#pragma warning disable SA1313 // Parameter names must begin with lower-case letter
            private static bool Prefix(TouristAI __instance, uint citizenID, ref Citizen data)
            {
                RealTimeAI?.UpdateLocation(__instance, citizenID, ref data);
                return false;
            }
#pragma warning restore SA1313 // Parameter names must begin with lower-case letter
        }
    }
}
