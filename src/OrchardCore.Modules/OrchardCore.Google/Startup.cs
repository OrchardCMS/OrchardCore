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
using OrchardCore.Google.Analytics.Drivers;
using OrchardCore.Google.Analytics.Recipes;
using OrchardCore.Google.Analytics.Settings;
using OrchardCore.Google.Authentication.Configuration;
using OrchardCore.Google.Authentication.Drivers;
using OrchardCore.Google.Authentication.Recipes;
using OrchardCore.Google.Authentication.Services;
using OrchardCore.Google.Authentication.Settings;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Recipes;
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

    [Feature(GoogleConstants.Features.GoogleAuthentication)]
    [RequireFeatures("OrchardCore.Deployment")]
    public class DeploymentStartup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<IDeploymentSource, SiteSettingsPropertyDeploymentSource<GoogleAuthenticationSettings>>();
            services.AddScoped<IDisplayDriver<DeploymentStep>>(sp =>
            {
                var S = sp.GetService<IStringLocalizer<GoogleAuthenticationStartup>>();
                return new SiteSettingsPropertyDeploymentStepDriver<GoogleAuthenticationSettings>(S["Google Authentication settings"], S["Exports the Google Authentication settings."]);
            });
            services.AddSingleton<IDeploymentStepFactory>(new SiteSettingsPropertyDeploymentStepFactory<GoogleAuthenticationSettings>());
        }
    }

    [Feature(GoogleConstants.Features.GoogleAnalytics)]
    public class GoogleAnalyticsStartup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IPermissionProvider, Permissions.GoogleAnalytics>();
            services.AddRecipeExecutionStep<GoogleAnalyticsSettingsStep>();
            services.AddScoped<IDisplayDriver<ISite>, GoogleAnalyticsSettingsDisplayDriver>();
            services.AddScoped<INavigationProvider, GoogleAnalyticsAdminMenu>();
            services.Configure<MvcOptions>((options) =>
            {
                options.Filters.Add(typeof(GoogleAnalyticsFilter));
            });
        }
    }

    [Feature(GoogleConstants.Features.GoogleAnalytics)]
    [RequireFeatures("OrchardCore.Deployment")]
    public class GoogleAnalyticsDeploymentStartup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<IDeploymentSource, SiteSettingsPropertyDeploymentSource<GoogleAnalyticsSettings>>();
            services.AddScoped<IDisplayDriver<DeploymentStep>>(sp =>
            {
                var S = sp.GetService<IStringLocalizer<GoogleAnalyticsDeploymentStartup>>();
                return new SiteSettingsPropertyDeploymentStepDriver<GoogleAnalyticsSettings>(S["Google Analytics settings"], S["Exports the Google Analytics settings."]);
            });
            services.AddSingleton<IDeploymentStepFactory>(new SiteSettingsPropertyDeploymentStepFactory<GoogleAnalyticsSettings>());
        }
    }
}
