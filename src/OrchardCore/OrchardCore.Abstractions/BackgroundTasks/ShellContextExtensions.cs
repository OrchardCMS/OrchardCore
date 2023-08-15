using System;
using System.Linq;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Builders;
using OrchardCore.Modules;

namespace OrchardCore.BackgroundTasks;

public static class ShellContextExtensions
{
    public static HttpContext CreateHttpContext(this ShellContext shell, IServerAddressesFeature feature = null)
    {
        feature ??= shell.ServiceProvider
            ?.GetService<IServer>()
            ?.Features.Get<IServerAddressesFeature>();

        BindingAddress serverAddress = null;
        if (feature is not null)
        {
            foreach (var address in feature.Addresses)
            {
                var bindingAddress = BindingAddress.Parse(address);
                if (bindingAddress.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase))
                {
                    serverAddress = bindingAddress;
                    break;
                }

                serverAddress ??= bindingAddress;
            }
        }

        var context = CreateHttpContext(shell.Settings, serverAddress);
        context.Features.Set(new ShellContextFeature
        {
            ShellContext = shell,
            OriginalPathBase = serverAddress?.PathBase ?? PathString.Empty,
            OriginalPath = "/"
        });

        return context;
    }

    private static HttpContext CreateHttpContext(ShellSettings settings, BindingAddress serverAddress)
    {
        var context = new DefaultHttpContext().UseShellScopeServices();

        var urlHost = settings.RequestUrlHosts.FirstOrDefault();
        if (!String.IsNullOrWhiteSpace(urlHost))
        {
            // Prioritize the host from the shell settings.
            context.Request.Host = new HostString(urlHost);
        }
        else if (!String.IsNullOrWhiteSpace(serverAddress?.Host))
        {
            if (serverAddress.Port > 0)
            {
                // Then the host from the server address.
                context.Request.Host = new HostString(serverAddress.Host, serverAddress.Port);
            }
            else
            {
                context.Request.Host = new HostString(serverAddress.Host);
            }
        }
        else
        {
            context.Request.Host = new HostString("localhost");
        }

        var pathBase = new PathString("/");
        if (!String.IsNullOrWhiteSpace(serverAddress?.PathBase))
        {
            // The server address may include a virtual folder.
            pathBase = pathBase.Add(serverAddress.PathBase);
        }

        if (!String.IsNullOrWhiteSpace(settings.RequestUrlPrefix))
        {
            pathBase = pathBase.Add($"/{settings.RequestUrlPrefix}");
        }

        context.Request.Scheme = "https";
        context.Request.PathBase = pathBase;
        context.Request.Path = "/";

        context.Items["IsBackground"] = true;

        return context;
    }
}
