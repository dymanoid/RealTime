// <copyright file="IContainerViewItem.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.UI
{
    using ICities;

    internal interface IContainerViewItem : IViewItem
    {
        UIHelperBase Container { get; }
    }
}
