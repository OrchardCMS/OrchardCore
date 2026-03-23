using System.Net;
using System.Net.Mime;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using OrchardCore.Modules.FileProviders;

namespace OrchardCore.Seo.Services;

public class RobotsMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IStaticFileProvider _staticFileProvider;
    private readonly IEnumerable<IRobotsProvider> _robotsProviders;

    public RobotsMiddleware(
        RequestDelegate next,
        IStaticFileProvider staticFileProvider,
        IEnumerable<IRobotsProvider> robotsProviders)
    {
        _next = next;
        _staticFileProvider = staticFileProvider;
        _robotsProviders = robotsProviders;
    }

    public async Task Invoke(HttpContext httpContext)
    {
        if (httpContext.Request.Path.StartsWithSegments("/" + SeoConstants.RobotsFileName))
        {
            var file = _staticFileProvider.GetFileInfo(SeoConstants.RobotsFileName);

            if (file.Exists)
            {
                // At this point we know that a robots.txt file exists as a static file.
                // Let the static file provider handle it.
                await _next(httpContext);

                return;
            }

            var content = new StringBuilder();

            foreach (var provider in _robotsProviders.Reverse())
            {
                var item = (await provider.GetContentAsync())?.Trim();

                if (string.IsNullOrEmpty(item))
                {
                    continue;
                }

                content.AppendLine(item);
            }

            ClearResponse(httpContext.Response);
            httpContext.Response.ContentType = MediaTypeNames.Text.Plain;
            await httpContext.Response.WriteAsync(content.ToString());

            return;
        }

        await _next(httpContext);
    }

    /// <summary>
    /// Resets the <paramref name="response"/> status and body. Similar to <see cref="ResponseExtensions.Clear"/>, but
    /// doesn't clear <see cref="HttpResponse.Headers"/> so headers added by other middlewares can persist.
    /// </summary>
    private static void ClearResponse(HttpResponse response)
    {
        response.StatusCode = (int)HttpStatusCode.OK;
        response.HttpContext.Features.GetRequiredFeature<IHttpResponseFeature>().ReasonPhrase = null;

        if (response.Body.CanSeek)
        {
            response.Body.SetLength(0);
        }
    }
}
