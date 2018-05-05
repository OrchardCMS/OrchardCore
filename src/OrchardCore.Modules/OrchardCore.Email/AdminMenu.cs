using System;
using Microsoft.Extensions.Localization;
using OrchardCore.Email.Drivers;
using OrchardCore.Environment.Navigation;

namespace OrchardCore.Email
{
    public class AdminMenu : INavigationProvider
    {
        private readonly IStringLocalizer<AdminMenu> T;

        public AdminMenu(IStringLocalizer<AdminMenu> localizer)
        {
            T = localizer;
        }

        public void BuildNavigation(string name, NavigationBuilder builder)
        {
            if (!String.Equals(name, "admin", StringComparison.OrdinalIgnoreCase))
                return;

            builder
                .Add(T["Configuration"], configuration => configuration
                    .Add(T["Settings"], settings => settings
                       .Add(T["Smtp"], T["Smtp"], entry => entry
                          .Action("Index", "Admin", new { area = "OrchardCore.Settings", groupId = SmtpSettingsDisplayDriver.GroupId })
                          .Permission(Permissions.ManageEmailSettings)
                          .LocalNav()
                )));
        }
    }
}
