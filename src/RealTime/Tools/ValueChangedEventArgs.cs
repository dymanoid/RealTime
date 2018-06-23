// <copyright file="ValueChangedEventArgs.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.Tools
{
    using System;

    internal sealed class ValueChangedEventArgs<T> : EventArgs
    {
        public ValueChangedEventArgs(T oldValue, T newValue)
        {
            NewValue = newValue;
        }

        public T OldValue { get; }

        public T NewValue { get; }
    }
}
