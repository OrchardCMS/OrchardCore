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
                .Add(T["Design"], design => design
                   .Add(T["Smtp Settings"], "1", p => p
                      .Permission(Permissions.ManageEmailSettings)
                      .Action("Index", "Admin", new { area = "OrchardCore.Settings", groupId = SmtpSettingsDisplayDriver.GroupId })
                      .LocalNav()
                   )
                );
        }
    }
}
