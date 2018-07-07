// <copyright file="ResidentAIHook.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.GameConnection
{
    using System;
    using Harmony;
    using RealTime.CustomAI;
    using RealTime.Patching;
    using RealTime.Tools;
    using static HumanAIConnectionBase<ResidentAI, Citizen>;
    using static ResidentAIConnection<ResidentAI, Citizen>;

    internal static class ResidentAIHook
    {
        internal static RealTimeResidentAI<ResidentAI, Citizen> RealTimeAI { get; set; }

        internal static ResidentAIConnection<ResidentAI, Citizen> GetResidentAIConnection()
        {
            try
            {
                DoRandomMoveDelegate doRandomMove
                    = FastDelegate.Create<ResidentAI, DoRandomMoveDelegate>("DoRandomMove");

                FindEvacuationPlaceDelegate findEvacuationPlace
                    = FastDelegate.Create<ResidentAI, FindEvacuationPlaceDelegate>("FindEvacuationPlace");

                FindHospitalDelegate findHospital
                    = FastDelegate.Create<ResidentAI, FindHospitalDelegate>("FindHospital");

                FindVisitPlaceDelegate findVisitPlace
                    = FastDelegate.Create<ResidentAI, FindVisitPlaceDelegate>("FindVisitPlace");

                GetEntertainmentReasonDelegate getEntertainmentReason
                    = FastDelegate.Create<ResidentAI, GetEntertainmentReasonDelegate>("GetEntertainmentReason");

                GetEvacuationReasonDelegate getEvacuationReason
                    = FastDelegate.Create<ResidentAI, GetEvacuationReasonDelegate>("GetEvacuationReason");

                GetShoppingReasonDelegate getShoppingReason
                    = FastDelegate.Create<ResidentAI, GetShoppingReasonDelegate>("GetShoppingReason");

                StartMovingDelegate startMoving
                    = FastDelegate.Create<ResidentAI, StartMovingDelegate>("StartMoving");

                StartMovingWithOfferDelegate startMovingWithOffer
                    = FastDelegate.Create<ResidentAI, StartMovingWithOfferDelegate>("StartMoving");

                return new ResidentAIConnection<ResidentAI, Citizen>(
                    doRandomMove,
                    findEvacuationPlace,
                    findHospital,
                    findVisitPlace,
                    getEntertainmentReason,
                    getEvacuationReason,
                    getShoppingReason,
                    startMoving,
                    startMovingWithOffer);
            }
            catch (Exception e)
            {
                Log.Error("The 'Real Time' mod failed to create a delegate for type 'ResidentAI', no method patching for the class: " + e);
                return null;
            }
        }

        [HarmonyPatch(typeof(ResidentAI), nameof(UpdateLocation), null)]
        private static class UpdateLocation
        {
#pragma warning disable SA1313 // Parameter names must begin with lower-case letter
            public static bool Prefix(ResidentAI __instance, uint citizenID, ref Citizen data)
#pragma warning restore SA1313 // Parameter names must begin with lower-case letter
            {
                RealTimeAI?.UpdateLocation(__instance, citizenID, ref data);
                return false;
            }
        }
    }
}
