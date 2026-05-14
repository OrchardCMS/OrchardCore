using System.Collections.Concurrent;
using OrchardCore.DisplayManagement.Utilities;

namespace OrchardCore.Taxonomies;

internal static class TermAlternatesFactory
{
    private static readonly ConcurrentDictionary<TermPartAlternatesCacheKey, string[]> _termPartCache = new();
    private static readonly ConcurrentDictionary<TermAlternatesCacheKey, string[]> _termCache = new();

    public static string[] GetTermPartAlternates(string contentType, string displayType)
    {
        var key = new TermPartAlternatesCacheKey(contentType ?? string.Empty, displayType ?? string.Empty);

        return _termPartCache.GetOrAdd(key, static k =>
        [
            $"TermPart_{k.DisplayType}",
            $"{k.ContentType}__TermPart",
            $"{k.ContentType}_{k.DisplayType}__TermPart"
        ]);
    }

    public static string[] GetTermAlternates(string differentiator, string termContentType)
    {
        var key = new TermAlternatesCacheKey(differentiator ?? string.Empty, termContentType ?? string.Empty);

        return _termCache.GetOrAdd(key, static k =>
        {
            var encodedContentType = k.TermContentType.EncodeAlternateElement();

            if (string.IsNullOrEmpty(k.Differentiator))
            {
                return ["Term__" + encodedContentType];
            }

            return
            [
                "Term__" + k.Differentiator,
                "Term__" + encodedContentType,
            ];
        });
    }

    private readonly record struct TermPartAlternatesCacheKey(
        string ContentType,
        string DisplayType);

    private readonly record struct TermAlternatesCacheKey(
        string Differentiator,
        string TermContentType);
}
