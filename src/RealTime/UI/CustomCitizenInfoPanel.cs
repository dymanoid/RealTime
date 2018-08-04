// <copyright file="CustomCitizenInfoPanel.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.UI
{
    using RealTime.CustomAI;
    using SkyTools.Localization;

    /// <summary>
    /// A customized citizen info panel that additionally shows the origin building of the citizen.
    /// </summary>
    internal sealed class CustomCitizenInfoPanel : RealTimeInfoPanelBase<HumanWorldInfoPanel>
    {
        private const string GameInfoPanelName = "(Library) CitizenWorldInfoPanel";

        private CustomCitizenInfoPanel(string panelName, RealTimeResidentAI<ResidentAI, Citizen> residentAI, ILocalizationProvider localizationProvider)
            : base(panelName, residentAI, localizationProvider)
        {
        }

        /// <summary>Enables the citizen info panel customization. Can return null on failure.</summary>
        /// <param name="residentAI">The custom resident AI.</param>
        /// <param name="localizationProvider">The localization provider to use for text translation.</param>
        /// <returns>An instance of the <see cref="CustomCitizenInfoPanel"/> object that can be used for disabling
        /// the customization, or null when the customization fails.</returns>
        public static CustomCitizenInfoPanel Enable(RealTimeResidentAI<ResidentAI, Citizen> residentAI, ILocalizationProvider localizationProvider)
        {
            var result = new CustomCitizenInfoPanel(GameInfoPanelName, residentAI, localizationProvider);
            return result.Initialize() ? result : null;
        }

        /// <summary>Updates the origin building display.</summary>
        /// <param name="instance">The game object instance to get the information from.</param>
        public override void UpdateCustomInfo(ref InstanceID instance)
        {
            if (instance.Type != InstanceType.Citizen)
            {
                UpdateCitizenInfo(0);
            }
            else
            {
                UpdateCitizenInfo(instance.Citizen);
            }
        }
    }
}
