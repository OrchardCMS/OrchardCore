using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;
using OrchardCore.Settings;

namespace OrchardCore.GitHub;

public sealed class AdminSettingsMenuGitHubLogin : SettingsNavigationProvider
{
    internal readonly IStringLocalizer S;

    public AdminSettingsMenuGitHubLogin(IStringLocalizer<AdminSettingsMenuGitHubLogin> stringLocalizer)
    {
        S = stringLocalizer;
    }

    protected override ValueTask BuildAsync(NavigationBuilder builder)
    {
        builder
            .Add(S["Authentication"], authentication => authentication
                .Add(S["GitHub"], S["GitHub"].PrefixPosition(), gitHub => gitHub
                    .AddClass("github")
                    .Id("github")
                    .Action(GetRouteValues(GitHubConstants.Features.GitHubAuthentication))
                    .Permission(Permissions.ManageGitHubAuthentication)
                    .LocalNav()
                )
            );

        return ValueTask.CompletedTask;
    }
}
