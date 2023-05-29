using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
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
using OrchardCore.Settings;
using OrchardCore.Settings.Deployment;

namespace OrchardCore.Google
{
    [Feature(GoogleConstants.Features.GoogleAuthentication)]
    public class GoogleAuthenticationStartup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IPermissionProvider, Permissions.GoogleAuthentication>();
            services.AddSingleton<GoogleAuthenticationService, GoogleAuthenticationService>();
            services.AddScoped<IDisplayDriver<ISite>, GoogleAuthenticationSettingsDisplayDriver>();
            services.AddScoped<INavigationProvider, GoogleAuthenticationAdminMenu>();
            // Register the options initializers required by the Google Handler.
            services.TryAddEnumerable(new[]
            {
                // Orchard-specific initializers:
                ServiceDescriptor.Transient<IConfigureOptions<AuthenticationOptions>, GoogleOptionsConfiguration>(),
                ServiceDescriptor.Transient<IConfigureOptions<GoogleOptions>, GoogleOptionsConfiguration>(),
                // Built-in initializers:
                ServiceDescriptor.Transient<IPostConfigureOptions<GoogleOptions>, OAuthPostConfigureOptions<GoogleOptions,GoogleHandler>>()
            });

            services.AddTransient<IConfigureOptions<GoogleAuthenticationSettings>, GoogleAuthenticationSettingsConfiguration>();
        }
    }

    [Feature(GoogleConstants.Features.GoogleAnalytics)]
    public class GoogleAnalyticsStartup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IPermissionProvider, Permissions.GoogleAnalytics>();
            services.AddSingleton<IGoogleAnalyticsService, GoogleAnalyticsService>();

            services.AddScoped<IDisplayDriver<ISite>, GoogleAnalyticsSettingsDisplayDriver>();
            services.AddScoped<INavigationProvider, GoogleAnalyticsAdminMenu>();
            services.Configure<MvcOptions>((options) =>
            {
                options.Filters.Add(typeof(GoogleAnalyticsFilter));
            });
        }
    }

    [Feature(GoogleConstants.Features.GoogleTagManager)]
    public class GoogleTagManagerStartup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IPermissionProvider, Permissions.GoogleTagManager>();
            services.AddSingleton<IGoogleTagManagerService, GoogleTagManagerService>();

            services.AddScoped<IDisplayDriver<ISite>, GoogleTagManagerSettingsDisplayDriver>();
            services.AddScoped<INavigationProvider, GoogleTagManagerAdminMenu>();
            services.Configure<MvcOptions>((options) =>
            {
                options.Filters.Add(typeof(GoogleTagManagerFilter));
            });
        }
    }

    [Feature(GoogleConstants.Features.GoogleAuthentication)]
    [RequireFeatures("OrchardCore.Deployment")]
    public class GoogleAuthenticationDeploymentStartup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddSiteSettingsPropertyDeploymentStep<GoogleAuthenticationSettings, GoogleAuthenticationDeploymentStartup>(S => S["Google Authentication Settings"], S => S["Exports the Google Authentication settings."]);
        }
    }

    [Feature(GoogleConstants.Features.GoogleAnalytics)]
    [RequireFeatures("OrchardCore.Deployment")]
    public class GoogleAnalyticsDeploymentStartup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddSiteSettingsPropertyDeploymentStep<GoogleAnalyticsSettings, GoogleAnalyticsDeploymentStartup>(S => S["Google Analytics Settings"], S => S["Exports the Google Analytics settings."]);
        }
    }

    [Feature(GoogleConstants.Features.GoogleTagManager)]
    [RequireFeatures("OrchardCore.Deployment")]
    public class GoogleTagManagerDeploymentStartup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddSiteSettingsPropertyDeploymentStep<GoogleTagManagerSettings, GoogleTagManagerDeploymentStartup>(S => S["Google Tag Manager Settings"], S => S["Exports the Google Tag Manager settings."]);
        }
    }
}
