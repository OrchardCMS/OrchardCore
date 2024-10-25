using System.Text;
using Fluid;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using OrchardCore.DisplayManagement.Liquid;
using OrchardCore.Environment.Shell.Configuration;

namespace OrchardCore.Liquid;

public class ScriptsMiddleware
{
    private readonly RequestDelegate _next;
    private byte[] _bytes;
    private string _etag;

    public ScriptsMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext httpContext)
    {
        if (httpContext.Request.Path.StartsWithSegments("/OrchardCore.Liquid/Scripts", StringComparison.OrdinalIgnoreCase))
        {
            if (Path.GetFileName(httpContext.Request.Path.Value) == "liquid-intellisense.js")
            {
                if (httpContext.Request.Headers.TryGetValue(HeaderNames.IfNoneMatch, out var v))
                {
                    if (v.Contains(_etag))
                    {
                        httpContext.Response.StatusCode = StatusCodes.Status304NotModified;

                        return;
                    }
                }

                var cacheControl = $"public, max-age={TimeSpan.FromDays(30).TotalSeconds}, s-max-age={TimeSpan.FromDays(365.25).TotalSeconds}";
                if (_bytes == null)
                {
                    var templateOptions = httpContext.RequestServices.GetRequiredService<IOptions<TemplateOptions>>();
                    var liquidViewParser = httpContext.RequestServices.GetRequiredService<LiquidViewParser>();
                    var shellConfiguration = httpContext.RequestServices.GetRequiredService<IShellConfiguration>();
                    cacheControl = shellConfiguration.GetValue("StaticFileOptions:CacheControl", cacheControl);

                    var filters = string.Join(',', templateOptions.Value.Filters.Select(x => $"'{x.Key}'"));
                    var tags = string.Join(',', liquidViewParser.RegisteredTags.Select(x => $"'{x.Key}'"));

                    var script = $@"[{filters}].forEach(value=>{{if(!liquidFilters.includes(value)){{ liquidFilters.push(value);}}}});
                                [{tags}].forEach(value=>{{if(!liquidTags.includes(value)){{ liquidTags.push(value);}}}});";

                    _etag = Guid.NewGuid().ToString("n");
                    _bytes = Encoding.UTF8.GetBytes(script);
                }

                httpContext.Response.Headers[HeaderNames.CacheControl] = cacheControl;
                httpContext.Response.Headers[HeaderNames.ContentType] = "application/javascript";
                httpContext.Response.Headers[HeaderNames.ETag] = _etag;

                await httpContext.Response.Body.WriteAsync(_bytes, httpContext.RequestAborted);

                return;
            }
        }

        await _next.Invoke(httpContext);
    }
}
