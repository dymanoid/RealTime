// <copyright file="TitleBar.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.UI
{
    using ColossalFramework.UI;
    using UnityEngine;

    /// <summary>
    /// An UI component representing a simple title bar with a close button.
    /// </summary>
    /// <seealso cref="UIPanel" />
    internal sealed class TitleBar : UIPanel
    {
        private const string CloseButtonSprite = "buttonclose";
        private const string CloseButtonHoverSprite = "buttonclosehover";
        private const string CloseButtonPressedSprite = "buttonclosepressed";

        private string caption;
        private UILabel captionLabel;
        private UIButton closeButton;

        /// <summary>Gets or sets the title bar caption.</summary>
        public string Caption
        {
            get
            {
                return captionLabel?.text ?? caption ?? string.Empty;
            }

            set
            {
                caption = value;
                if (captionLabel != null)
                {
                    captionLabel.text = value;
                }
            }
        }

        /// <summary>Called by Unity engine to start this instance.</summary>
        public override void Start()
        {
            base.Start();

            height = 50;
            width = parent.width;

            captionLabel = AddUIComponent<UILabel>();
            closeButton = AddUIComponent<UIButton>();

            captionLabel.autoSize = true;
            captionLabel.padding = new RectOffset(10, 10, 15, 15);
            captionLabel.relativePosition = Vector3.zero;
            captionLabel.text = caption;

            closeButton.eventClick += CloseButtonClick;
            closeButton.relativePosition = new Vector3(width - closeButton.width - (captionLabel.padding.horizontal * 2), 5);
            closeButton.normalBgSprite = CloseButtonSprite;
            closeButton.hoveredBgSprite = CloseButtonHoverSprite;
            closeButton.pressedBgSprite = CloseButtonPressedSprite;
        }

        private void CloseButtonClick(UIComponent component, UIMouseEventParameter eventParam)
        {
            closeButton.eventClick -= CloseButtonClick;
            parent.Hide();
        }
    }
}
