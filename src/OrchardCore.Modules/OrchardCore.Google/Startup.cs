using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Google.Analytics;
using OrchardCore.Google.Analytics.Deployment;
using OrchardCore.Google.Analytics.Drivers;
using OrchardCore.Google.Analytics.Recipes;
using OrchardCore.Google.Analytics.Services;
using OrchardCore.Google.TagManager;
using OrchardCore.Google.TagManager.Drivers;
using OrchardCore.Google.TagManager.Services;
using OrchardCore.Google.Authentication.Configuration;
using OrchardCore.Google.Authentication.Drivers;
using OrchardCore.Google.Authentication.Recipes;
using OrchardCore.Google.Authentication.Services;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Recipes;
using OrchardCore.Security.Permissions;
using OrchardCore.Settings;
using OrchardCore.Settings.Deployment;
using OrchardCore.Google.TagManager.Settings;

namespace OrchardCore.Google
{
    [Feature(GoogleConstants.Features.GoogleAuthentication)]
    public class GoogleAuthenticationStartup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IPermissionProvider, Permissions.GoogleAuthentication>();
            services.AddRecipeExecutionStep<GoogleAuthenticationSettingsStep>();
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
        }
    }

    [Feature(GoogleConstants.Features.GoogleAnalytics)]
    public class GoogleAnalyticsStartup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IPermissionProvider, Permissions.GoogleAnalytics>();
            services.AddSingleton<IGoogleAnalyticsService, GoogleAnalyticsService>();
            services.AddRecipeExecutionStep<GoogleAnalyticsSettingsStep>();

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

    [RequireFeatures("OrchardCore.Deployment")]
    public class DeploymentStartup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IDisplayDriver<DeploymentStep>, GoogleAnalyticsDeploymentStepDriver>();
            services.AddTransient<IDeploymentSource, GoogleAnalyticsDeploymentSource>();
            services.AddSingleton<IDeploymentStepFactory, DeploymentStepFactory<GoogleAnalyticsDeploymentStep>>();

            services.AddTransient<IDeploymentSource, SiteSettingsPropertyDeploymentSource<GoogleTagManagerSettings>>();
            services.AddScoped<IDisplayDriver<DeploymentStep>>(sp =>
            {
                var S = sp.GetService<IStringLocalizer<DeploymentStartup>>();
                return new SiteSettingsPropertyDeploymentStepDriver<GoogleTagManagerSettings>(S["Google Tag Manager Settings"], S["Exports the Google Tag Manager settings."]);
            });
            services.AddSingleton<IDeploymentStepFactory>(new SiteSettingsPropertyDeploymentStepFactory<GoogleTagManagerSettings>());
        }
    }
}
