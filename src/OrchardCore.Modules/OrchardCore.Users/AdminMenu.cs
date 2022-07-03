using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Users.Drivers;
using OrchardCore.Users.Models;

namespace OrchardCore.Users
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
                .Add(S["Security"], NavigationConstants.AdminMenuSecurityPosition, security => security
                    .AddClass("security").Id("security")
                    .Add(S["Users"], S["Users"].PrefixPosition(), users => users
                        .AddClass("users").Id("users")
                        .Action("Index", "Admin", "OrchardCore.Users")
                        .Permission(Permissions.ViewUsers)
                        .Resource(new User())
                        .LocalNav()
                    )
                )
                .Add(S["Configuration"], design => design
                    .Add(S["Settings"], settings => settings
                        .Add(S["Security"], security => security.Id("security")
                            .Add(S["User login"], S["User login"].PrefixPosition(), login => login
                                .Permission(Permissions.ManageUsers)
                                .Action("Index", "Admin", new { area = "OrchardCore.Settings", groupId = LoginSettingsDisplayDriver.GroupId })
                                .LocalNav()
                            )
                        )
                    )
                );

            return Task.CompletedTask;
        }
    }

    [Feature("OrchardCore.Users.ChangeEmail")]
    public class ChangeEmailAdminMenu : INavigationProvider
    {
        private readonly IStringLocalizer S;

        public ChangeEmailAdminMenu(IStringLocalizer<ChangeEmailAdminMenu> localizer)
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
                            .Add(S["User change email"], S["User change email"].PrefixPosition(), changeEmail => changeEmail
                                .Permission(Permissions.ManageUsers)
                                .Action("Index", "Admin", new { area = "OrchardCore.Settings", groupId = ChangeEmailSettingsDisplayDriver.GroupId })
                                .LocalNav()
                            )
                        )
                    )
               );

            return Task.CompletedTask;
        }
    }

    [Feature("OrchardCore.Users.Registration")]
    public class RegistrationAdminMenu : INavigationProvider
    {
        private readonly IStringLocalizer S;

        public RegistrationAdminMenu(IStringLocalizer<RegistrationAdminMenu> localizer)
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
                            .Add(S["User registration"], S["User registration"].PrefixPosition(), registration => registration
                                .Permission(Permissions.ManageUsers)
                                .Action("Index", "Admin", new { area = "OrchardCore.Settings", groupId = RegistrationSettingsDisplayDriver.GroupId })
                                .LocalNav()
                            )
                        )
                    )
                );

            return Task.CompletedTask;
        }
    }

    [Feature("OrchardCore.Users.ResetPassword")]
    public class ResetPasswordAdminMenu : INavigationProvider
    {
        private readonly IStringLocalizer S;

        public ResetPasswordAdminMenu(IStringLocalizer<ResetPasswordAdminMenu> localizer)
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
                            .Add(S["User reset password"], S["User reset password"].PrefixPosition(), password => password
                                .Permission(Permissions.ManageUsers)
                                .Action("Index", "Admin", new { area = "OrchardCore.Settings", groupId = ResetPasswordSettingsDisplayDriver.GroupId })
                                .LocalNav()
                            )
                        )
                    )
                );

            return Task.CompletedTask;
        }
    }
}
