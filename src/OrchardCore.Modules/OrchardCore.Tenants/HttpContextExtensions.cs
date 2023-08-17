using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using OrchardCore.Environment.Shell;
using OrchardCore.Tenants.ViewModels;

namespace OrchardCore.Tenants;
public static class HttpContextExtensions
{
    public static string GetEncodedUrl(this HttpContext httpContext, ShellSettingsEntry entry, bool appendQuery = true)
    {
        var shellSettings = entry.ShellSettings;
        var host = shellSettings.RequestUrlHosts.FirstOrDefault();

        var hostString = httpContext.Request.Host;
        if (host is not null)
        {
            hostString = new HostString(host);
            if (httpContext.Request.Host.Port.HasValue)
            {
                hostString = new HostString(hostString.Host, httpContext.Request.Host.Port.Value);
            }
        }

        var pathString = httpContext.Features.Get<ShellContextFeature>()?.OriginalPathBase ?? PathString.Empty;
        if (!String.IsNullOrEmpty(shellSettings.RequestUrlPrefix))
        {
            pathString = pathString.Add('/' + shellSettings.RequestUrlPrefix);
        }

        var queryString = QueryString.Empty;
        if (appendQuery && !String.IsNullOrEmpty(entry.Token))
        {
            queryString = QueryString.Create("token", entry.Token);
        }

        return $"{httpContext.Request.Scheme}://{hostString + pathString + queryString}";
    }
}
