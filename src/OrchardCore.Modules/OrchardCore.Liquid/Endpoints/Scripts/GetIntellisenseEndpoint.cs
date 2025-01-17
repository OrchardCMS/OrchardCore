using Fluid;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.DisplayManagement.Liquid;
using OrchardCore.Environment.Shell.Configuration;
using System.Text;

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

    private static IResult HandleRequest(HttpContext context, IMemoryCache cache)
    {
        const string cacheKey = "OrchardCore.Liquid/Scripts/liquid-intellisense.js";

        bool isScriptCached = cache.TryGetValue(cacheKey, out byte[] scriptBytes);
        if (!isScriptCached)
        {
            scriptBytes = GenerateScript(context.RequestServices);
            cache.Set(cacheKey, scriptBytes);
        }

        // Set cache header from configuration
        var shellConfiguration = context.RequestServices.GetRequiredService<IShellConfiguration>();
        context.Response.Headers.CacheControl = shellConfiguration.GetValue(
                "StaticFileOptions:CacheControl",
                // Fallback value
                $"public, max-age={TimeSpan.FromDays(30).TotalSeconds}");

        return Results.Bytes(scriptBytes, "application/javascript");
    }
}
