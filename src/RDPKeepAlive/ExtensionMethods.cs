using System;
using System.Collections.Generic;

namespace RDPKeepAlive
{
    internal static class ExtensionMethods
    {
        /// <summary>
        ///     Checks if the collection contains the specified item.
        /// </summary>
        /// <param name="collection">
        ///     The collection to search.
        /// </param>
        /// <param name="item">
        ///     The item to find.
        /// </param>
        /// <param name="stringComparison">
        ///     The string comparison to use. OrdinalIgnoreCase by defaults
        /// </param>
        /// <returns>
        ///     True if the item is found; otherwise, false.
        /// </returns>
        public static bool Contains(this IEnumerable<string> collection, string item, StringComparison stringComparison = StringComparison.OrdinalIgnoreCase)
        {
            ArgumentNullException.ThrowIfNull(collection);

            if (string.IsNullOrEmpty(item))
            {
                throw new ArgumentException($"'{nameof(item)}' cannot be null or empty.", nameof(item));
            }

            foreach (var element in collection)
            {
                if (string.Equals(element, item, stringComparison))
                {
                    return true;
                }
            }
            return false;
        }
    }
}