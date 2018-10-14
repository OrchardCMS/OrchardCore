using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;
using System;
using System.Threading.Tasks;

namespace OrchardCore.Queries.Sql
{
    public class AdminMenu : INavigationProvider
    {
        public AdminMenu(IStringLocalizer<AdminMenu> localizer)
        {
            T = localizer;
        }

        public IStringLocalizer T { get; set; }

        public Task BuildNavigationAsync(string name, NavigationBuilder builder)
        {
            if (!String.Equals(name, "admin", StringComparison.OrdinalIgnoreCase))
            {
                return Task.CompletedTask;
            }

            builder
                .Add(T["Configuration"], "10", configuration => configuration
                    .AddClass("menu-configuration").Id("configuration")
                    .Add(T["Site"], "10", site => site
                        .Add(T["SQL Queries"], "5", queries => queries
                            .Action("Query", "Admin", new { area = "OrchardCore.Queries" })
                            .Permission(Permissions.ManageSqlQueries)
                            .LocalNav())));


            return Task.CompletedTask;

        }
    }
}
