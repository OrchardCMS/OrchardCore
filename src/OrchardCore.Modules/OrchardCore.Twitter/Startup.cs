using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Navigation;
using OrchardCore.Twitter.Signin.Configuration;
using OrchardCore.Twitter.Drivers;
using OrchardCore.Twitter.Services;
using OrchardCore.Modules;
using OrchardCore.Security.Permissions;
using OrchardCore.Settings;
using Microsoft.AspNetCore.Authentication.Twitter;
using OrchardCore.Twitter.Signin.Services;
using OrchardCore.Twitter.Signin.Drivers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using System;
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

            services.AddTransient<TwitterClientMessageHandler>();

            services.AddHttpClient<TwitterClient>()
                .AddHttpMessageHandler<TwitterClientMessageHandler>()
                .AddTransientHttpErrorPolicy(policy => policy.WaitAndRetryAsync(3, attempt => TimeSpan.FromSeconds(0.5 * attempt)));
        }

        public override void Configure(IApplicationBuilder app, IRouteBuilder routes, IServiceProvider serviceProvider)
        {
            base.Configure(app, routes, serviceProvider);
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
