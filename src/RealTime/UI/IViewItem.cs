// <copyright file="IViewItem.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.UI
{
    using RealTime.Localization;

    internal interface IViewItem
    {
        void Translate(LocalizationProvider localizationProvider);
    }
}
