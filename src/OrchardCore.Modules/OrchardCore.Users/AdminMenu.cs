using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;
using OrchardCore.Modules;
using OrchardCore.Users.Drivers;

namespace OrchardCore.Users
{
    public class AdminMenu : INavigationProvider
    {
        public AdminMenu(IStringLocalizer<AdminMenu> localizer)
        {
            T = localizer;
        }

        public IStringLocalizer T { get; set; }

        public Task BuildNavigationAsync(string name, NavigationBuilder builder)
        {
            if (!String.Equals(name, "admin", StringComparison.OrdinalIgnoreCase))
            {
                return Task.CompletedTask;
            }

            builder.Add(T["Security"], "7", security => security
                    .AddClass("security").Id("security")
                        .Add(T["Users"], "5", installed => installed
                            .Action("Index", "Admin", "OrchardCore.Users")
                            .Permission(Permissions.ManageUsers)
                            .LocalNav()
                         )
                        .Add(T["Settings"], settings => settings
                            .Add(T["Login"], T["Login"], registration => registration
                                .Permission(Permissions.ManageUsers)
                                .Action("Index", "Admin", new { area = "OrchardCore.Settings", groupId = LoginSettingsDisplayDriver.GroupId })
                                .LocalNav()
                                )
                            )
                       );

            return Task.CompletedTask;
        }
    }

    [Feature("OrchardCore.Users.Registration")]
    public class RegistrationAdminMenu : INavigationProvider
    {
        public RegistrationAdminMenu(IStringLocalizer<RegistrationAdminMenu> localizer)
        {
            T = localizer;
        }

        public IStringLocalizer T { get; set; }

        public Task BuildNavigationAsync(string name, NavigationBuilder builder)
        {
            if (!String.Equals(name, "admin", StringComparison.OrdinalIgnoreCase))
            {
                return Task.CompletedTask;
            }

            builder
                .Add(T["Security"], security => security
                    .Add(T["Settings"], settings => settings
                        .Add(T["Registration"], T["Registration"], registration => registration
                            .Permission(Permissions.ManageUsers)
                            .Action("Index", "Admin", new { area = "OrchardCore.Settings", groupId = RegistrationSettingsDisplayDriver.GroupId })
                            .LocalNav()
                        )));

            return Task.CompletedTask;
        }
    }

    [Feature("OrchardCore.Users.ResetPassword")]
    public class ResetPasswordAdminMenu : INavigationProvider
    {
        public ResetPasswordAdminMenu(IStringLocalizer<ResetPasswordAdminMenu> localizer)
        {
            T = localizer;
        }

        public IStringLocalizer T { get; set; }

        public Task BuildNavigationAsync(string name, NavigationBuilder builder)
        {
            if (!String.Equals(name, "admin", StringComparison.OrdinalIgnoreCase))
            {
                return Task.CompletedTask;
            }

            builder
                .Add(T["Security"], security => security
                    .Add(T["Settings"], settings => settings
                        .Add(T["Reset password"], T["Reset password"], password => password
                            .Permission(Permissions.ManageUsers)
                            .Action("Index", "Admin", new { area = "OrchardCore.Settings", groupId = ResetPasswordSettingsDisplayDriver.GroupId })
                            .LocalNav()
                        )));

            return Task.CompletedTask;
        }
    }
}
