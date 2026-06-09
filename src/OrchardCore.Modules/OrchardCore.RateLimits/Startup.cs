using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.RateLimits.Drivers;
using OrchardCore.RateLimits.Settings;
using OrchardCore.Security.Permissions;
using OrchardCore.Settings.Deployment;

namespace OrchardCore.RateLimits;

public sealed class Startup : StartupBase
{
    private readonly IShellConfiguration _shellConfiguration;

    public Startup(IShellConfiguration shellConfiguration)
    {
        _shellConfiguration = shellConfiguration;
    }

    public override int ConfigureOrder => OrchardCoreConstants.ConfigureOrder.RateLimiter;

    public override void ConfigureServices(IServiceCollection services)
    {
        services.Configure<GlobalRateLimitOptions>(_shellConfiguration.GetSection("RateLimits:Global"));
        services.AddRateLimiter();
        services.AddTransient<IConfigureOptions<RateLimiterOptions>, RateLimiterOptionsConfigurations>();
        services.AddSiteDisplayDriver<RateLimitsSettingsDisplayDriver>();
        services.AddNavigationProvider<AdminMenu>();
        services.AddPermissionProvider<Permissions>();
    }

    public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
    {
        app.UseRateLimiter();
    }
}

[RequireFeatures("OrchardCore.Deployment")]
public sealed class DeploymentStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddSiteSettingsPropertyDeploymentStep<RateLimitsSettings, DeploymentStartup>(
            S => S["Rate Limits settings"],
            S => S["Exports the Rate Limits settings."]);
    }
}
