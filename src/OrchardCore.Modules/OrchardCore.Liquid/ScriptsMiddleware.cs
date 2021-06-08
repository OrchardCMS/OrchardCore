using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using OrchardCore.DisplayManagement.Liquid;
using OrchardCore.Entities;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Settings;
using Fluid;
using Microsoft.Extensions.Caching.Memory;

namespace OrchardCore.Liquid
{
    public class ScriptsMiddleware
    {
        private readonly RequestDelegate _next;

        public ScriptsMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            if (httpContext.Request.Path.StartsWithSegments("/OrchardCore.Liquid/Scripts", StringComparison.OrdinalIgnoreCase))
            {
                var script = default(string);
                if (Path.GetFileName(httpContext.Request.Path.Value) == "liquid-intellisense.js")
                {
                    var cache = httpContext.RequestServices.GetRequiredService<IMemoryCache>();
                    const string key = "OrchardCore.Liquid/Scripts/liquid-intellisense.js";
                    if (!cache.TryGetValue(key, out script))
                    {

                        var templateOptions = httpContext.RequestServices.GetRequiredService<IOptions<TemplateOptions>>();
                        var liquidViewParser = httpContext.RequestServices.GetRequiredService<LiquidViewParser>();

                        var filters = string.Join(',', templateOptions.Value.Filters.Select(x => $"'{x.Key}'"));
                        var tags = string.Join(',', liquidViewParser.RegisteredTags.Select(x => $"'{x.Key}'"));

                        script = $@"[{filters}].forEach(value=>{{if(!liquidFilters.includes(value)){{ liquidFilters.push(value);}}}});
[{tags}].forEach(value=>{{if(!liquidTags.includes(value)){{ liquidTags.push(value);}}}});";

                        cache.Set(key, script);
                    }
                    var bytes = Encoding.UTF8.GetBytes(script);
                    var cancellationToken = httpContext?.RequestAborted ?? CancellationToken.None;
                    await httpContext.Response.Body.WriteAsync(Encoding.UTF8.GetBytes(script), 0, bytes.Length, cancellationToken);
                    return;
                }
            }

            await _next.Invoke(httpContext);
        }
    }
}
