using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.AdminMenu.Services;
using OrchardCore.Navigation;

namespace OrchardCore.AdminMenu
{
    public class AdminMenu : INavigationProvider
    {
        private readonly AdminMenuNavigationProvidersCoordinator _adminMenuNavigationProvider;
        private protected IStringLocalizer S;

        public AdminMenu(AdminMenuNavigationProvidersCoordinator adminMenuNavigationProvider,
            IStringLocalizer<AdminMenu> localizer)
        {
            _adminMenuNavigationProvider = adminMenuNavigationProvider;
            S = localizer;
        }

        public async Task BuildNavigationAsync(string name, NavigationBuilder builder)
        {
            if (!String.Equals(name, "admin", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            // Configuration and settings menus for the AdminMenu module
            builder.Add(S["Configuration"], configuration => configuration
                    .Add(S["Admin Menus"], S["Admin Menus"].PrefixPosition(), admt => admt
                        .Permission(Permissions.ManageAdminMenu)
                        .Action("List", "Menu", new { area = "OrchardCore.AdminMenu" })
                        .LocalNav()
                    ));

            // This is the entry point for the adminMenu: dynamically generated custom admin menus
            await _adminMenuNavigationProvider.BuildNavigationAsync("adminMenu", builder);
        }
    }
}
