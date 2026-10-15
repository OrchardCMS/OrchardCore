using Ganss.Xss;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace OrchardCore.Infrastructure.Html;

public class HtmlSanitizerService : IHtmlSanitizerService, IDisposable
{
    // The maximum length of the fragments whose sanitized result is cached.
    private const int MaxCachedLength = 64 * 1024;

    // The maximum number of characters, of the cached fragments and their sanitized results, kept in the cache.
    private const long CacheSizeLimit = 4 * 1024 * 1024;

    private readonly HtmlSanitizer _sanitizer = new();

    // Content is sanitized each time it is displayed, while it rarely changes. As the sanitizer configuration
    // is fixed for the lifetime of the tenant, the result for a given fragment can be reused, which avoids
    // parsing and serializing the same fragments again on each request.
    private readonly MemoryCache _cache = new(new MemoryCacheOptions { SizeLimit = CacheSizeLimit });

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

        if (_cache.TryGetValue(html, out string sanitized))
        {
            return sanitized;
        }

        sanitized = _sanitizer.Sanitize(html);

        _cache.Set(html, sanitized, new MemoryCacheEntryOptions { Size = html.Length + sanitized.Length });

        return sanitized;
    }

    public void Dispose()
    {
        _cache.Dispose();
        GC.SuppressFinalize(this);
    }
}
