using AspNet.Security.OAuth.GitHub;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.GitHub.Configuration;
using OrchardCore.GitHub.Drivers;
using OrchardCore.GitHub.Recipes;
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
        services.AddSiteDisplayDriver<GitHubAuthenticationSettingsDisplayDriver>();
        services.AddNavigationProvider<AdminMenuGitHubLogin>();

        // Register the options initializers required by the GitHub Handler.
        services.AddTransient<IConfigureOptions<AuthenticationOptions>, GitHubAuthenticationOptionsConfiguration>();

        services.AddTransient<IConfigureOptions<GitHubAuthenticationOptions>, GitHubAuthenticationOptionsConfiguration>();

        // Built-in initializers:
        services.AddTransient<IPostConfigureOptions<GitHubAuthenticationOptions>, OAuthPostConfigureOptions<GitHubAuthenticationOptions, GitHubAuthenticationHandler>>();
    }
}


[Feature(GitHubConstants.Features.GitHubAuthentication)]
[RequireFeatures("OrchardCore.Recipes.Core")]
public sealed class RecipesStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddRecipeExecutionStep<GitHubAuthenticationSettingsStep>();
    }
}
