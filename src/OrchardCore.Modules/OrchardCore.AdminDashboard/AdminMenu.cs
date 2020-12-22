using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.AdminDashboard.Services;
using OrchardCore.Navigation;

namespace OrchardCore.AdminDashboard
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

            // Configuration and settings menus for the AdminDashboard module
            builder.Add(S["Dashboard"], NavigationConstants.AdminMenuDashboardPosition, adm => adm
                    .AddClass("dashboard").Id("dashboard").Action("Index", "Dashboard", new { area = "OrchardCore.AdminDashboard" })
                        .Permission(Permissions.ManageAdminDashboard)
                        .LocalNav()
                    );

            return Task.CompletedTask;
        }
    }
}
