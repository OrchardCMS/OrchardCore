using System;
using Microsoft.AspNetCore.Http;

namespace OrchardCore.BackgroundTasks;

internal static class HttpContextExtensions
{
    public static void SetBaseUrl(this HttpContext context, string baseUrl)
    {
        if (Uri.TryCreate(baseUrl, UriKind.Absolute, out var uri))
        {
            context.Request.Scheme = uri.Scheme;
            context.Request.Host = new HostString(uri.Host, uri.Port);
            context.Request.PathBase = uri.AbsolutePath;

            if (!String.IsNullOrWhiteSpace(uri.Query))
            {
                context.Request.QueryString = new QueryString(uri.Query);
            }
        }
    }
}
