using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
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

namespace OrchardCore.GitHub;

public sealed class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddPermissionProvider<Permissions>();
    }
}

[Feature(GitHubConstants.Features.GitHubAuthentication)]
public sealed class GitHubLoginStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IGitHubAuthenticationService, GitHubAuthenticationService>();
        services.AddSiteDisplayDriver<GitHubAuthenticationSettingsDisplayDriver>();
        services.AddNavigationProvider<AdminMenuGitHubLogin>();
        services.AddRecipeExecutionStep<GitHubAuthenticationSettingsStep>();

        services.AddTransient<IConfigureOptions<GitHubAuthenticationSettings>, GitHubAuthenticationSettingsConfiguration>();

        // Register the options initializers required by the GitHub Handler.
        // Orchard-specific initializers:
        services.AddTransient<IConfigureOptions<AuthenticationOptions>, GitHubOptionsConfiguration>();
        services.AddTransient<IConfigureOptions<GitHubOptions>, GitHubOptionsConfiguration>();
        // Built-in initializers:
        services.AddTransient<IPostConfigureOptions<GitHubOptions>, OAuthPostConfigureOptions<GitHubOptions, GitHubHandler>>();
    }
}
