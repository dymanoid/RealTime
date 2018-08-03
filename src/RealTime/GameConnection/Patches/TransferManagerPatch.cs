// <copyright file="TransferManagerPatch.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.GameConnection.Patches
{
    using System.Reflection;
    using RealTime.CustomAI;
    using SkyTools.Patching;

    /// <summary>
    /// A static class that provides the patch objects for the game's transfer manager.
    /// </summary>
    internal static class TransferManagerPatch
    {
        /// <summary>Gets the patch object for the outgoing offer method.</summary>
        public static IPatch AddOutgoingOffer { get; } = new TransferManager_AddOutgoingOffer();

        /// <summary>Gets or sets the custom AI object for buildings.</summary>
        public static RealTimeBuildingAI RealTimeAI { get; set; }

        private sealed class TransferManager_AddOutgoingOffer : PatchBase
        {
            protected override MethodInfo GetMethod()
            {
                return typeof(TransferManager).GetMethod(
                    "AddOutgoingOffer",
                    BindingFlags.Instance | BindingFlags.Public,
                    null,
                    new[] { typeof(TransferManager.TransferReason), typeof(TransferManager.TransferOffer) },
                    new ParameterModifier[0]);
            }

            private static bool Prefix(TransferManager.TransferReason material, ref TransferManager.TransferOffer offer)
            {
                switch (material)
                {
                    case TransferManager.TransferReason.Entertainment:
                    case TransferManager.TransferReason.EntertainmentB:
                    case TransferManager.TransferReason.EntertainmentC:
                    case TransferManager.TransferReason.EntertainmentD:
                    case TransferManager.TransferReason.TouristA:
                    case TransferManager.TransferReason.TouristB:
                    case TransferManager.TransferReason.TouristC:
                    case TransferManager.TransferReason.TouristD:
                        return RealTimeAI?.IsEntertainmentTarget(offer.Building) ?? true;

                    default:
                        return true;
                }
            }
        }
    }
}
