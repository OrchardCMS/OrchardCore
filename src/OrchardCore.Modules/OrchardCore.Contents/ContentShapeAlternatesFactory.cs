using System.Collections.Concurrent;
using OrchardCore.DisplayManagement.Utilities;

namespace OrchardCore.Contents;

/// <summary>
/// Provides cached alternate patterns for Content shapes.
/// Alternates are computed once per unique configuration and cached for reuse.
/// </summary>
internal static class ContentShapeAlternatesFactory
{
    private const int CacheCapacity = 1_000;

    private static readonly ConcurrentDictionary<ContentAlternatesCacheKey, ContentAlternatesCacheEntry> _cache = new();
    private static long _clock;
    private static int _isTrimming;

    /// <summary>
    /// Gets or creates cached alternates for a Content shape configuration.
    /// </summary>
    public static string[] GetAlternates(string contentType, string contentItemId, string displayType)
    {
        var key = new ContentAlternatesCacheKey(contentType, contentItemId, displayType ?? string.Empty);
        var tick = Interlocked.Increment(ref _clock);

        if (_cache.TryGetValue(key, out var existingEntry))
        {
            existingEntry.Touch(tick);
            return existingEntry.Alternates;
        }

        var alternates = BuildAlternates(key);
        var entry = new ContentAlternatesCacheEntry(alternates, tick);
        var cachedEntry = _cache.GetOrAdd(key, entry);

        if (!ReferenceEquals(cachedEntry, entry))
        {
            cachedEntry.Touch(tick);
            return cachedEntry.Alternates;
        }

        TrimIfNeeded();
        return alternates;
    }

    internal static void ClearCache()
    {
        _cache.Clear();
        Interlocked.Exchange(ref _clock, 0);
        Volatile.Write(ref _isTrimming, 0);
    }

    internal readonly record struct ContentAlternatesCacheKey(
        string ContentType,
        string ContentItemId,
        string DisplayType);

    private sealed class ContentAlternatesCacheEntry
    {
        private long _lastAccess;

        public ContentAlternatesCacheEntry(string[] alternates, long lastAccess)
        {
            Alternates = alternates;
            _lastAccess = lastAccess;
        }

        public string[] Alternates { get; }

        public long LastAccess => Volatile.Read(ref _lastAccess);

        public void Touch(long lastAccess)
        {
            Volatile.Write(ref _lastAccess, lastAccess);
        }
    }

    private static void TrimIfNeeded()
    {
        if (_cache.Count <= CacheCapacity)
        {
            return;
        }

        if (Interlocked.CompareExchange(ref _isTrimming, 1, 0) != 0)
        {
            return;
        }

        try
        {
            while (_cache.Count > CacheCapacity)
            {
                var excess = _cache.Count - CacheCapacity;
                var snapshot = _cache.ToArray();
                if (snapshot.Length <= CacheCapacity)
                {
                    return;
                }

                Array.Sort(snapshot, static (left, right) => left.Value.LastAccess.CompareTo(right.Value.LastAccess));

                for (var i = 0; i < excess && i < snapshot.Length; i++)
                {
                    _cache.TryRemove(snapshot[i].Key, out _);
                }
            }
        }
        finally
        {
            Volatile.Write(ref _isTrimming, 0);
        }
    }

    private static string[] BuildAlternates(ContentAlternatesCacheKey key)
    {
        var alternates = new List<string>();
        var encodedContentType = key.ContentType.EncodeAlternateElement();

        // Content__[DisplayType] e.g. Content-Summary
        alternates.Add("Content_" + key.DisplayType.EncodeAlternateElement());

        // Content__[ContentType] e.g. Content-BlogPost
        alternates.Add("Content__" + encodedContentType);

        // Content__[Id] e.g. Content-42
        alternates.Add("Content__" + key.ContentItemId);

        // Content_[DisplayType]__[ContentType] e.g. Content-BlogPost.Summary
        alternates.Add("Content_" + key.DisplayType + "__" + encodedContentType);

        // Content_[DisplayType]__[Id] e.g. Content-42.Summary
        alternates.Add("Content_" + key.DisplayType + "__" + key.ContentItemId);

        return alternates.ToArray();
    }
}
