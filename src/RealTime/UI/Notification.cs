// <copyright file="Notification.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.UI
{
    using System;
    using ColossalFramework.UI;
    using SkyTools.Tools;
    using SkyTools.UI;

    /// <summary>
    /// Provides notification tools like a pop-up or a message box.
    /// </summary>
    internal static class Notification
    {
        private const string UIInfoPanel = "InfoPanel";
        private const string UIPanelTime = "PanelTime";

        /// <summary>Notifies the user either with a pop up or with a message box.</summary>
        /// <param name="caption">The caption of the pop up or message box.</param>
        /// <param name="text">The notification text.</param>
        /// <exception cref="ArgumentException">Thrown when any argument is null or an empty string.</exception>
        public static void Notify(string caption, string text)
        {
            if (string.IsNullOrEmpty(caption))
            {
                throw new ArgumentException("The caption cannot be null or an empty string", nameof(caption));
            }

            if (string.IsNullOrEmpty(text))
            {
                throw new ArgumentException("The text cannot be null or an empty string.", nameof(text));
            }

            if (!NotifyWithPopup(caption, text))
            {
                NotifyWithDialog(caption, text);
            }
        }

        private static void NotifyWithDialog(string caption, string text)
        {
            MessageBox.Show(caption, text);
        }

        private static bool NotifyWithPopup(string caption, string text)
        {
            UIPanel infoPanel = UIView.Find<UIPanel>(UIInfoPanel);
            if (infoPanel == null)
            {
                Log.Warning("No UIPanel found: " + UIInfoPanel);
                return false;
            }

            UIPanel panelTime = infoPanel.Find<UIPanel>(UIPanelTime);
            if (panelTime == null)
            {
                Log.Warning("No UIPanel found: " + UIPanelTime);
                return false;
            }

            Popup.Show(panelTime, caption, text);
            return true;
        }
    }
}
