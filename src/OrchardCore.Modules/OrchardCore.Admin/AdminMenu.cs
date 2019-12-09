using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;
using OrchardCore.Admin.Drivers;

namespace OrchardCore.Admin
{
    public class AdminMenu : INavigationProvider
    {
        public AdminMenu(IStringLocalizer<AdminMenu> localizer)
        {
            T = localizer;
        }

        public IStringLocalizer T { get; }

        public Task BuildNavigationAsync(string name, NavigationBuilder builder)
        {
            if (!String.Equals(name, "admin", StringComparison.OrdinalIgnoreCase))
            {
                return Task.CompletedTask;
            }

            builder
                .Add(T["Design"], design => design
                    .Add(T["Settings"], settings => settings
                        .Add(T["Admin Theme"], T["Admin Theme"], zones => zones
                            .Action("Index", "Admin", new { area = "OrchardCore.Settings", groupId = AdminThemeSiteSettingsDisplayDriver.GroupId })
                            .Permission(PermissionsAdminTheme.ManageAdminTheme)
                            .LocalNav()
                        )
                    ));

            return Task.CompletedTask;
        }
    }
}