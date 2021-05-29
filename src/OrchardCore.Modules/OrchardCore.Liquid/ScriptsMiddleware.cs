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
                    var p = new FluidParser();
                    var tags = string.Join(',', p.RegisteredTags.Keys.Select(x => $"'{x}'"));
                    
                    script = $@"[{tags}].forEach(value=>{{if(!liquidTags.includes(value)){{ liquidTags.push(value);}}}});";

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
