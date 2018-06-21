// <copyright file="ConfigItemSliderAttribute.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.UI
{
    using System;

    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    internal sealed class ConfigItemSliderAttribute : ConfigItemUIBaseAttribute
    {
        public ConfigItemSliderAttribute(float min, float max, float step)
        {
            if (max <= min)
            {
                throw new ArgumentException("The maximum value must be greater than the minimum value");
            }

            if (step == 0)
            {
                throw new ArgumentException("The step value must be greater than 0");
            }

            Min = min;
            Max = max;
            Step = step;
        }

        public ConfigItemSliderAttribute(float min, float max)
            : this(min, max, 1f)
        {
        }

        public float Min { get; }

        public float Max { get; }

        public float Step { get; }
    }
}
