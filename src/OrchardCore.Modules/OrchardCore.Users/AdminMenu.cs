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
        protected readonly IStringLocalizer S;

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

            builder.Add(S["Security"], NavigationConstants.AdminMenuSecurityPosition, security => security
                    .AddClass("security").Id("security")
                        .Add(S["Users"], S["Users"].PrefixPosition(), users => users
                            .AddClass("users").Id("users")
                            .Action("Index", "Admin", "OrchardCore.Users")
                            .Permission(CommonPermissions.ListUsers)
                            .Resource(new User())
                            .LocalNav()
                         )
                        .Add(S["Settings"], settings => settings
                            .Add(S["User Login"], S["User Login"].PrefixPosition(), login => login
                                .Permission(CommonPermissions.ManageUsers)
                                .Action("Index", "Admin", new { area = "OrchardCore.Settings", groupId = LoginSettingsDisplayDriver.GroupId })
                                .LocalNav()
                                )
                            )
                       );

            return Task.CompletedTask;
        }
    }

    [Feature("OrchardCore.Users.ChangeEmail")]
    public class ChangeEmailAdminMenu : INavigationProvider
    {
        protected readonly IStringLocalizer S;

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
                .Add(S["Security"], security => security
                    .Add(S["Settings"], settings => settings
                        .Add(S["User Change email"], S["User Change email"].PrefixPosition(), registration => registration
                            .Permission(CommonPermissions.ManageUsers)
                            .Action("Index", "Admin", new { area = "OrchardCore.Settings", groupId = ChangeEmailSettingsDisplayDriver.GroupId })
                            .LocalNav()
                        )));

            return Task.CompletedTask;
        }
    }

    [Feature("OrchardCore.Users.Registration")]
    public class RegistrationAdminMenu : INavigationProvider
    {
        protected readonly IStringLocalizer S;

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
                .Add(S["Security"], security => security
                    .Add(S["Settings"], settings => settings
                        .Add(S["User Registration"], S["User Registration"].PrefixPosition(), registration => registration
                            .Permission(CommonPermissions.ManageUsers)
                            .Action("Index", "Admin", new { area = "OrchardCore.Settings", groupId = RegistrationSettingsDisplayDriver.GroupId })
                            .LocalNav()
                        )));

            return Task.CompletedTask;
        }
    }

    [Feature("OrchardCore.Users.ResetPassword")]
    public class ResetPasswordAdminMenu : INavigationProvider
    {
        protected readonly IStringLocalizer S;

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
                .Add(S["Security"], security => security
                    .Add(S["Settings"], settings => settings
                        .Add(S["User Reset password"], S["User Reset password"].PrefixPosition(), password => password
                            .Permission(CommonPermissions.ManageUsers)
                            .Action("Index", "Admin", new { area = "OrchardCore.Settings", groupId = ResetPasswordSettingsDisplayDriver.GroupId })
                            .LocalNav()
                        )));

            return Task.CompletedTask;
        }
    }
}
