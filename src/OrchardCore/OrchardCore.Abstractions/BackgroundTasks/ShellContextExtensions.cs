using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Builders;
using OrchardCore.Modules;

namespace OrchardCore.BackgroundTasks;

public static class ShellContextExtensions
{
    private const string Localhost = "localhost";

    public static HttpContext CreateHttpContext(this ShellContext shell)
    {
        var context = CreateHttpContext(shell.Settings);
        context.Features.Set(new ShellContextFeature
        {
            ShellContext = shell,
            OriginalPathBase = PathString.Empty,
            OriginalPath = "/"
        });

        return context;
    }

    private static HttpContext CreateHttpContext(ShellSettings settings)
    {
        var context = new DefaultHttpContext().UseShellScopeServices();

        context.Request.Scheme = "https";
        var urlHost = settings.RequestUrlHosts.FirstOrDefault();
        context.Request.Host = new HostString(urlHost ?? Localhost);

        context.Request.PathBase = PathString.Empty;
        if (!String.IsNullOrWhiteSpace(settings.RequestUrlPrefix))
        {
            context.Request.PathBase = $"/{settings.RequestUrlPrefix}";
        }

        context.Request.Path = "/";
        context.Items["IsBackground"] = true;

        return context;
    }
}
