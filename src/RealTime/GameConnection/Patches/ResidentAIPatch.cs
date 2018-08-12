// <copyright file="ResidentAIPatch.cs" company="dymanoid">Copyright (c) dymanoid. All rights reserved.</copyright>

namespace RealTime.GameConnection.Patches
{
    using System;
    using System.Reflection;
    using RealTime.CustomAI;
    using SkyTools.Patching;
    using SkyTools.Tools;
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

        /// <summary>Gets the patch object for the arrive at target method.</summary>
        public static IPatch ArriveAtTarget { get; } = new HumanAI_ArriveAtTarget();

        /// <summary>Gets the patch object for the update age method.</summary>
        public static IPatch UpdateAge { get; } = new ResidentAI_UpdateAge();

        /// <summary>Gets the patch object for the 'can make babies' method.</summary>
        public static IPatch CanMakeBabies { get; } = new ResidentAI_CanMakeBabies();

        /// <summary>Creates a game connection object for the resident AI class.</summary>
        /// <returns>A new <see cref="ResidentAIConnection{ResidentAI, Citizen}"/> object.</returns>
        public static ResidentAIConnection<ResidentAI, Citizen> GetResidentAIConnection()
        {
            try
            {
                DoRandomMoveDelegate doRandomMove
                    = FastDelegateFactory.Create<DoRandomMoveDelegate>(typeof(ResidentAI), "DoRandomMove", true);

                FindEvacuationPlaceDelegate findEvacuationPlace
                    = FastDelegateFactory.Create<FindEvacuationPlaceDelegate>(typeof(ResidentAI), "FindEvacuationPlace", true);

                FindHospitalDelegate findHospital
                    = FastDelegateFactory.Create<FindHospitalDelegate>(typeof(ResidentAI), "FindHospital", true);

                FindVisitPlaceDelegate findVisitPlace
                    = FastDelegateFactory.Create<FindVisitPlaceDelegate>(typeof(ResidentAI), "FindVisitPlace", true);

                GetEntertainmentReasonDelegate getEntertainmentReason
                    = FastDelegateFactory.Create<GetEntertainmentReasonDelegate>(typeof(ResidentAI), "GetEntertainmentReason", true);

                GetEvacuationReasonDelegate getEvacuationReason
                    = FastDelegateFactory.Create<GetEvacuationReasonDelegate>(typeof(ResidentAI), "GetEvacuationReason", true);

                GetShoppingReasonDelegate getShoppingReason
                    = FastDelegateFactory.Create<GetShoppingReasonDelegate>(typeof(ResidentAI), "GetShoppingReason", true);

                StartMovingDelegate startMoving
                    = FastDelegateFactory.Create<StartMovingDelegate>(typeof(ResidentAI), "StartMoving", true);

                StartMovingWithOfferDelegate startMovingWithOffer
                    = FastDelegateFactory.Create<StartMovingWithOfferDelegate>(typeof(ResidentAI), "StartMoving", true);

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

        private sealed class HumanAI_ArriveAtTarget : PatchBase
        {
            protected override MethodInfo GetMethod()
            {
                return typeof(ResidentAI).GetMethod(
                    "ArriveAtTarget",
                    BindingFlags.Instance | BindingFlags.NonPublic,
                    null,
                    new[] { typeof(ushort), typeof(CitizenInstance).MakeByRefType() },
                    new ParameterModifier[0]);
            }

#pragma warning disable SA1313 // Parameter names must begin with lower-case letter
            private static void Postfix(ref CitizenInstance citizenData, bool __result)
            {
                if (__result)
                {
                    RealTimeAI?.RegisterCitizenArrival(citizenData.m_citizen);
                }
            }
#pragma warning restore SA1313 // Parameter names must begin with lower-case letter
        }

        private sealed class ResidentAI_UpdateAge : PatchBase
        {
            protected override MethodInfo GetMethod()
            {
                return typeof(ResidentAI).GetMethod(
                    "UpdateAge",
                    BindingFlags.Instance | BindingFlags.NonPublic,
                    null,
                    new[] { typeof(uint), typeof(Citizen).MakeByRefType() },
                    new ParameterModifier[0]);
            }

#pragma warning disable SA1313 // Parameter names must begin with lower-case letter
            private static bool Prefix(ref bool __result)
            {
                if (RealTimeAI != null && !RealTimeAI.CanCitizensGrowUp)
                {
                    __result = false;
                    return false;
                }

                return true;
            }
#pragma warning restore SA1313 // Parameter names must begin with lower-case letter
        }

        private sealed class ResidentAI_CanMakeBabies : PatchBase
        {
            protected override MethodInfo GetMethod()
            {
                return typeof(ResidentAI).GetMethod(
                    "CanMakeBabies",
                    BindingFlags.Instance | BindingFlags.Public,
                    null,
                    new[] { typeof(uint), typeof(Citizen).MakeByRefType() },
                    new ParameterModifier[0]);
            }

#pragma warning disable SA1313 // Parameter names must begin with lower-case letter
            private static bool Prefix(uint citizenID, ref Citizen data, ref bool __result)
            {
                if (RealTimeAI != null)
                {
                    __result = RealTimeAI.CanMakeBabies(citizenID, ref data);
                    return false;
                }

                return true;
            }
#pragma warning restore SA1313 // Parameter names must begin with lower-case letter
        }
    }
}