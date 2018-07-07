// <copyright file="MessageBox.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.UI
{
    using ColossalFramework.UI;

    /// <summary>
    /// Displays a message box that can contain text that informs and instructs the user.
    /// </summary>
    internal static class MessageBox
    {
        private const string DialogPanelName = "ExceptionPanel";

        /// <summary>Displays a message box with specified text and caption.</summary>
        /// <param name="caption">The caption of the message to display.</param>
        /// <param name="message">The message text.</param>
        public static void Show(string caption, string message)
        {
            UIView.library.ShowModal<ExceptionPanel>(DialogPanelName).SetMessage(caption ?? string.Empty, message ?? string.Empty, false);
        }
    }
}
