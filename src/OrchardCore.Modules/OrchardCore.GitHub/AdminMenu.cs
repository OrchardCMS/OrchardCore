using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.Modules;
using OrchardCore.Navigation;

namespace OrchardCore.GitHub
{
    [Feature(GitHubConstants.Features.GitHubAuthentication)]
    public class AdminMenuGitHubLogin : INavigationProvider
    {
        private readonly IStringLocalizer S;

        public AdminMenuGitHubLogin(IStringLocalizer<AdminMenuGitHubLogin> localizer)
        {
            S = localizer;
        }

        public Task BuildNavigationAsync(string name, NavigationBuilder builder)
        {
            if (!String.Equals(name, "admin", StringComparison.OrdinalIgnoreCase))
            {
                return Task.CompletedTask;
            }

            builder
                .Add(S["Configuration"], configuration => configuration
                    .Add(S["Settings"], settings => settings
                        .Add(S["Security"], security => security.Id("security")
                            .Add(S["Authentication"], authentication => authentication
                                .Add(S["GitHub"], S["GitHub"].PrefixPosition(), github => github
                                    .AddClass("github").Id("github")
                                    .Action("Index", "Admin", new { area = "OrchardCore.Settings", groupId = GitHubConstants.Features.GitHubAuthentication })
                                    .Permission(Permissions.ManageGitHubAuthentication)
                                    .LocalNav()
                                )
                            )
                        )
                    )
                );

            return Task.CompletedTask;
        }
    }
}
