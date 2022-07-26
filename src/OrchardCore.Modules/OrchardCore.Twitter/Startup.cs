using System;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Twitter;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Recipes;
using OrchardCore.Security.Permissions;
using OrchardCore.Settings;
using OrchardCore.Twitter.Configuration;
using OrchardCore.Twitter.Drivers;
using OrchardCore.Twitter.Recipes;
using OrchardCore.Twitter.Services;
using Polly;

namespace OrchardCore.Twitter;

[Feature(TwitterConstants.Features.TwitterAuthentication)]
public class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<IPermissionProvider, Permissions>();
        services.AddScoped<INavigationProvider, AdminMenu>();
        services.AddSingleton<ITwitterAuthenticationService, TwitterAuthenticationService>();
        services.AddScoped<IDisplayDriver<ISite>, TwitterAuthenticationSettingsDisplayDriver>();

        // Register the options initializers required by the Twitter Handler.
        services.TryAddEnumerable(new[]
        {
            // Orchard-specific initializers:
            ServiceDescriptor.Transient<IConfigureOptions<AuthenticationOptions>, TwitterOptionsConfiguration>(),
            ServiceDescriptor.Transient<IConfigureOptions<TwitterOptions>, TwitterOptionsConfiguration>(),
            // Built-in initializers:
            ServiceDescriptor.Transient<IPostConfigureOptions<TwitterOptions>, TwitterPostConfigureOptions>()
        });

        services.AddRecipeExecutionStep<TwitterAuthenticationSettingsStep>();

        services.AddTransient<TwitterClientMessageHandler>();

        services.AddHttpClient<TwitterClient>()
            .AddHttpMessageHandler<TwitterClientMessageHandler>()
            .AddTransientHttpErrorPolicy(policy => policy.WaitAndRetryAsync(3, attempt => TimeSpan.FromSeconds(0.5 * attempt)));
    }
}
