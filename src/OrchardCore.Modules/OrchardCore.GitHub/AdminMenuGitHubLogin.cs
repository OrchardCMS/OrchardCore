using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.GitHub;

public sealed class AdminMenuGitHubLogin : AdminNavigationProvider
{
    private static readonly RouteValueDictionary _routeValues = new()
    {
        { "area", "OrchardCore.Settings" },
        { "groupId", GitHubConstants.Features.GitHubAuthentication },
    };

    internal readonly IStringLocalizer S;

    public AdminMenuGitHubLogin(IStringLocalizer<AdminMenuGitHubLogin> stringLocalizer)
    {
        S = stringLocalizer;
    }

    protected override ValueTask BuildAsync(NavigationBuilder builder)
    {
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

        return ValueTask.CompletedTask;
    }
}
