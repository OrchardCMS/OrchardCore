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
    private const string _localhost = "localhost";

    public static HttpContext CreateHttpContext(this ShellContext shell, IServerAddressesFeature feature = null)
    {
        BindingAddress address = null;
        feature ??= shell.ServiceProvider?.GetService<IServer>().Features.Get<IServerAddressesFeature>();
        if (feature is not null)
        {
            foreach (var addressString in feature.Addresses)
            {
                var bindingAddress = BindingAddress.Parse(addressString);
                if (bindingAddress.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase))
                {
                    address = bindingAddress;
                    break;
                }
            }
        }

        var context = CreateHttpContext(shell.Settings, address);
        context.Features.Set(new ShellContextFeature
        {
            ShellContext = shell,
            OriginalPathBase = address?.PathBase ?? PathString.Empty,
            OriginalPath = "/"
        });

        return context;
    }

    private static HttpContext CreateHttpContext(ShellSettings settings, BindingAddress address)
    {
        var context = new DefaultHttpContext().UseShellScopeServices();

        context.Request.Scheme = "https";

        if (!String.IsNullOrWhiteSpace(address?.Host))
        {
            if (address.Port > 0)
            {
                context.Request.Host = new HostString(address.Host, address.Port);
            }
            else
            {
                context.Request.Host = new HostString(address.Host);
            }
        }
        else
        {
            var urlHost = settings.RequestUrlHosts.FirstOrDefault();
            context.Request.Host = new HostString(urlHost ?? _localhost);
        }

        var pathBase = new PathString("/");
        if (!String.IsNullOrWhiteSpace(address?.PathBase))
        {
            pathBase = pathBase.Add(address.PathBase);
        }

        if (!String.IsNullOrWhiteSpace(settings.RequestUrlPrefix))
        {
            pathBase = pathBase.Add($"/{settings.RequestUrlPrefix}");
        }

        context.Items["IsBackground"] = true;
        context.Request.PathBase = pathBase;
        context.Request.Path = "/";

        return context;
    }
}
