using Microsoft.Extensions.Localization;
using Orchard.Environment.Navigation;
using System;

namespace Orchard.Indexing
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
                        .Add(T["Indexes"], "7", indexes => indexes
                            .Action("Index", "Admin", new { area = "Orchard.Indexing" })
                            .Permission(Permissions.ManageIndexes)
                            .LocalNav()
                        )
                    )
                );
        }
    }
}
