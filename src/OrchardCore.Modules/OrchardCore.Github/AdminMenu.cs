using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.Environment.Shell.Descriptor.Models;
using OrchardCore.Modules;
using OrchardCore.Navigation;

namespace OrchardCore.Github
{
    [Feature(GithubConstants.Features.GithubAuthentication)]
    public class AdminMenuGithubLogin : INavigationProvider
    {
        private readonly ShellDescriptor _shellDescriptor;

        public AdminMenuGithubLogin(
            IStringLocalizer<AdminMenuGithubLogin> localizer,
            ShellDescriptor shellDescriptor)
        {
            T = localizer;
            _shellDescriptor = shellDescriptor;
        }

        public IStringLocalizer T { get; set; }

        public Task BuildNavigationAsync(string name, NavigationBuilder builder)
        {
            if (String.Equals(name, "admin", StringComparison.OrdinalIgnoreCase))
            {
                builder.Add(T["Github"], "15", settings => settings
                        .AddClass("github").Id("github")
                        .Add(T["Github Authentication"], "10", client => client
                            .Action("Index", "Admin", new { area = "OrchardCore.Settings", groupId = GithubConstants.Features.GithubAuthentication })
                            .Permission(Permissions.ManageGithubAuthentication)
                            .LocalNav())
                    );
            }
            return Task.CompletedTask;
        }
    }
}
