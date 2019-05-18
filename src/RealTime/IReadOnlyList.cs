// <copyright file="IReadOnlyList.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime
{
    /// <summary>
    /// A simple interface for a read-only list containing some items.
    /// This is the very simplified version of the built-in interface provided by the .NET BCL.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the list.</typeparam>
    internal interface IReadOnlyList<T>
    {
        /// <summary>
        /// Gets the number of the elements contained in the list.
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Gets the item from the list at the specified <paramref name="index"/>.
        /// </summary>
        /// <param name="index">The index of the element to get.</param>
        ///
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown when the <paramref name="index"/> is negative
        /// or is greater or equal to <see cref="Count"/>.</exception>
        ///
        /// <returns>The element at the specified <paramref name="index"/>.</returns>
        T this[int index] { get; }
    }
}