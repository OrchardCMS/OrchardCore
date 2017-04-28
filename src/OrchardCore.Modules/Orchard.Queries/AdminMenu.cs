using Microsoft.Extensions.Localization;
using Orchard.Environment.Navigation;
using System;

namespace Orchard.Queries
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
                    .Add(T["Site"], "10", site => site
                        .Add(T["Lucene Queries"], "8", queries => queries
                            .Action("Index", "Admin", new { area = "Orchard.Queries" })
                            .LocalNav())))
                ;
        }
    }
}
