// <copyright file="TouristAIHook.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.GameConnection
{
    using System;
    using Harmony;
    using RealTime.CustomAI;
    using RealTime.Patching;
    using RealTime.Tools;
    using static HumanAIConnectionBase<TouristAI, Citizen>;
    using static TouristAIConnection<TouristAI, Citizen>;

    internal static class TouristAIHook
    {
        internal static RealTimeTouristAI<TouristAI, Citizen> RealTimeAI { get; set; }

        internal static TouristAIConnection<TouristAI, Citizen> GetTouristAIConnection()
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
                Log.Error($"The 'Real Time' mod failed to create a delegate for type 'TouristAI', no method patching for the class: '{e.Message}'");
                return null;
            }
        }

        [HarmonyPatch(typeof(TouristAI), nameof(UpdateLocation), null)]
        private static class UpdateLocation
        {
#pragma warning disable SA1313 // Parameter names must begin with lower-case letter
            public static bool Prefix(TouristAI __instance, uint citizenID, ref Citizen data)
#pragma warning restore SA1313 // Parameter names must begin with lower-case letter
            {
                RealTimeAI?.UpdateLocation(__instance, citizenID, ref data);
                return false;
            }
        }
    }
}
