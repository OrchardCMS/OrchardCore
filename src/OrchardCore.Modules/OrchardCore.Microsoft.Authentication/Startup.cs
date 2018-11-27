using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.MicrosoftAccount;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Navigation;
using OrchardCore.Microsoft.Authentication.Configuration;
using OrchardCore.Microsoft.Authentication.Drivers;
using OrchardCore.Microsoft.Authentication.Services;
using OrchardCore.Modules;
using OrchardCore.Security.Permissions;
using OrchardCore.Settings;

namespace OrchardCore.Microsoft.Authentication
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IPermissionProvider, Permissions>();
            services.AddScoped<INavigationProvider, AdminMenu>();

            //services.AddSingleton<IAzureADAuthenticationService, AzureADAuthenticationService>();
            //services.AddScoped<IDisplayDriver<ISite>, AzureADAuthenticationSettingsDisplayDriver>();
        }
    }

    [Feature(MicrosoftAuthenticationConstants.Features.MicrosoftAccount)]
    public class LoginStartup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IMicrosoftAuthenticationService, MicrosoftAuthenticationService>();
            services.AddScoped<IDisplayDriver<ISite>, MicrosoftAuthenticationSettingsDisplayDriver>();
            services.AddScoped<INavigationProvider, AdminMenuLogin>();

            // Register the options initializers required by the OpenID Connect client handler.
            services.TryAddEnumerable(new[]
            {
                // Orchard-specific initializers:
                //ServiceDescriptor.Transient<IConfigureOptions<AuthenticationOptions>, MicrosoftAuthenticationConfiguration>(),
                ServiceDescriptor.Transient<IConfigureOptions<MicrosoftAccountOptions>, MicrosoftAuthenticationConfiguration>(),

                // Built-in initializers:
                ServiceDescriptor.Transient<IPostConfigureOptions<MicrosoftAccountOptions>, OAuthPostConfigureOptions<MicrosoftAccountOptions,MicrosoftAccountHandler>>()
            });
        }
    }
}
