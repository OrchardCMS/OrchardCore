using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Google.Analytics;
using OrchardCore.Google.Analytics.Drivers;
using OrchardCore.Google.Analytics.Services;
using OrchardCore.Google.Analytics.Settings;
using OrchardCore.Google.Authentication.Configuration;
using OrchardCore.Google.Authentication.Drivers;
using OrchardCore.Google.Authentication.Services;
using OrchardCore.Google.Authentication.Settings;
using OrchardCore.Google.TagManager;
using OrchardCore.Google.TagManager.Drivers;
using OrchardCore.Google.TagManager.Services;
using OrchardCore.Google.TagManager.Settings;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Security.Permissions;
using OrchardCore.Settings.Deployment;

namespace OrchardCore.Google;

[Feature(GoogleConstants.Features.GoogleAuthentication)]
public sealed class GoogleAuthenticationStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddPermissionProvider<GoogleAuthenticationPermissionProvider>();
        services.AddSingleton<GoogleAuthenticationService, GoogleAuthenticationService>();
        services.AddSiteDisplayDriver<GoogleAuthenticationSettingsDisplayDriver>();
        services.AddNavigationProvider<GoogleAuthenticationAdminMenu>();

        // Register the options initializers required by the Google Handler.
        // Orchard-specific initializers:
        services.AddTransient<IConfigureOptions<AuthenticationOptions>, GoogleOptionsConfiguration>();
        services.AddTransient<IConfigureOptions<GoogleOptions>, GoogleOptionsConfiguration>();

        // Built-in initializers:
        services.AddTransient<IPostConfigureOptions<GoogleOptions>, OAuthPostConfigureOptions<GoogleOptions, GoogleHandler>>();

        services.AddTransient<IConfigureOptions<GoogleAuthenticationSettings>, GoogleAuthenticationSettingsConfiguration>();
    }
}

[Feature(GoogleConstants.Features.GoogleAnalytics)]
public sealed class GoogleAnalyticsStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddPermissionProvider<GoogleAnalyticsPermissionsProvider>();
        services.AddSingleton<IGoogleAnalyticsService, GoogleAnalyticsService>();

        services.AddSiteDisplayDriver<GoogleAnalyticsSettingsDisplayDriver>();
        services.AddNavigationProvider<GoogleAnalyticsAdminMenu>();

        services.Configure<MvcOptions>((options) =>
        {
            options.Filters.Add<GoogleAnalyticsFilter>();
        });
    }
}

[Feature(GoogleConstants.Features.GoogleTagManager)]
public sealed class GoogleTagManagerStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddPermissionProvider<GoogleTagManagerPermissionProvider>();
        services.AddSingleton<IGoogleTagManagerService, GoogleTagManagerService>();
        services.AddSiteDisplayDriver<GoogleTagManagerSettingsDisplayDriver>();
        services.AddNavigationProvider<GoogleTagManagerAdminMenu>();

        services.Configure<MvcOptions>((options) =>
        {
            options.Filters.Add<GoogleTagManagerFilter>();
        });
    }
}

[Feature(GoogleConstants.Features.GoogleAuthentication)]
[RequireFeatures("OrchardCore.Deployment")]
public sealed class GoogleAuthenticationDeploymentStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddSiteSettingsPropertyDeploymentStep<GoogleAuthenticationSettings, GoogleAuthenticationDeploymentStartup>(S => S["Google Authentication Settings"], S => S["Exports the Google Authentication settings."]);
    }
}

[Feature(GoogleConstants.Features.GoogleAnalytics)]
[RequireFeatures("OrchardCore.Deployment")]
public sealed class GoogleAnalyticsDeploymentStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddSiteSettingsPropertyDeploymentStep<GoogleAnalyticsSettings, GoogleAnalyticsDeploymentStartup>(S => S["Google Analytics Settings"], S => S["Exports the Google Analytics settings."]);
    }
}

[Feature(GoogleConstants.Features.GoogleTagManager)]
[RequireFeatures("OrchardCore.Deployment")]
public sealed class GoogleTagManagerDeploymentStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddSiteSettingsPropertyDeploymentStep<GoogleTagManagerSettings, GoogleTagManagerDeploymentStartup>(S => S["Google Tag Manager Settings"], S => S["Exports the Google Tag Manager settings."]);
    }
}
