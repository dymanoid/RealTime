// <copyright file="ResidentAIPatch.cs" company="dymanoid">Copyright (c) dymanoid. All rights reserved.</copyright>

namespace RealTime.GameConnection.Patches
{
    using System;
    using System.Reflection;
    using RealTime.CustomAI;
    using RealTime.Patching;
    using RealTime.Tools;
    using static HumanAIConnectionBase<ResidentAI, Citizen>;
    using static ResidentAIConnection<ResidentAI, Citizen>;

    /// <summary>
    /// A static class that provides the patch objects and the game connection objects for the resident AI .
    /// </summary>
    internal static class ResidentAIPatch
    {
        /// <summary>Gets or sets the custom AI object for resident citizens.</summary>
        public static RealTimeResidentAI<ResidentAI, Citizen> RealTimeAI { get; set; }

        /// <summary>Gets the patch object for the location method.</summary>
        public static IPatch Location { get; } = new ResidentAI_UpdateLocation();

        /// <summary>Creates a game connection object for the resident AI class.</summary>
        /// <returns>A new <see cref="ResidentAIConnection{ResidentAI, Citizen}"/> object.</returns>
        public static ResidentAIConnection<ResidentAI, Citizen> GetResidentAIConnection()
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

        private sealed class ResidentAI_UpdateLocation : PatchBase
        {
            protected override MethodInfo GetMethod()
            {
                return typeof(ResidentAI).GetMethod(
                    "UpdateLocation",
                    BindingFlags.Instance | BindingFlags.NonPublic,
                    null,
                    new[] { typeof(uint), typeof(Citizen).MakeByRefType() },
                    new ParameterModifier[0]);
            }

#pragma warning disable SA1313 // Parameter names must begin with lower-case letter

            private static bool Prefix(ResidentAI __instance, uint citizenID, ref Citizen data)
            {
                RealTimeAI?.UpdateLocation(__instance, citizenID, ref data);
                return false;
            }
#pragma warning restore SA1313 // Parameter names must begin with lower-case letter
        }
    }
}