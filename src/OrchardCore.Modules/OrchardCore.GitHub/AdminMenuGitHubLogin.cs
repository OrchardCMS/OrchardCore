using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.GitHub;

public sealed class AdminMenuGitHubLogin : INavigationProvider
{
    private static readonly RouteValueDictionary _routeValues = new()
    {
        { "area", "OrchardCore.Settings" },
        { "groupId", GitHubConstants.Features.GitHubAuthentication },
    };

    internal readonly IStringLocalizer S;

    public AdminMenuGitHubLogin(IStringLocalizer<AdminMenuGitHubLogin> localizer)
    {
        S = localizer;
    }

    public Task BuildNavigationAsync(string name, NavigationBuilder builder)
    {
        if (!NavigationHelper.IsAdminMenu(name))
        {
            return Task.CompletedTask;
        }

        builder
            .Add(S["Security"], security => security
                .Add(S["Authentication"], authentication => authentication
                    .Add(S["GitHub"], S["GitHub"].PrefixPosition(), settings => settings
                        .AddClass("github")
                        .Id("github")
                        .Action("Index", "Admin", _routeValues)
                        .Permission(Permissions.ManageGitHubAuthentication)
                        .LocalNav()
                    )
                )
            );

        return Task.CompletedTask;
    }
}
