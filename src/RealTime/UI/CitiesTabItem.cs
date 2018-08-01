// <copyright file="CitiesTabItem.cs" company="dymanoid">Copyright (c) dymanoid. All rights reserved.</copyright>

namespace RealTime.UI
{
    using System;
    using ColossalFramework.UI;
    using RealTime.Localization;

    /// <summary>A tab item container.</summary>
    /// <seealso cref="CitiesContainerItemBase"/>
    internal sealed class CitiesTabItem : CitiesContainerItemBase
    {
        private readonly UIButton tabButton;

        private CitiesTabItem(UIButton tabButton, UIHelper tabContainer, string id)
            : base(tabContainer, id)
        {
            this.tabButton = tabButton;
        }

        /// <summary>Creates and sets up an instance of the <see cref="CitiesTabItem"/> class.</summary>
        /// <param name="tabStrip">The tab strip to use as parent for the new tab item.</param>
        /// <param name="id">The unique ID of the new tab item.</param>
        /// <returns>An instance of <see cref="CitiesTabItem"/> or null on failure.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="tabStrip"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="id"/> is null or an empty string.</exception>
        public static CitiesTabItem Create(UITabstrip tabStrip, string id)
        {
            if (tabStrip == null)
            {
                throw new ArgumentNullException(nameof(tabStrip));
            }

            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentException("The tab item's id cannot be null or an empty string", nameof(id));
            }

            UIButton tabButton = tabStrip.AddTab();
            if (tabButton == null)
            {
                return null;
            }

            tabButton.autoSize = true;
            tabButton.textPadding.left = 10;
            tabButton.textPadding.right = 10;
            tabButton.textPadding.top = 10;
            tabButton.textPadding.bottom = 6;
            tabButton.wordWrap = false;
            tabButton.normalBgSprite = "SubBarButtonBase";
            tabButton.disabledBgSprite = "SubBarButtonBaseDisabled";
            tabButton.focusedBgSprite = "SubBarButtonBaseFocused";
            tabButton.hoveredBgSprite = "SubBarButtonBaseHovered";
            tabButton.pressedBgSprite = "SubBarButtonBasePressed";

            var tabContainer = tabStrip.tabContainer.components[tabStrip.tabCount - 1] as UIPanel;
            if (tabContainer == null)
            {
                return null;
            }

            tabContainer.padding.top = 30;
            tabContainer.autoLayout = true;
            tabContainer.autoLayoutDirection = LayoutDirection.Vertical;
            tabContainer.autoLayoutPadding.bottom = 20;
            tabContainer.autoLayoutPadding.left = 10;
            tabContainer.autoLayoutPadding.right = 10;
            return new CitiesTabItem(tabButton, new UIHelper(tabContainer), id);
        }

        /// <summary>Performs the actual view item translation.</summary>
        /// <param name="localizationProvider">The localization provider to use for translation. Guaranteed to be not null.</param>
        protected override void TranslateImpl(ILocalizationProvider localizationProvider)
        {
            tabButton.text = localizationProvider.Translate(Id);
            tabButton.tooltip = tabButton.text;
        }
    }
}