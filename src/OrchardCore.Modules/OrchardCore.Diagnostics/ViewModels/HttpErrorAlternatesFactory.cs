using System.Collections.Concurrent;
using System.Net;

namespace OrchardCore.Diagnostics.ViewModels;

internal static class HttpErrorAlternatesFactory
{
    private static readonly ConcurrentDictionary<HttpErrorAlternatesCacheKey, string[]> _cache = new();

    public static string[] GetAlternates(int code, HttpStatusCode statusCode)
    {
        var key = new HttpErrorAlternatesCacheKey(code, statusCode);

        return _cache.GetOrAdd(key, static k =>
        [
            $"HttpError__{k.Code}",
            $"HttpError__{k.StatusCode}"
        ]);
    }

    private readonly record struct HttpErrorAlternatesCacheKey(
        int Code,
        HttpStatusCode StatusCode);
}
