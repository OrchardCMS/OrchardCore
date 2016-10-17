using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Orchard.Utility
{
    public static class CollectionExtensions
    {
        public static IReadOnlyList<T> ToReadOnlyCollection<T>(this IEnumerable<T> enumerable)
        {
            // ToArray trims the excess space and speeds up access
            return new ReadOnlyCollection<T>(new List<T>(enumerable).ToArray());
        }
    }
}