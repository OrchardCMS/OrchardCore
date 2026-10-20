using Ganss.Xss;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace OrchardCore.Infrastructure.Html;

public class HtmlSanitizerService : IHtmlSanitizerService
{
    // The maximum length of the fragments whose sanitized result is cached.
    private const int MaxCachedLength = 64 * 1024;

    // The maximum number of characters, of the cached fragments and their sanitized results, kept in the cache.
    private const long CacheSizeLimit = 4 * 1024 * 1024;

    // An entry that is not read for this long is evicted, so fragments that are no longer displayed, or that
    // belong to a tenant that was released or is idle, don't stay in memory.
    private static readonly TimeSpan _slidingExpiration = TimeSpan.FromMinutes(10);

    // The entries are evicted at the latest after this long, even if they are read continuously.
    private static readonly TimeSpan _absoluteExpiration = TimeSpan.FromHours(1);

    // Content is sanitized each time it is displayed, while it rarely changes, so the result for a given fragment
    // can be reused, which avoids parsing and serializing the same fragments again on each request. The cache is
    // shared by all the tenants so that its size doesn't grow with the number of tenants, which is why the entries
    // are keyed by the service that created them: each tenant has its own sanitizer configuration, so the result
    // of one tenant must never be served to another.
    private static readonly MemoryCache _cache = new(new MemoryCacheOptions { SizeLimit = CacheSizeLimit });

    private static long _lastId;

    private readonly HtmlSanitizer _sanitizer = new();
    private readonly long _id = Interlocked.Increment(ref _lastId);

    public HtmlSanitizerService(IOptions<HtmlSanitizerOptions> options)
    {
        foreach (var action in options.Value.Configure)
        {
            action?.Invoke(_sanitizer);
        }
    }

    public string Sanitize(string html)
    {
        if (string.IsNullOrEmpty(html) || html.Length > MaxCachedLength)
        {
            return _sanitizer.Sanitize(html);
        }

        var key = new CacheKey(_id, html);

        if (_cache.TryGetValue(key, out string sanitized))
        {
            return sanitized;
        }

        sanitized = _sanitizer.Sanitize(html);

        _cache.Set(key, sanitized, new MemoryCacheEntryOptions
        {
            Size = html.Length + sanitized.Length,
            SlidingExpiration = _slidingExpiration,
            AbsoluteExpirationRelativeToNow = _absoluteExpiration,
        });

        return sanitized;
    }

    private readonly record struct CacheKey(long ServiceId, string Html);
}
