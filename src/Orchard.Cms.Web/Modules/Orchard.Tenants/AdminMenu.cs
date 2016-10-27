using Microsoft.Extensions.Localization;
using Orchard.Environment.Navigation;
using System;

namespace Orchard.Tenants
{
    public class AdminMenu : INavigationProvider
    {
        public AdminMenu(IStringLocalizer<AdminMenu> localizer)
        {
            T = localizer;
        }

        public IStringLocalizer T { get; set; }

        public void BuildNavigation(string name, NavigationBuilder builder)
        {
            if (!String.Equals(name, "admin", StringComparison.OrdinalIgnoreCase))
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
