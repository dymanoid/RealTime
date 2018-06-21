// <copyright file="UnityViewItemFactory.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.UI
{
    using System;
    using System.Reflection;
    using ICities;
    using RealTime.Localization;

    internal sealed class UnityViewItemFactory : IViewItemFactory
    {
        private readonly UIHelperBase uiHelper;

        public UnityViewItemFactory(UIHelperBase uiHelper)
        {
            this.uiHelper = uiHelper ?? throw new ArgumentNullException(nameof(uiHelper));
        }

        public IContainerViewItem CreateGroup(string id)
        {
            return new UnityPageViewItem(uiHelper.AddGroup(Constants.Placeholder), id);
        }

        public IViewItem CreateCheckBox(IContainerViewItem container, string id, PropertyInfo property, object config)
        {
            return new UnityCheckBoxItem(container.Container, id, property, config);
        }

        public IViewItem CreateSlider(IContainerViewItem container, string id, PropertyInfo property, object config, float min, float max, float step)
        {
            return new UnitySliderItem(container.Container, id, property, config, min, max, step);
        }
    }
}
