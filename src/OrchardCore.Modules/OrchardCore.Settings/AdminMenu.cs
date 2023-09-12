using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;
using OrchardCore.Settings.Drivers;

namespace OrchardCore.Settings
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

            builder
                .Add(S["Configuration"], NavigationConstants.AdminMenuConfigurationPosition, configuration => configuration
                .AddClass("menu-configuration")
                .Id("configuration")
                    .Add(S["Settings"], "1", settings => settings
                    .Add(S["General"], "1", entry => entry
                    .AddClass("general").Id("general")
                        .Action("Index", "Admin", new { area = "OrchardCore.Settings", groupId = DefaultSiteSettingsDisplayDriver.GroupId })
                        .Permission(Permissions.ManageGroupSettings)
                        .LocalNav()
                    ),
                    priority: 1)
            );

            return Task.CompletedTask;
        }
    }
}
