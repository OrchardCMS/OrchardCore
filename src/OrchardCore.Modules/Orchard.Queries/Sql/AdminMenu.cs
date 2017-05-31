using Microsoft.Extensions.Localization;
using Orchard.Environment.Navigation;
using System;

namespace Orchard.Queries.Sql
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
                    .AddClass("menu-design").Id("design")
                    .Add(T["Site"], "10", site => site
                        .Add(T["SQL Queries"], "5", queries => queries
                            .Action("Query", "Admin", new { area = "Orchard.Queries" })
                            .Permission(Permissions.ManageSqlQueries)
                            .LocalNav())));

        }
    }
}
