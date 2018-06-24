// <copyright file="LinkedListExtensions.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.Tools
{
    using System;
    using System.Collections.Generic;

    internal static class LinkedListExtensions
    {
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
