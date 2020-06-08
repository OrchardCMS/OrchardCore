using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Facebook;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Facebook.Login.Configuration;
using OrchardCore.Facebook.Login.Drivers;
using OrchardCore.Facebook.Login.Services;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Settings;

namespace OrchardCore.Facebook
{
    [Feature(FacebookConstants.Features.Login)]
    public class StartupLogin : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IFacebookLoginService, FacebookLoginService>();
            services.AddScoped<IDisplayDriver<ISite>, FacebookLoginSettingsDisplayDriver>();
            services.AddScoped<INavigationProvider, AdminMenuLogin>();

            // Register the options initializers required by the Facebook handler.
            services.TryAddEnumerable(new[]
            {
                // Orchard-specific initializers:
                ServiceDescriptor.Transient<IConfigureOptions<AuthenticationOptions>, FacebookLoginConfiguration>(),
                ServiceDescriptor.Transient<IConfigureOptions<FacebookOptions>, FacebookLoginConfiguration>(),

                // Built-in initializers:
                ServiceDescriptor.Transient<IPostConfigureOptions<FacebookOptions>, OAuthPostConfigureOptions<FacebookOptions,FacebookHandler>>()
            });
        }
    }
}
