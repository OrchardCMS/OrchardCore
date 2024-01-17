using System;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Twitter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Recipes;
using OrchardCore.Security.Permissions;
using OrchardCore.Settings;
using OrchardCore.Twitter.Drivers;
using OrchardCore.Twitter.Recipes;
using OrchardCore.Twitter.Services;
using OrchardCore.Twitter.Settings;
using OrchardCore.Twitter.Signin.Configuration;
using OrchardCore.Twitter.Signin.Drivers;
using OrchardCore.Twitter.Signin.Services;
using Polly;

namespace OrchardCore.Twitter
{
    public class Startup : StartupBase
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
            services.AddTransient<IConfigureOptions<TwitterSettings>, TwitterSettingsConfiguration>();

            services.AddHttpClient<TwitterClient>()
                .AddHttpMessageHandler<TwitterClientMessageHandler>()
                .AddTransientHttpErrorPolicy(policy => policy.WaitAndRetryAsync(3, attempt => TimeSpan.FromSeconds(0.5 * attempt)));
        }

        public override void Configure(IApplicationBuilder builder, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        {
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
}
