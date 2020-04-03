using System.Collections.Generic;

namespace OrchardCore.Workflows.Helpers
{
    public static class ListExtensions
    {
        public static void AddRange<T>(this IList<T> list, IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                list.Add(item);
            }
        }

        public static void RemoveRange<T>(this IList<T> list, IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                list.Remove(item);
            }
        }
    }
}
