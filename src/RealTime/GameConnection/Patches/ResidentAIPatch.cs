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

        /// <summary>Gets the patch object for the start moving method.</summary>
        public static IPatch StartMoving { get; } = new ResidentAI_StartMoving();

        /// <summary>Gets the patch object for the simulation step method (for citizens instances).</summary>
        public static IPatch InstanceSimulationStep { get; } = new ResidentAI_SimulationStep();

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
                var doRandomMove
                    = FastDelegateFactory.Create<DoRandomMoveDelegate>(typeof(ResidentAI), "DoRandomMove", instanceMethod: true);

                var findEvacuationPlace
                    = FastDelegateFactory.Create<FindEvacuationPlaceDelegate>(typeof(ResidentAI), "FindEvacuationPlace", instanceMethod: true);

                var findHospital
                    = FastDelegateFactory.Create<FindHospitalDelegate>(typeof(ResidentAI), "FindHospital", instanceMethod: true);

                var findVisitPlace
                    = FastDelegateFactory.Create<FindVisitPlaceDelegate>(typeof(ResidentAI), "FindVisitPlace", instanceMethod: true);

                var getEntertainmentReason
                    = FastDelegateFactory.Create<GetEntertainmentReasonDelegate>(typeof(ResidentAI), "GetEntertainmentReason", instanceMethod: true);

                var getEvacuationReason
                    = FastDelegateFactory.Create<GetEvacuationReasonDelegate>(typeof(ResidentAI), "GetEvacuationReason", instanceMethod: true);

                var getShoppingReason
                    = FastDelegateFactory.Create<GetShoppingReasonDelegate>(typeof(ResidentAI), "GetShoppingReason", instanceMethod: true);

                var startMoving
                    = FastDelegateFactory.Create<StartMovingDelegate>(typeof(ResidentAI), "StartMoving", instanceMethod: true);

                var startMovingWithOffer
                    = FastDelegateFactory.Create<StartMovingWithOfferDelegate>(typeof(ResidentAI), "StartMoving", instanceMethod: true);

                var attemptAutodidact
                    = FastDelegateFactory.Create<AttemptAutodidactDelegate>(typeof(ResidentAI), "AttemptAutodidact", instanceMethod: true);

                return new ResidentAIConnection<ResidentAI, Citizen>(
                    doRandomMove,
                    findEvacuationPlace,
                    findHospital,
                    findVisitPlace,
                    getEntertainmentReason,
                    getEvacuationReason,
                    getShoppingReason,
                    startMoving,
                    startMovingWithOffer,
                    attemptAutodidact);
            }
            catch (Exception e)
            {
                Log.Error("The 'Real Time' mod failed to create a delegate for type 'ResidentAI', no method patching for the class: " + e);
                return null;
            }
        }

        private sealed class ResidentAI_UpdateLocation : PatchBase
        {
            protected override MethodInfo GetMethod() =>
                typeof(ResidentAI).GetMethod(
                    "UpdateLocation",
                    BindingFlags.Instance | BindingFlags.NonPublic,
                    null,
                    new[] { typeof(uint), typeof(Citizen).MakeByRefType() },
                    new ParameterModifier[0]);

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Redundancy", "RCS1213", Justification = "Harmony patch")]
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming Rules", "SA1313", Justification = "Harmony patch")]
            private static bool Prefix(ResidentAI __instance, uint citizenID, ref Citizen data)
            {
                RealTimeAI.UpdateLocation(__instance, citizenID, ref data);
                return false;
            }
        }

        private sealed class HumanAI_ArriveAtTarget : PatchBase
        {
            protected override MethodInfo GetMethod() =>
                typeof(HumanAI).GetMethod(
                    "ArriveAtTarget",
                    BindingFlags.Instance | BindingFlags.NonPublic,
                    null,
                    new[] { typeof(ushort), typeof(CitizenInstance).MakeByRefType() },
                    new ParameterModifier[0]);

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Redundancy", "RCS1213", Justification = "Harmony patch")]
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming Rules", "SA1313", Justification = "Harmony patch")]
            private static void Postfix(ref CitizenInstance citizenData, bool __result, HumanAI __instance)
            {
                if (__result && citizenData.m_citizen != 0 && __instance is ResidentAI)
                {
                    RealTimeAI.RegisterCitizenArrival(citizenData.m_citizen);
                }
            }
        }

        private sealed class ResidentAI_UpdateAge : PatchBase
        {
            protected override MethodInfo GetMethod() =>
                typeof(ResidentAI).GetMethod(
                    "UpdateAge",
                    BindingFlags.Instance | BindingFlags.NonPublic,
                    null,
                    new[] { typeof(uint), typeof(Citizen).MakeByRefType() },
                    new ParameterModifier[0]);

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Redundancy", "RCS1213", Justification = "Harmony patch")]
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming Rules", "SA1313", Justification = "Harmony patch")]
            private static bool Prefix(ref bool __result)
            {
                if (RealTimeAI.CanCitizensGrowUp)
                {
                    return true;
                }

                __result = false;
                return false;
            }
        }

        private sealed class ResidentAI_CanMakeBabies : PatchBase
        {
            protected override MethodInfo GetMethod() =>
                typeof(ResidentAI).GetMethod(
                    "CanMakeBabies",
                    BindingFlags.Instance | BindingFlags.Public,
                    null,
                    new[] { typeof(uint), typeof(Citizen).MakeByRefType() },
                    new ParameterModifier[0]);

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Redundancy", "RCS1213", Justification = "Harmony patch")]
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming Rules", "SA1313", Justification = "Harmony patch")]
            private static bool Prefix(uint citizenID, ref Citizen data, ref bool __result)
            {
                __result = RealTimeAI.CanMakeBabies(citizenID, ref data);
                return false;
            }
        }

        private sealed class ResidentAI_StartMoving : PatchBase
        {
            protected override MethodInfo GetMethod() =>
                typeof(HumanAI).GetMethod(
                    "StartMoving",
                    BindingFlags.Instance | BindingFlags.Public,
                    null,
                    new[] { typeof(uint), typeof(Citizen).MakeByRefType(), typeof(ushort), typeof(ushort) },
                    new ParameterModifier[0]);

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Redundancy", "RCS1213", Justification = "Harmony patch")]
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming Rules", "SA1313", Justification = "Harmony patch")]
            private static void Postfix(uint citizenID, bool __result, HumanAI __instance)
            {
                if (__result && citizenID != 0 && __instance is ResidentAI)
                {
                    RealTimeAI.RegisterCitizenDeparture(citizenID);
                }
            }
        }

        private sealed class ResidentAI_SimulationStep : PatchBase
        {
            protected override MethodInfo GetMethod() =>
                typeof(ResidentAI).GetMethod(
                    "SimulationStep",
                    BindingFlags.Instance | BindingFlags.Public,
                    null,
                    new[] { typeof(ushort), typeof(CitizenInstance).MakeByRefType(), typeof(CitizenInstance.Frame).MakeByRefType(), typeof(bool) },
                    new ParameterModifier[0]);

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Redundancy", "RCS1213", Justification = "Harmony patch")]
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming Rules", "SA1313", Justification = "Harmony patch")]
            private static void Postfix(ushort instanceID, ref CitizenInstance citizenData, ResidentAI __instance)
            {
                if (instanceID == 0)
                {
                    return;
                }

                if ((citizenData.m_flags & (CitizenInstance.Flags.WaitingTaxi | CitizenInstance.Flags.WaitingTransport)) != 0)
                {
                    RealTimeAI.ProcessWaitingForTransport(__instance, citizenData.m_citizen, instanceID);
                }
            }
        }
    }
}
