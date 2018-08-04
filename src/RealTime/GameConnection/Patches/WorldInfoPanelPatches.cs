// <copyright file="WorldInfoPanelPatches.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.GameConnection.Patches
{
    using System;
    using System.Reflection;
    using RealTime.UI;
    using SkyTools.Patching;

    /// <summary>
    /// A static class that provides the patch objects for the world info panel game methods.
    /// </summary>
    internal static class WorldInfoPanelPatches
    {
        /// <summary>Gets or sets the customized citizen information panel.</summary>
        public static CustomCitizenInfoPanel CitizenInfoPanel { get; set; }

        /// <summary>Gets or sets the customized vehicle information panel.</summary>
        public static CustomVehicleInfoPanel VehicleInfoPanel { get; set; }

        /// <summary>Gets the patch for the update bindings method.</summary>
        public static IPatch UpdateBindings { get; } = new WorldInfoPanel_UpdateBindings();

        private sealed class WorldInfoPanel_UpdateBindings : PatchBase
        {
            protected override MethodInfo GetMethod()
            {
                return typeof(WorldInfoPanel).GetMethod(
                    "UpdateBindings",
                    BindingFlags.Instance | BindingFlags.NonPublic,
                    null,
                    new Type[0],
                    new ParameterModifier[0]);
            }

#pragma warning disable SA1313 // Parameter names must begin with lower-case letter
            private static void Postfix(WorldInfoPanel __instance, ref InstanceID ___m_InstanceID)
            {
                switch (__instance)
                {
                    case CitizenWorldInfoPanel _:
                        CitizenInfoPanel?.UpdateCustomInfo(ref ___m_InstanceID);
                        break;

                    case VehicleWorldInfoPanel _:
                        VehicleInfoPanel?.UpdateCustomInfo(ref ___m_InstanceID);
                        break;
                }
            }
#pragma warning restore SA1313 // Parameter names must begin with lower-case letter
        }
    }
}
