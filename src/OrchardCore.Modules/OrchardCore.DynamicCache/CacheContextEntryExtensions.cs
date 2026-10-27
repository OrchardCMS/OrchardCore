using System.Runtime.InteropServices;
using OrchardCore.Environment.Cache;

namespace OrchardCore.DynamicCache;

public static class CacheContextEntryExtensions
{
    private static readonly Comparison<CacheContextEntry> s_comparison = static (x, y) =>
    {
        var result = Comparer<string>.Default.Compare(x.Key, y.Key);
        return result != 0 ? result : Comparer<string>.Default.Compare(x.Value, y.Value);
    };

    public static string GetContextHash(this IEnumerable<CacheContextEntry> entries)
    {
        // The entries come from the cache context manager as a list, which is sorted in place
        // instead of being copied, a copy is only made for any other source.
        var sorted = entries is List<CacheContextEntry> list
            ? CollectionsMarshal.AsSpan(list)
            : entries.ToArray().AsSpan();

        if (sorted.Length == 0)
        {
            return string.Empty;
        }

        // Same order as 'OrderBy(x => x.Key).ThenBy(x => x.Value)', so that the keys of the existing cache entries are preserved.
        sorted.Sort(s_comparison);

        var length = 0;
        foreach (var entry in sorted)
        {
            length += (entry.Key?.Length ?? 0) + (entry.Value?.Length ?? 0);
        }

        return string.Create(length, sorted, static (span, entries) =>
        {
            foreach (var entry in entries)
            {
                if (entry.Key is not null)
                {
                    entry.Key.AsSpan().CopyTo(span);
                    span = span[entry.Key.Length..];
                }

                if (entry.Value is not null)
                {
                    entry.Value.AsSpan().CopyTo(span);
                    span = span[entry.Value.Length..];
                }
            }
        });
    }
}
