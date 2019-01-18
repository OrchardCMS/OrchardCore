using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Navigation;
using OrchardCore.Twitter.Configuration;
using OrchardCore.Twitter.Drivers;
using OrchardCore.Twitter.Services;
using OrchardCore.Modules;
using OrchardCore.Security.Permissions;
using OrchardCore.Settings;
using Microsoft.AspNetCore.Authentication.Twitter;

namespace OrchardCore.Twitter
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IPermissionProvider, Permissions>();
        }
    }

    [Feature(TwitterConstants.Features.TwitterSignin)]
    public class TwitterLoginStartup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<ITwitterSigninService, TwitterSigninService>();
            services.AddScoped<IDisplayDriver<ISite>, TwitterSigninSettingsDisplayDriver>();
            services.AddScoped<INavigationProvider, AdminMenuTwitterLogin>();
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
