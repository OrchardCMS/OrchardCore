using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Navigation;
using OrchardCore.Github.Configuration;
using OrchardCore.Github.Drivers;
using OrchardCore.Github.Services;
using OrchardCore.Modules;
using OrchardCore.Security.Permissions;
using OrchardCore.Settings;

namespace OrchardCore.Github
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IPermissionProvider, Permissions>();
        }
    }

    [Feature(GithubConstants.Features.GithubAuthentication)]
    public class GithubLoginStartup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IGithubAuthenticationService, GithubAuthenticationService>();
            services.AddScoped<IDisplayDriver<ISite>, GithubAuthenticationSettingsDisplayDriver>();
            services.AddScoped<INavigationProvider, AdminMenuGithubLogin>();
            // Register the options initializers required by the Github Handler.
            services.TryAddEnumerable(new[]
            {
                // Orchard-specific initializers:
                ServiceDescriptor.Transient<IConfigureOptions<AuthenticationOptions>, GithubOptionsConfiguration>(),
                ServiceDescriptor.Transient<IConfigureOptions<GithubOptions>, GithubOptionsConfiguration>(),
                // Built-in initializers:
                ServiceDescriptor.Transient<IPostConfigureOptions<GithubOptions>, OAuthPostConfigureOptions<GithubOptions,GithubHandler>>()
            });
        }
    }
}
