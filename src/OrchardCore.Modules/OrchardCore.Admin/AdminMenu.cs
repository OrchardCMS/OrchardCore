using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.Admin.Drivers;
using OrchardCore.Navigation;

namespace OrchardCore.Admin
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
                .Add(S["Configuration"], configuration => configuration
                    .Add(S["Settings"], settings => settings
                        .Add(S["Admin"], S["Admin"].PrefixPosition(), admin => admin
                        .AddClass("admin").Id("admin")
                            .Action("Index", "Admin", new { area = "OrchardCore.Settings", groupId = AdminSiteSettingsDisplayDriver.GroupId })
                            .Permission(PermissionsAdminSettings.ManageAdminSettings)
                            .LocalNav()
                        )
                    ));

            return Task.CompletedTask;
        }
    }
}
