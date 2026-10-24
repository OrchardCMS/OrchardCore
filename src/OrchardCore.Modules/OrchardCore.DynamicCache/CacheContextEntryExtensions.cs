using OrchardCore.Environment.Cache;

namespace OrchardCore.DynamicCache;

public static class CacheContextEntryExtensions
{
    public static string GetContextHash(this IEnumerable<CacheContextEntry> entries)
    {
        var sorted = entries.ToArray();

        if (sorted.Length == 0)
        {
            return string.Empty;
        }

        // Same order as 'OrderBy(x => x.Key).ThenBy(x => x.Value)', so that the keys of the existing cache entries are preserved.
        Array.Sort(sorted, static (x, y) =>
        {
            var result = Comparer<string>.Default.Compare(x.Key, y.Key);
            return result != 0 ? result : Comparer<string>.Default.Compare(x.Value, y.Value);
        });

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
