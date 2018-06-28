// <copyright file="ConfigItemSliderAttribute.cs" company="dymanoid">
//     Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.UI
{
    using System;

    /// <summary>
    /// An attribute specifying that the configuration item has to be presented as a slider.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    internal sealed class ConfigItemSliderAttribute : ConfigItemUIBaseAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigItemSliderAttribute"/> class.
        /// </summary>
        /// <param name="min">The minimum slider value.</param>
        /// <param name="max">The maximum slider value.</param>
        /// <param name="step">The slider step value. Default is 1.</param>
        /// <param name="valueType">The type of the value to display. Default is <see cref="SliderValueType.Percentage"/>.</param>
        /// <exception cref="ArgumentException">
        /// Thrown when the <paramref name="max"/> value is less or equal to the
        /// <paramref name="min"/> value.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the <paramref name="step"/> value is less or equal to zero.
        /// </exception>
        public ConfigItemSliderAttribute(float min, float max, float step = 1f, SliderValueType valueType = SliderValueType.Percentage)
        {
            if (max <= min)
            {
                throw new ArgumentException("The maximum value must be greater than the minimum value");
            }

            if (step <= 0)
            {
                throw new ArgumentException("The step value must be greater than 0");
            }

            Min = min;
            Max = max;
            Step = step;
            ValueType = valueType;
        }

        /// <summary>Gets the slider minimum value.</summary>
        public float Min { get; }

        /// <summary>Gets the slider maximum value.</summary>
        public float Max { get; }

        /// <summary>Gets the slider step value.</summary>
        public float Step { get; }

        /// <summary>Gets the type of the value that will be displayed.</summary>
        public SliderValueType ValueType { get; }
    }
}