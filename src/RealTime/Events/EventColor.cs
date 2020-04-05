// <copyright file="EventColor.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.Events
{
    using System;

    /// <summary>
    /// A struct representing the event color.
    /// </summary>
    internal struct EventColor : IEquatable<EventColor>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EventColor"/> struct.
        /// </summary>
        /// <param name="red">The red component of the color.</param>
        /// <param name="green">The green component of the color.</param>
        /// <param name="blue">The blue component of the color.</param>
        public EventColor(byte red, byte green, byte blue)
        {
            Red = red;
            Green = green;
            Blue = blue;
        }

        /// <summary>
        /// Gets the red component of the color.
        /// </summary>
        public byte Red { get; }

        /// <summary>
        /// Gets the green component of the color.
        /// </summary>
        public byte Green { get; }

        /// <summary>
        /// Gets the blue component of the color.
        /// </summary>
        public byte Blue { get; }

        public static bool operator ==(EventColor left, EventColor right) => left.Equals(right);

        public static bool operator !=(EventColor left, EventColor right) => !(left == right);

        /// <inheritdoc/>
        public override bool Equals(object obj) => obj is EventColor color && Equals(color);

        /// <inheritdoc/>
        public bool Equals(EventColor other) => Red == other.Red && Green == other.Green && Blue == other.Blue;

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            int hashCode = -1058441243;
            hashCode = hashCode * -1521134295 + Red.GetHashCode();
            hashCode = hashCode * -1521134295 + Green.GetHashCode();
            hashCode = hashCode * -1521134295 + Blue.GetHashCode();
            return hashCode;
        }
    }
}
