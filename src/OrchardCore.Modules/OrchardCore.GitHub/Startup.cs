using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.GitHub.Configuration;
using OrchardCore.GitHub.Drivers;
using OrchardCore.GitHub.Recipes;
using OrchardCore.GitHub.Services;
using OrchardCore.GitHub.Settings;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Recipes;
using OrchardCore.Security.Permissions;
using OrchardCore.Settings;

namespace OrchardCore.GitHub
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IPermissionProvider, Permissions>();
        }
    }

    [Feature(GitHubConstants.Features.GitHubAuthentication)]
    public class GitHubLoginStartup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IGitHubAuthenticationService, GitHubAuthenticationService>();
            services.AddScoped<IDisplayDriver<ISite>, GitHubAuthenticationSettingsDisplayDriver>();
            services.AddScoped<INavigationProvider, AdminMenuGitHubLogin>();
            services.AddRecipeExecutionStep<GitHubAuthenticationSettingsStep>();

            services.AddTransient<IConfigureOptions<GitHubAuthenticationSettings>, GitHubAuthenticationSettingsConfiguration>();

            // Register the options initializers required by the GitHub Handler.
            services.TryAddEnumerable(new[]
            {
                // Orchard-specific initializers:
                ServiceDescriptor.Transient<IConfigureOptions<AuthenticationOptions>, GitHubOptionsConfiguration>(),
                ServiceDescriptor.Transient<IConfigureOptions<GitHubOptions>, GitHubOptionsConfiguration>(),
                // Built-in initializers:
                ServiceDescriptor.Transient<IPostConfigureOptions<GitHubOptions>, OAuthPostConfigureOptions<GitHubOptions,GitHubHandler>>()
            });
        }
    }
}
