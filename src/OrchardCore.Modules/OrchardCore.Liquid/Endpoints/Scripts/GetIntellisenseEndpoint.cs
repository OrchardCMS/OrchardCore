using System.IO.Hashing;
using System.Text;
using Fluid;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using OrchardCore.DisplayManagement.Liquid;

#nullable enable

namespace OrchardCore.Liquid.Endpoints.Scripts;

public static class GetIntellisenseEndpoint
{
    // Update this version when the script changes to invalidate client caches
    private static readonly int ScriptVersion = 1;

    public static IEndpointRouteBuilder AddGetIntellisenseScriptEndpoint(this IEndpointRouteBuilder builder)
    {
        builder.MapGet("OrchardCore.Liquid/Scripts/liquid-intellisense.js", HandleRequest)
            .AllowAnonymous()
            .DisableAntiforgery();

        return builder;
    }

    public static ulong HashCacheBustingValues(LiquidViewParser liquidViewParser, TemplateOptions templateOptions)
    {
        var hash = new XxHash3(ScriptVersion);

        foreach (var filter in templateOptions.Filters)
        {
            hash.Append(Encoding.UTF8.GetBytes(filter.Key ?? ""));
        }

        foreach (var tag in liquidViewParser.RegisteredTags)
        {
            hash.Append(Encoding.UTF8.GetBytes(tag.Key ?? ""));
        }

        return hash.GetCurrentHashAsUInt64();
    }

    private static IResult HandleRequest(HttpContext context, IMemoryCache memoryCache, LiquidViewParser liquidViewParser, IOptions<TemplateOptions> templateOptions)
    {
        // We could check that the current cache entry matches the requested hash, but instead we always return the actual content that
        // the client should have. The hash is only used for cache busting on the client.

        // The cache entry will be busted whenever the filters or tags change, i.e. when some features are enabled or disabled.

        var scriptBytes = memoryCache.GetOrCreate("LiquidIntellisenseScript", entry =>
        {
            entry.SetSlidingExpiration(TimeSpan.FromHours(1));

            return GenerateScriptBytes(liquidViewParser, templateOptions);
        });

        if (scriptBytes == null)
        {
            return Results.InternalServerError();
        }

        // Set the cache timeout to the maximum allowed length of one year
        // max-age is needed because immutable is not widely supported
        context.Response.Headers.CacheControl = "public, max-age=31536000, immutable";

        return Results.Bytes(scriptBytes, "application/javascript");
    }

    private static byte[] GenerateScriptBytes(LiquidViewParser liquidViewParser, IOptions<TemplateOptions> templateOptions)
    {
        var filters = string.Join(',', templateOptions.Value.Filters.Select(x => $"'{x.Key}'"));
        var tags = string.Join(',', liquidViewParser.RegisteredTags.Select(x => $"'{x.Key}'"));

        // Note: Update ScriptVersion when the script changes
        // Note: All injected values except those in url must be used in HashCacheBustingValues
        var scriptBytes = (new byte[][] {
                "["u8.ToArray(),
                Encoding.UTF8.GetBytes(filters),
                "].forEach(value=>{if(!liquidFilters.includes(value)){ liquidFilters.push(value);}});["u8.ToArray(),
                Encoding.UTF8.GetBytes(tags),
                "].forEach(value=>{if(!liquidTags.includes(value)){ liquidTags.push(value);}});"u8.ToArray()
            }).SelectMany(x => x).ToArray();

        return scriptBytes;
    }
}
