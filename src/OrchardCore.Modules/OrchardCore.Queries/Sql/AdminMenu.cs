using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;
using System;
using System.Threading.Tasks;

namespace OrchardCore.Queries.Sql
{
    public class AdminMenu : INavigationProvider
    {
        private readonly IStringLocalizer S;

        public AdminMenu(IStringLocalizer<AdminMenu> localizer)
        {
            S = localizer;
        }

        public Task BuildNavigationAsync(string name, NavigationBuilder builder)
        {
            if (!String.Equals(name, "admin", StringComparison.OrdinalIgnoreCase))
            {
                return Task.CompletedTask;
            }

            builder
                .Add(S["Search"], search => search
                    .Add(S["Queries"], "10", queries => queries
                        .Add(S["Run SQL Query"], "5", sql => sql
                            .Action("Query", "Admin", new { area = "OrchardCore.Queries" })
                            .Permission(Permissions.ManageSqlQueries)
                            .LocalNav())));


            return Task.CompletedTask;

        }
    }
}
