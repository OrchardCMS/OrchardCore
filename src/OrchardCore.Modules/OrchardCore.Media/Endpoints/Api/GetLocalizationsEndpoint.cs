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
using OrchardCore.Localization;

namespace OrchardCore.Media.Endpoints.Api;

public static class GetLocalizationsEndpoint
{
    public static IEndpointRouteBuilder AddGetLocalizationsEndpoint(this IEndpointRouteBuilder builder)
    {
        builder.MapGet("api/media/localizations", HandleAsync)
            .WithName("ApiGetMediaLocalizations")
            .WithTags("MediaApi")
            .AllowAnonymous()
            .DisableAntiforgery()
            .Produces<Dictionary<string, string>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status304NotModified);

        return builder;
    }

    // Anonymous by design: the standalone gallery loads its UI labels before the user authenticates,
    // and these are non-sensitive display strings — the same set the embedded admin page renders via
    // Orchard.GetJSLocalizations("media-gallery"). The payload only changes per UI culture and per
    // tenant reload (localization sources are static within a shell's lifetime), so it is memoized
    // per culture in the tenant-scoped IMemoryCache — a shell reload replaces the container and the
    // cache with it — and served with an ETag so repeat loads revalidate to 304 instead of paying
    // the localizer lookups and serialization again.
    private static IResult HandleAsync(HttpContext httpContext, IEnumerable<IJSLocalizer> jsLocalizers, IMemoryCache cache)
    {
        var culture = CultureInfo.CurrentUICulture.Name;
        var (payload, etag) = cache.GetOrCreate($"MediaGalleryLocalizations_{culture}", _ =>
        {
            // Same merge the embedded admin page uses via Orchard.GetJSLocalizations, so the two
            // surfaces can never drift.
            var result = jsLocalizers.GetMergedLocalizations("media-gallery");
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
