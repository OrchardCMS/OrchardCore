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
#pragma warning disable IDE1006 // Naming Styles
        private readonly IStringLocalizer S;
#pragma warning restore IDE1006 // Naming Styles

        public AdminMenuGitHubLogin(IStringLocalizer<AdminMenuGitHubLogin> localizer)
        {
            S = localizer;
        }

        public Task BuildNavigationAsync(string name, NavigationBuilder builder)
        {
            if (String.Equals(name, "admin", StringComparison.OrdinalIgnoreCase))
            {
                builder.Add(S["Security"], security => security
                        .Add(S["Authentication"], authentication => authentication
                        .Add(S["GitHub"], S["GitHub"].PrefixPosition(), settings => settings
                        .AddClass("github").Id("github")
                            .Action("Index", "Admin", new { area = "OrchardCore.Settings", groupId = GitHubConstants.Features.GitHubAuthentication })
                            .Permission(Permissions.ManageGitHubAuthentication)
                            .LocalNav())
                    ));
            }
            return Task.CompletedTask;
        }
    }
}
