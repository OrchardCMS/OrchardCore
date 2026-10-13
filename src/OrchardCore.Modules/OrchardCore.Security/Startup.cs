using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Environment.Options;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Security.Drivers;
using OrchardCore.Security.Permissions;
using OrchardCore.Security.Services;
using OrchardCore.Security.Settings;
using OrchardCore.Settings.Deployment;

namespace OrchardCore.Security;

public sealed class Startup : StartupBase
{
    public override int Order
        => OrchardCoreConstants.ConfigureOrder.Security;

    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddPermissionProvider<SecurityPermissions>();
        services.AddSiteDisplayDriver<SecuritySettingsDisplayDriver>();
        services.AddNavigationProvider<AdminMenu>();
        services.AddSingleton<ISecurityService, SecurityService>();

        services.AddSignalOptionsChangeTokenSource<SecuritySettings>();
        services.AddTransient<IConfigureOptions<SecuritySettings>, SecuritySettingsConfiguration>();
    }

    public override void Configure(IApplicationBuilder builder, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
    {
        // The headers follow the security settings, which are updated without releasing the tenant.
        builder.UseMiddleware<SecuritySettingsHeadersMiddleware>();
    }
}

[RequireFeatures("OrchardCore.Deployment")]
public sealed class SecurityDeploymentStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddSiteSettingsPropertyDeploymentStep<SecuritySettings, SecurityDeploymentStartup>(
            S => S["Security settings"],
            S => S["Exports the Security settings."]);
    }
}
