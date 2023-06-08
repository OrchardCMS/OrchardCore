using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using OrchardCore.Admin;
using OrchardCore.Modules.FileProviders;

namespace OrchardCore.Seo.Services;

public class RobotsMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IStaticFileProvider _staticFileProvider;
    private readonly IEnumerable<IRobotsProvider> _robotsProviders;
    private readonly AdminOptions _adminOptions;

    public RobotsMiddleware(
        RequestDelegate next,
        IStaticFileProvider staticFileProvider,
        IEnumerable<IRobotsProvider> robotsProviders,
        IOptions<AdminOptions> adminOptions)
    {
        _next = next;
        _staticFileProvider = staticFileProvider;
        _robotsProviders = robotsProviders;
        _adminOptions = adminOptions.Value;
    }

    public async Task Invoke(HttpContext httpContext)
    {
        if (httpContext.Request.Path.StartsWithSegments("/" + SeoConstants.RobotsFileName))
        {
            var file = _staticFileProvider.GetFileInfo(SeoConstants.RobotsFileName);

            if (file.Exists)
            {
                // At this point we know that the a robots.txt file exists as a static file. Let the static file provider handle it.
                await _next(httpContext);

                return;
            }

            httpContext.Response.Clear();
            httpContext.Response.ContentType = "text/plain";

            var content = new StringBuilder();

            foreach (var provider in _robotsProviders.Reverse())
            {
                var item = (await provider.ContentAsync())?.Trim();

                if (String.IsNullOrEmpty(item))
                {
                    continue;
                }

                content.AppendLine(item);
            }

            if (content.Length > 0)
            {
                await httpContext.Response.WriteAsync(content.ToString());
            }
            else
            {
                await httpContext.Response.WriteAsync(SeoHelpers.GetDefaultRobotsContents(_adminOptions));
            }
        }

        await _next(httpContext);
    }
}
