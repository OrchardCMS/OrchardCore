using Microsoft.Extensions.Localization;
using Orchard.Environment.Navigation;
using System;
using OrchardCore.Tenant;

namespace Orchard.Tenants
{
    public class AdminMenu : INavigationProvider
    {
        private readonly TenantSettings _tenantSettings;

        public AdminMenu(IStringLocalizer<AdminMenu> localizer, TenantSettings tenantSettings)
        {
            _tenantSettings = tenantSettings;
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
            if (_tenantSettings.Name != TenantHelper.DefaultTenantName)
            {
                return;
            }

            builder
                .Add(T["Design"], "10", design => design
                    .AddClass("menu-design").Id("design")
                    .Add(T["Tenants"], "5", deployment => deployment
                        .Action("Index", "Admin", new { area = "Orchard.Tenants" })
                        .Permission(Permissions.ManageTenants)
                        .LocalNav()
                    )
                );
        }
    }
}
