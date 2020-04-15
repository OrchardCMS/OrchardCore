using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;
using OrchardCore.ThemeSettings.Drivers;

namespace OrchardCore.ThemeSettings
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
                .Add(S["Design"], configuration => configuration
                    .Add(S["Settings"], settings => settings
                        .Add(S["Theme"], S["Theme"], zones => zones
                            .Action("Index", "Admin", new { area = "OrchardCore.Settings", groupId = ThemeSettingsDisplayDriver.GroupId })
                            .Permission(Permissions.ManageThemeSettings)
                            .LocalNav()
                        )));

            return Task.CompletedTask;
        }
    }
}
