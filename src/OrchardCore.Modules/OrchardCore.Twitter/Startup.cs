using System;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Twitter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Recipes;
using OrchardCore.Security.Permissions;
using OrchardCore.Settings;
using OrchardCore.Settings.Deployment;
using OrchardCore.Twitter.Drivers;
using OrchardCore.Twitter.Recipes;
using OrchardCore.Twitter.Services;
using OrchardCore.Twitter.Settings;
using OrchardCore.Twitter.Signin.Configuration;
using OrchardCore.Twitter.Signin.Drivers;
using OrchardCore.Twitter.Signin.Services;
using OrchardCore.Twitter.Signin.Settings;
using Polly;

namespace OrchardCore.Twitter
{
    public class ModuleStartup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IPermissionProvider, Permissions>();
        }
    }

    [Feature(TwitterConstants.Features.Twitter)]
    public class TwitterStartup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IDisplayDriver<ISite>, TwitterSettingsDisplayDriver>();
            services.AddScoped<INavigationProvider, AdminMenu>();
            services.AddSingleton<ITwitterSettingsService, TwitterSettingsService>();

            services.AddRecipeExecutionStep<TwitterSettingsStep>();

            services.AddTransient<TwitterClientMessageHandler>();

            services.AddHttpClient<TwitterClient>()
                .AddHttpMessageHandler<TwitterClientMessageHandler>()
                .AddTransientHttpErrorPolicy(policy => policy.WaitAndRetryAsync(3, attempt => TimeSpan.FromSeconds(0.5 * attempt)));
        }

        public override void Configure(IApplicationBuilder builder, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        {
        }
    }

    [Feature(TwitterConstants.Features.Twitter)]
    [RequireFeatures("OrchardCore.Deployment")]
    public class TwitterDeploymentStartup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<IDeploymentSource, SiteSettingsPropertyDeploymentSource<TwitterSettings>>();
            services.AddScoped<IDisplayDriver<DeploymentStep>>(sp =>
            {
                var S = sp.GetService<IStringLocalizer<TwitterDeploymentStartup>>();
                return new SiteSettingsPropertyDeploymentStepDriver<TwitterSettings>(S["Twitter settings"], S["Exports the Twitter settings."]);
            });
            services.AddSingleton<IDeploymentStepFactory>(new SiteSettingsPropertyDeploymentStepFactory<TwitterSettings>());
        }
    }

    [Feature(TwitterConstants.Features.Signin)]
    public class TwitterSigninStartup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<INavigationProvider, AdminMenuSignin>();
            services.AddSingleton<ITwitterSigninService, TwitterSigninService>();
            services.AddScoped<IDisplayDriver<ISite>, TwitterSigninSettingsDisplayDriver>();
            // Register the options initializers required by the Twitter Handler.
            services.TryAddEnumerable(new[]
            {
                // Orchard-specific initializers:
                ServiceDescriptor.Transient<IConfigureOptions<AuthenticationOptions>, TwitterOptionsConfiguration>(),
                ServiceDescriptor.Transient<IConfigureOptions<TwitterOptions>, TwitterOptionsConfiguration>(),
                // Built-in initializers:
                ServiceDescriptor.Transient<IPostConfigureOptions<TwitterOptions>, TwitterPostConfigureOptions>()
            });
        }
    }

    [Feature(TwitterConstants.Features.Signin)]
    [RequireFeatures("OrchardCore.Deployment")]
    public class TwitterSigninDeploymentStartup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<IDeploymentSource, SiteSettingsPropertyDeploymentSource<TwitterSigninSettings>>();
            services.AddScoped<IDisplayDriver<DeploymentStep>>(sp =>
            {
                var S = sp.GetService<IStringLocalizer<TwitterSigninDeploymentStartup>>();
                return new SiteSettingsPropertyDeploymentStepDriver<TwitterSigninSettings>(S["Twitter Signin settings"], S["Exports the Twitter Signin settings."]);
            });
            services.AddSingleton<IDeploymentStepFactory>(new SiteSettingsPropertyDeploymentStepFactory<TwitterSigninSettings>());
        }
    }
}
