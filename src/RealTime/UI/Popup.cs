// <copyright file="Popup.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.UI
{
    using ColossalFramework.UI;
    using UnityEngine;

    /// <summary>
    /// A pop-up that can be shown to display some information without a modal dialog.
    /// </summary>
    /// <seealso cref="UIPanel" />
    internal sealed class Popup : UIPanel
    {
        private const string PopupSpriteName = "InfoBubble";

        private TitleBar titleBar;
        private UILabel infoLabel;

        private string caption;
        private string text;
        private Vector2 popupPosition;

        public static void Show(UIComponent parent, string caption, string text)
        {
            if (parent == null)
            {
                throw new System.ArgumentNullException(nameof(parent));
            }

            Popup popup = parent.AddUIComponent<Popup>();
            popup.popupPosition = new Vector2(parent.relativePosition.x + (parent.width / 2), parent.relativePosition.y);
            popup.caption = caption ?? string.Empty;
            popup.text = text ?? string.Empty;
            popup.eventVisibilityChanged += popup.PopupVisibilityChanged;
        }

        public override void Start()
        {
            base.Start();
            eventSizeChanged += PopupSizeChanged;

            titleBar = AddUIComponent<TitleBar>();
            infoLabel = AddUIComponent<UILabel>();

            width = 500;
            autoFitChildrenVertically = true;
            autoLayout = true;
            autoLayoutDirection = LayoutDirection.Vertical;
            backgroundSprite = PopupSpriteName;

            titleBar.Caption = caption;
            titleBar.width = width;

            infoLabel.width = width;
            infoLabel.padding = new RectOffset(15, 15, 5, 30);
            infoLabel.wordWrap = true;
            infoLabel.autoHeight = true;
            infoLabel.textScale = 0.8f;
            infoLabel.text = text;
        }

        private void PopupSizeChanged(UIComponent component, Vector2 value)
        {
            relativePosition = new Vector3(popupPosition.x, popupPosition.y - height);
        }

        private void PopupVisibilityChanged(UIComponent component, bool value)
        {
            if (value)
            {
                return;
            }

            eventVisibilityChanged -= PopupVisibilityChanged;
            parent.RemoveUIComponent(this);
            Destroy(this);
        }
    }
}
