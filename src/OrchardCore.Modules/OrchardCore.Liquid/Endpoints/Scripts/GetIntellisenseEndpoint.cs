using System.IO.Hashing;
using System.Text;
using Fluid;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using OrchardCore.DisplayManagement.Liquid;
using OrchardCore.Environment.Shell.Configuration;

#nullable enable

namespace OrchardCore.Liquid.Endpoints.Scripts;

public static class GetIntellisenseEndpoint
{
    public static IEndpointRouteBuilder AddGetIntellisenseScriptEndpoint(this IEndpointRouteBuilder builder)
    {
        builder.MapGet("OrchardCore.Liquid/Scripts/liquid-intellisense.js", HandleRequest)
            .AllowAnonymous()
            .DisableAntiforgery();

        return builder;
    }

    private static byte[] GenerateScript(IServiceProvider serviceProvider)
    {
        var templateOptions = serviceProvider.GetRequiredService<IOptions<TemplateOptions>>();
        var liquidViewParser = serviceProvider.GetRequiredService<LiquidViewParser>();
        var filters = string.Join(',', templateOptions.Value.Filters.Select(x => $"'{x.Key}'"));
        var tags = string.Join(',', liquidViewParser.RegisteredTags.Select(x => $"'{x.Key}'"));
        var script = $@"[{filters}].forEach(value=>{{if(!liquidFilters.includes(value)){{ liquidFilters.push(value);}}}});
                    [{tags}].forEach(value=>{{if(!liquidTags.includes(value)){{ liquidTags.push(value);}}}});";

        return Encoding.UTF8.GetBytes(script);
    }

    private static IResult HandleRequest(HttpContext context, IMemoryCache cache, IShellConfiguration shellConfiguration)
    {
        context.Response.Headers.CacheControl = shellConfiguration.GetValue(
                "StaticFileOptions:CacheControl",
                // Fallback value
                $"public, max-age={TimeSpan.FromDays(30).TotalSeconds}");

        // Assumes IfNoneMatch has only one ETag for performance
        var requestETag = context.Request.Headers.IfNoneMatch;
        if (!StringValues.IsNullOrEmpty(requestETag) && cache.Get(requestETag) != null)
        {
            context.Response.Headers.ETag = requestETag;

            return Results.StatusCode(304);
        }

        const string scriptCacheKey = "OrchardCore.Liquid/Scripts/liquid-intellisense.js";

        var scriptBytes = (byte[]?)cache.Get(scriptCacheKey);
        if (scriptBytes == null)
        {
            scriptBytes = GenerateScript(context.RequestServices);

            cache.Set(scriptCacheKey, scriptBytes);
        }

        // Uses cross-processes hashing to enable revalidation after restart
        StringValues eTag = $"\"{XxHash3.HashToUInt64(scriptBytes, 0)}\"";

        // Mark that the eTag corresponds to a fresh file
        cache.Set(eTag, true);

        context.Response.Headers.ETag = eTag;

        // Can be true if the cache was reset after the last client request
        if (requestETag.Equals(eTag))
        {
            return Results.StatusCode(304);
        }

        return Results.Bytes(scriptBytes, "application/javascript");
    }
}
