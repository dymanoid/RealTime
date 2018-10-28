// <copyright file="Statistics.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.Simulation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using ColossalFramework.Globalization;
    using ColossalFramework.UI;
    using RealTime.Localization;
    using SkyTools.Localization;
    using SkyTools.Tools;
    using UnityEngine;

    /// <summary>
    /// Handles the customization of the game's statistics numbers.
    /// </summary>
    internal sealed class Statistics
    {
        private const int VanillaFramesPerWeek = 3840;
        private const string UnitPlaceholder = "{unit}";
        private const string OverrddenTranslationType = "Units";
        private const string CityInfoPanelName = "(Library) CityInfoPanel";
        private const string DistrictInfoPanelName = "(Library) DistrictWorldInfoPanel";
        private const string IndustryInfoPanelName = "(Library) IndustryWorldInfoPanel";
        private const string UniqueFactoryInfoPanelName = "(Library) UniqueFactoryWorldInfoPanel";
        private const string TouristsPanelName = "Tourists";
        private const string TouristsLabelName = "Label";
        private const string InfoPanelName = "InfoPanel";
        private const string CitizensChangeLabelName = "ProjectedChange";
        private const string IncomeLabelName = "ProjectedIncome";
        private const string BuildingsButtonsContainer = "TSContainer";

        private readonly ITimeInfo timeInfo;
        private readonly ILocalizationProvider localizationProvider;
        private readonly Locale customLocale;

        private Locale mainLocale;
        private UILabel cityInfoPanelTourists;
        private UILabel districtInfoPanelTourists;
        private UILabel labelPopulation;
        private UILabel labelIncome;
        private UITabContainer buildingsTabContainer;
        private WorldInfoPanel industryInfoPanel;
        private WorldInfoPanel uniqueFactoryInfoPanel;

        /// <summary>Initializes a new instance of the <see cref="Statistics"/> class.</summary>
        /// <param name="timeInfo">An object that provides the game's time information.</param>
        /// <param name="localizationProvider">The object to get the current locale from.</param>
        /// <exception cref="ArgumentNullException">Thrown when any argument is null.</exception>
        public Statistics(ITimeInfo timeInfo, ILocalizationProvider localizationProvider)
        {
            this.timeInfo = timeInfo ?? throw new ArgumentNullException(nameof(timeInfo));
            this.localizationProvider = localizationProvider ?? throw new ArgumentNullException(nameof(localizationProvider));

            customLocale = new Locale();
        }

        /// <summary>Initializes this instance by preparing connections to the necessary game parts.</summary>
        /// <returns><c>true</c> on success; otherwise, <c>false</c>.</returns>
        public bool Initialize()
        {
            if (mainLocale != null)
            {
                return true;
            }

            try
            {
                FieldInfo field = typeof(LocaleManager).GetField("m_Locale", BindingFlags.Instance | BindingFlags.NonPublic);
                mainLocale = field.GetValue(LocaleManager.instance) as Locale;
            }
            catch (Exception ex)
            {
                Log.Warning("The 'Real Time' mod could not obtain the locale field of the LocaleManager, error message: " + ex);
                return false;
            }

            cityInfoPanelTourists = GameObject
                .Find(CityInfoPanelName)?
                .GetComponent<CityInfoPanel>()?
                .Find<UIPanel>(TouristsPanelName)?
                .Find<UILabel>(TouristsLabelName);

            if (cityInfoPanelTourists == null)
            {
                Log.Warning("The 'Real Time' mod could not obtain the CityInfoPanel.Tourists.Label object");
            }

            districtInfoPanelTourists = GameObject
                .Find(DistrictInfoPanelName)?
                .GetComponent<DistrictWorldInfoPanel>()?
                .Find<UIPanel>(TouristsPanelName)?
                .Find<UILabel>(TouristsLabelName);

            if (districtInfoPanelTourists == null)
            {
                Log.Warning("The 'Real Time' mod could not obtain the DistrictWorldInfoPanel.Tourists.Label object");
            }

            industryInfoPanel = GameObject
                .Find(IndustryInfoPanelName)?
                .GetComponent<IndustryWorldInfoPanel>();

            if (industryInfoPanel == null)
            {
                Log.Warning("The 'Real Time' mod could not obtain the IndustryWorldInfoPanel object");
            }

            uniqueFactoryInfoPanel = GameObject
                .Find(UniqueFactoryInfoPanelName)?
                .GetComponent<UniqueFactoryWorldInfoPanel>();

            if (uniqueFactoryInfoPanel == null)
            {
                Log.Warning("The 'Real Time' mod could not obtain the UniqueFactoryWorldInfoPanel object");
            }

            UIPanel infoPanel = UIView.Find<UIPanel>(InfoPanelName);
            if (infoPanel == null)
            {
                Log.Warning("The 'Real Time' mod could not obtain the InfoPanel object");
            }
            else
            {
                labelPopulation = infoPanel.Find<UILabel>(CitizensChangeLabelName);
                if (labelPopulation == null)
                {
                    Log.Warning("The 'Real Time' mod could not obtain the ProjectedChange object");
                }

                labelIncome = infoPanel.Find<UILabel>(IncomeLabelName);
                if (labelIncome == null)
                {
                    Log.Warning("The 'Real Time' mod could not obtain the ProjectedIncome object");
                }
            }

            buildingsTabContainer = UIView.Find<UITabContainer>(BuildingsButtonsContainer);
            if (buildingsTabContainer == null)
            {
                Log.Warning("The 'Real Time' mod could not obtain the TSContainer object");
            }

            LocaleManager.eventLocaleChanged += LocaleChanged;
            return true;
        }

        /// <summary>Shuts down this instance.</summary>
        public void Close()
        {
            LocaleManager.eventLocaleChanged -= LocaleChanged;
            mainLocale = null;
            cityInfoPanelTourists = null;
            districtInfoPanelTourists = null;
            labelIncome = null;
            labelPopulation = null;
            buildingsTabContainer = null;
            industryInfoPanel = null;
            uniqueFactoryInfoPanel = null;
        }

        /// <summary>Refreshes the statistics units for current game speed.</summary>
        public void RefreshUnits()
        {
            if (mainLocale == null)
            {
                return;
            }

            var unit = TimeSpan.FromHours(VanillaFramesPerWeek * timeInfo.HoursPerFrame);

            double minutes = Math.Round(unit.TotalMinutes);
            if (minutes >= 30d)
            {
                minutes = Math.Round(minutes / 10d) * 10d;
            }
            else if (minutes >= 10d)
            {
                minutes = Math.Round(minutes / 5d) * 5d;
            }

            string displayUnit = $"{minutes:F0} {localizationProvider.Translate(TranslationKeys.Minutes)}";
            if (RefreshUnits(displayUnit))
            {
                RefreshUI();
            }
        }

        private static void RefreshToolTips(Component panel)
        {
            if (panel == null)
            {
                return;
            }

            IEnumerable<UIComponent> components = panel
                .GetComponentsInChildren<UIComponent>()?
                .Where(c => !string.IsNullOrEmpty(c.tooltipLocaleID));

            if (components == null)
            {
                return;
            }

            foreach (UIComponent component in components.Where(c => c is UISprite || c is UITextComponent))
            {
                component.tooltip = Locale.Get(component.tooltipLocaleID);
            }
        }

        private bool RefreshUnits(string displayUnit)
        {
            customLocale.Reset();

            IDictionary<string, string> overridden = localizationProvider.GetOverriddenTranslations(OverrddenTranslationType);
            if (overridden == null || overridden.Count == 0)
            {
                return false;
            }

            foreach (KeyValuePair<string, string> value in overridden)
            {
                string translated = value.Value.Replace(UnitPlaceholder, displayUnit);
                customLocale.AddLocalizedString(new Locale.Key { m_Identifier = value.Key }, translated);
            }

            mainLocale.Merge(null, customLocale);
            return true;
        }

        private void RefreshUI()
        {
            if (labelIncome != null)
            {
                labelIncome.tooltip = Locale.Get(labelIncome.tooltipLocaleID);
            }

            if (labelPopulation != null)
            {
                labelPopulation.tooltip = Locale.Get(labelPopulation.tooltipLocaleID);
            }

            if (cityInfoPanelTourists != null)
            {
                cityInfoPanelTourists.text = Locale.Get(cityInfoPanelTourists.localeID);
            }

            if (districtInfoPanelTourists != null)
            {
                districtInfoPanelTourists.text = Locale.Get(districtInfoPanelTourists.localeID);
            }

            RefreshToolTips(ToolsModifierControl.economyPanel);
            RefreshToolTips(industryInfoPanel);
            RefreshToolTips(uniqueFactoryInfoPanel);
            RefreshBuildingsButtons();
        }

        private void RefreshBuildingsButtons()
        {
            if (buildingsTabContainer == null)
            {
                return;
            }

            // This creates objects on heap, but it won't cause memory pressure because it's not called
            // in the simulation loop
            var items = buildingsTabContainer.GetComponentsInChildren<UIButton>()?
                .Select(b => new { Info = b.objectUserData as PrefabInfo, Button = b })
                .Where(i => i.Info is BuildingInfo || i.Info is NetInfo);

            if (items == null)
            {
                return;
            }

            foreach (var item in items)
            {
                item.Button.tooltip = item.Info.GetLocalizedTooltip();
            }
        }

        private void LocaleChanged()
        {
            RefreshUnits();
        }
    }
}
