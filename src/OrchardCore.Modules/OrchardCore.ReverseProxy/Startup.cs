using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Environment.Options;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.ReverseProxy.Configuration;
using OrchardCore.ReverseProxy.Drivers;
using OrchardCore.ReverseProxy.Services;
using OrchardCore.ReverseProxy.Settings;
using OrchardCore.Security.Permissions;
using OrchardCore.Settings.Deployment;

namespace OrchardCore.ReverseProxy;

public sealed class Startup : StartupBase
{
    public override int Order
        => OrchardCoreConstants.ConfigureOrder.ReverseProxy;

    public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
    {
        // The forwarded headers follow the reverse proxy settings, which are updated without releasing the tenant.
        app.UseMiddleware<ReverseProxyForwardedHeadersMiddleware>();
    }

    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddNavigationProvider<AdminMenu>();
        services.AddPermissionProvider<Permissions>();
        services.AddSiteDisplayDriver<ReverseProxySettingsDisplayDriver>();

        services.AddSignalOptionsChangeTokenSource<ReverseProxySettings>();
        services.AddSignalOptionsChangeTokenSource<ForwardedHeadersOptions>();
        services.AddTransient<IConfigureOptions<ReverseProxySettings>, ReverseProxySettingsConfiguration>();
        services.AddTransient<IConfigureOptions<ForwardedHeadersOptions>, ForwardedHeadersOptionsConfiguration>();
    }
}

[RequireFeatures("OrchardCore.Deployment")]
public sealed class DeploymentStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddSiteSettingsPropertyDeploymentStep<ReverseProxySettings, DeploymentStartup>(S => S["Reverse Proxy settings"], S => S["Exports the Reverse Proxy settings."]);
    }
}
