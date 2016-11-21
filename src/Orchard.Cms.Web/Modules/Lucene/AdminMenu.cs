using Microsoft.Extensions.Localization;
using Orchard.Environment.Navigation;
using System;

namespace Lucene
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
                        .Add(T["Lucene Indexes"], "7", indexes => indexes
                            .Action("Index", "Admin", new { area = "Lucene" })
                            .Permission(Permissions.ManageIndexes)
                            .LocalNav()
                        )
                    )
                );
        }
    }
}
