// <copyright file="LinkedListExtensions.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.Tools
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// A class containing extension methods for the <see cref="LinkedList{T}"/>.
    /// </summary>
    internal static class LinkedListExtensions
    {
        /// <summary>
        /// Gets the linked list's firsts node that matches the specified <paramref name="predicate"/>,
        /// or null if no matching node was found.
        /// </summary>
        ///
        /// <typeparam name="T">The linked list item's type.</typeparam>
        ///
        /// <param name="list">The linked list to search in.</param>
        /// <param name="predicate">The predicate to use for the search.</param>
        ///
        /// <returns>The first linked list's node that matches the <paramref name="predicate"/>, or null.</returns>
        ///
        /// <exception cref="ArgumentNullException">Thrown when any argument is null.</exception>
        public static LinkedListNode<T> FirstOrDefaultNode<T>(this LinkedList<T> list, Predicate<T> predicate)
        {
            if (list == null)
            {
                throw new ArgumentNullException(nameof(list));
            }

            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }

            if (list.Count == 0)
            {
                return null;
            }

            LinkedListNode<T> node = list.First;
            while (node != null)
            {
                if (predicate(node.Value))
                {
                    return node;
                }

                node = node.Next;
            }

            return null;
        }
    }
}
