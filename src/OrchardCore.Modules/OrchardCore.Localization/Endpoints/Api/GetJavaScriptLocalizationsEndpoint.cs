using System;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Net.Http.Headers;

namespace OrchardCore.Localization.Endpoints.Api;

public static class GetJavaScriptLocalizationsEndpoint
{
    private static readonly char[] _groupSeparator = [','];

    public static IEndpointRouteBuilder AddGetJavaScriptLocalizationsEndpoint(this IEndpointRouteBuilder builder)
    {
        builder.MapGet("api/localization/js/{group}", HandleAsync)
            .WithName("ApiGetJavaScriptLocalizations")
            .WithTags("LocalizationApi")
            .AllowAnonymous()
            .DisableAntiforgery()
            .Produces<Dictionary<string, string>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status304NotModified);

        return builder;
    }

    // Anonymous by design: callers such as a standalone SPA may need their UI labels before the user
    // authenticates, and IJSLocalizer implementations only ever expose non-sensitive display strings —
    // the same set a Razor view renders via Orchard.GetJSLocalizations(group). This route is always
    // enabled (see the OrchardCore.Localization.Js feature) so it works regardless of which module owns
    // a given group, or whether that module is even enabled — a disabled owning module simply
    // contributes no keys for its group, leaving it to the caller's own base-language defaults.
    // The payload only changes per requested group set and per UI culture (localization sources are
    // static within a shell's lifetime), so it is memoized in the tenant-scoped IMemoryCache — a shell
    // reload replaces the container and the cache with it — and served with an ETag so repeat loads
    // revalidate to 304 instead of paying the localizer lookups and serialization again.
    private static IResult HandleAsync(string group, HttpContext httpContext, IEnumerable<IJSLocalizer> jsLocalizers, IMemoryCache cache)
    {
        var groups = group.Split(_groupSeparator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var culture = CultureInfo.CurrentUICulture.Name;
        var cacheKey = $"JavaScriptLocalizations_{string.Join(',', groups)}_{culture}";

        var (payload, etag) = cache.GetOrCreate(cacheKey, _ =>
        {
            // Same merge a Razor view uses via Orchard.GetJSLocalizations, so the two surfaces can
            // never drift.
            var result = jsLocalizers.GetMergedLocalizations(groups);
            var bytes = JsonSerializer.SerializeToUtf8Bytes(result);

            return (bytes, new EntityTagHeaderValue($"\"{Convert.ToHexStringLower(SHA256.HashData(bytes))[..16]}\""));
        });

        var headers = httpContext.Response.GetTypedHeaders();
        headers.ETag = etag;
        headers.CacheControl = new CacheControlHeaderValue { Public = true, MaxAge = TimeSpan.FromMinutes(5) };
        httpContext.Response.Headers[HeaderNames.Vary] = "Accept-Language";

        var requestHeaders = httpContext.Request.GetTypedHeaders();
        if (requestHeaders.IfNoneMatch.Count > 0)
        {
            foreach (var candidate in requestHeaders.IfNoneMatch)
            {
                if (candidate.Compare(etag, useStrongComparison: false))
                {
                    return TypedResults.StatusCode(StatusCodes.Status304NotModified);
                }
            }
        }

        return TypedResults.Bytes(payload, "application/json; charset=utf-8");
    }
}
