using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.Modules;
using OrchardCore.Navigation;

namespace OrchardCore.Facebook
{
    public class AdminMenu : INavigationProvider
    {
        private readonly IStringLocalizer S;

        public AdminMenu(IStringLocalizer<AdminMenu> localizer)
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
                        .Add(S["Facebook app"], S["Facebook app"].PrefixPosition(), app => app
                            .AddClass("facebookApp").Id("facebookApp")
                            .Action("Index", "Admin", new { area = "OrchardCore.Settings", groupId = FacebookConstants.Features.Core })
                            .Permission(Permissions.ManageFacebookApp)
                            .LocalNav()
                        )
                    )
                );

            return Task.CompletedTask;
        }
    }

    [Feature(FacebookConstants.Features.Login)]
    public class AdminMenuLogin : INavigationProvider
    {
        private readonly IStringLocalizer S;

        public AdminMenuLogin(IStringLocalizer<AdminMenuLogin> localizer)
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
                                .Add(S["Facebook"], S["Facebook"].PrefixPosition(), facebook => facebook
                                    .AddClass("facebook").Id("facebook")
                                    .Action("Index", "Admin", new { area = "OrchardCore.Settings", groupId = FacebookConstants.Features.Login })
                                    .Permission(Permissions.ManageFacebookApp)
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
