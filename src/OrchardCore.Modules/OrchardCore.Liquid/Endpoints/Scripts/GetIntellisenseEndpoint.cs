using System.IO.Hashing;
using System.Text;
using Fluid;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using OrchardCore.DisplayManagement.Liquid;

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

    private static IResult HandleRequest(HttpContext context, LiquidViewParser liquidViewParser, IOptions<TemplateOptions> templateOptions)
    {
        var filters = string.Join(',', templateOptions.Value.Filters.Select(x => $"'{x.Key}'"));
        var tags = string.Join(',', liquidViewParser.RegisteredTags.Select(x => $"'{x.Key}'"));

        var scriptBytes = (new byte[][] {
                "["u8.ToArray(),
                Encoding.UTF8.GetBytes(filters),
                "].forEach(value=>{if(!liquidFilters.includes(value)){ liquidFilters.push(value);}});["u8.ToArray(),
                Encoding.UTF8.GetBytes(tags),
                "].forEach(value=>{if(!liquidTags.includes(value)){ liquidTags.push(value);}});"u8.ToArray()
            }).SelectMany(x => x).ToArray();

        // Uses cross-processes hashing to enable revalidation after restart
        StringValues eTag = $"\"{XxHash3.HashToUInt64(scriptBytes, 0)}\"";

        context.Response.Headers.CacheControl = "no-cache";
        context.Response.Headers.ETag = eTag;

        // Assumes IfNoneMatch has only one ETag for performance
        if (context.Request.Headers.IfNoneMatch.Equals(eTag))
        {
            return Results.StatusCode(304);
        }

        return Results.Bytes(scriptBytes, "application/javascript");
    }
}
