// <copyright file="IViewItemFactory.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.UI
{
    using System.Reflection;

    internal interface IViewItemFactory
    {
        IContainerViewItem CreateGroup(string id);

        IViewItem CreateCheckBox(IContainerViewItem container, string id, PropertyInfo property, object config);

        IViewItem CreateSlider(
            IContainerViewItem container,
            string id,
            PropertyInfo property,
            object config,
            float min,
            float max,
            float step,
            SliderValueType valueType);
    }
}
