// <copyright file="ReadOnlyList.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// A wrapper for a <see cref="List{T}"/> that offers read-only access to the items
    /// contained in that list.
    /// </summary>
    ///
    /// <typeparam name="T">The type of the items contained in this read-only list.</typeparam>
    internal sealed class ReadOnlyList<T> : IReadOnlyList<T>
    {
        private readonly IList<T> dataContainer;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReadOnlyList{T}"/> class.
        /// </summary>
        ///
        /// <param name="dataContainer">The source list where the items are contained.</param>
        ///
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="dataContainer"/> is null.</exception>
        public ReadOnlyList(IList<T> dataContainer)
        {
            this.dataContainer = dataContainer ?? throw new ArgumentNullException(nameof(dataContainer));
        }

        /// <summary>
        /// Gets the number of the elements contained in the list.
        /// </summary>
        public int Count => dataContainer.Count;

        /// <summary>
        /// Gets the item from the list at the specified <paramref name="index"/>.
        /// </summary>
        /// <param name="index">The index of the element to get.</param>
        ///
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="index"/> is negative
        /// or is greater or equal to <see cref="Count"/>.</exception>
        ///
        /// <returns>The element at the specified <paramref name="index"/>.</returns>
        public T this[int index] => dataContainer[index];
    }
}
