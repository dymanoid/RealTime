// <copyright file="UnityPageViewItem.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.UI
{
    using System;
    using ColossalFramework.UI;
    using ICities;
    using RealTime.Localization;

    internal sealed class UnityPageViewItem : IContainerViewItem
    {
        private const string LabelName = "Label";
        private readonly UIHelperBase page;
        private readonly string id;

        public UnityPageViewItem(UIHelperBase page, string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentException("The page ID cannot be null or empty string", nameof(id));
            }

            this.page = page ?? throw new ArgumentNullException(nameof(page));
            this.id = id;
        }

        public UIHelperBase Container => page;

        public void Translate(LocalizationProvider localizationProvider)
        {
            var content = page as UIHelper;
            if (content == null)
            {
                return;
            }

            UIComponent panel = ((UIComponent)content.self).parent;
            if (panel == null)
            {
                return;
            }

            UILabel label = panel.Find<UILabel>(LabelName);
            if (label != null)
            {
                label.text = localizationProvider.Translate(id);
            }
        }
    }
}
