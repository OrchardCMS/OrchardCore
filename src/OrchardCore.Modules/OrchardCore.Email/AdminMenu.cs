using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.Email.Drivers;
using OrchardCore.Modules;
using OrchardCore.Navigation;

namespace OrchardCore.Email
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
            if (!string.Equals(name, "admin", StringComparison.OrdinalIgnoreCase))
                return Task.CompletedTask;

            builder
                .Add(S["Configuration"], configuration => configuration
                    .Add(S["Settings"], settings => settings
                        .Add(S["Email"], S["Email"].PrefixPosition(), email => email
                            .AddClass("email").Id("email")
                            .Add(S["Email Settings"], S["Email Settings"].PrefixPosition(), options => options
                               .Action("Index", "Admin", new { area = "OrchardCore.Settings", groupId = EmailSettingsDisplayDriver.GroupId })
                               .Permission(Permissions.ManageEmailSettings)
                               .LocalNav())
                )));

            return Task.CompletedTask;
        }
    }

    [Feature("OrchardCore.Email.Smtp")]
    public class SmtpAdminMenu : INavigationProvider
    {
        protected readonly IStringLocalizer S;

        public SmtpAdminMenu(IStringLocalizer<SmtpAdminMenu> localizer)
        {
            S = localizer;
        }

        public Task BuildNavigationAsync(string name, NavigationBuilder builder)
        {
            if (!string.Equals(name, "admin", StringComparison.OrdinalIgnoreCase))
                return Task.CompletedTask;

            builder
                .Add(S["Configuration"], configuration => configuration
                    .Add(S["Settings"], settings => settings
                        .Add(S["Email"], S["Email"].PrefixPosition(), email => email
                            .AddClass("email").Id("email")
                            .Add(S["SMTP Settings"], S["SMTP Settings"].PrefixPosition(), options => options
                               .Action("Index", "Admin", new { area = "OrchardCore.Settings", groupId = SmtpSettingsDisplayDriver.GroupId })
                               .Permission(SmtpPermissions.ManageSmtpEmailSettings)
                               .LocalNav())
                )));

            return Task.CompletedTask;
        }
    }
}
