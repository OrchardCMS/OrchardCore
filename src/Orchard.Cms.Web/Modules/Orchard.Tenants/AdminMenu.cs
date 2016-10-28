using Microsoft.Extensions.Localization;
using Orchard.Environment.Navigation;
using System;
using Orchard.Environment.Shell;

namespace Orchard.Tenants
{
    public class AdminMenu : INavigationProvider
    {
        private readonly ShellSettings _shellSettings;

        public AdminMenu(IStringLocalizer<AdminMenu> localizer, ShellSettings shellSettings)
        {
            _shellSettings = shellSettings;
            T = localizer;
        }

        public IStringLocalizer T { get; set; }

        public void BuildNavigation(string name, NavigationBuilder builder)
        {
            if (!String.Equals(name, "admin", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            // Don't add the menu item on non-default tenants
            if (_shellSettings.Name != ShellHelper.DefaultShellName)
            {
                return;
            }

            builder
                .Add(T["Design"], "10", design => design
                    .AddClass("menu-design")
                    .Add(T["Site"], "10", import => import
                        .Add(T["Tenants"], "5", deployment => deployment
                            .Action("Index", "Admin", new { area = "Orchard.Tenants" })
                            .Permission(Permissions.ManageTenants)
                            .LocalNav()
                        )
                    )
                );
        }
    }
}
