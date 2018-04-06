using Microsoft.Extensions.Localization;
using OrchardCore.Environment.Navigation;
using System;
using OrchardCore.Environment.Shell;

namespace OrchardCore.Tenants
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
                .Add(T["Configuration"], "10", configuration => configuration
                    .AddClass("menu-configuration").Id("configuration")
                    .Add(T["Tenants"], "5", deployment => deployment
                        .Action("Index", "Admin", new { area = "OrchardCore.Tenants" })
                        .Permission(Permissions.ManageTenants)
                        .LocalNav()
                    )
                );
        }
    }
}
