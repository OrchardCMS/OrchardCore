using Fluid;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using OrchardCore.ContentManagement;
using OrchardCore.DisplayManagement.Liquid;
using OrchardCore.Environment.Shell.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Caching.Memory;

namespace OrchardCore.Liquid.Endpoints.Api;

public static class GetIntellisenseEndpoint
{

    public static IEndpointRouteBuilder AddGetIntellisenseEndpoint(this IEndpointRouteBuilder builder)
    {
        builder.MapGet("/OrchardCore.Liquid/Scripts/liquid-intellisense.js", HandleAsync)
            .AllowAnonymous()
            .DisableAntiforgery();

        return builder;
    }

    [Authorize(AuthenticationSchemes = "Api")]
    private static Task<IResult> HandleAsync(HttpContext httpContext, IMemoryCache cache)
    {
        const string cacheKey = "LiquidIntellisenseScript";

        var cacheControl = $"public, max-age={TimeSpan.FromDays(30).TotalSeconds}, s-max-age={TimeSpan.FromDays(365.25).TotalSeconds}";
        if (cache.TryGetValue(cacheKey, out (byte[] Bytes, string ETag) cachedData))
        {
            if (httpContext.Request.Headers.TryGetValue(HeaderNames.IfNoneMatch, out var etagHeader) &&
                etagHeader.Contains(cachedData.ETag))
            {
                return Task.FromResult(Results.StatusCode(StatusCodes.Status304NotModified));
            }

            httpContext.Response.Headers[HeaderNames.CacheControl] = cacheControl; 
            httpContext.Response.Headers[HeaderNames.ContentType] = "application/javascript";
            httpContext.Response.Headers[HeaderNames.ETag] = cachedData.ETag;

            return Task.FromResult(Results.Bytes(cachedData.Bytes, "application/javascript"));
        }

        var shellConfiguration = httpContext.RequestServices.GetRequiredService<IShellConfiguration>();
        cacheControl = shellConfiguration.GetValue("StaticFileOptions:CacheControl", cacheControl);

        var templateOptions = httpContext.RequestServices.GetRequiredService<IOptions<TemplateOptions>>();
        var liquidViewParser = httpContext.RequestServices.GetRequiredService<LiquidViewParser>();

        var filters = string.Join(',', templateOptions.Value.Filters.Select(x => $"'{x.Key}'"));
        var tags = string.Join(',', liquidViewParser.RegisteredTags.Select(x => $"'{x.Key}'"));

        var script = $@"[{filters}].forEach(value=>{{if(!liquidFilters.includes(value)){{ liquidFilters.push(value);}}}});
                        [{tags}].forEach(value=>{{if(!liquidTags.includes(value)){{ liquidTags.push(value);}}}});";

        var etag = Guid.NewGuid().ToString("n");
        var bytes = Encoding.UTF8.GetBytes(script);
        cache.Set(cacheKey,(bytes,etag));

        httpContext.Response.Headers[HeaderNames.CacheControl] = cacheControl;
        httpContext.Response.Headers[HeaderNames.ContentType] = "application/javascript";
        httpContext.Response.Headers[HeaderNames.ETag] = etag;

        return Task.FromResult(Results.Bytes(bytes, "application/javascript"));
    }

}
