using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;
using System;
using System.Threading.Tasks;

namespace OrchardCore.Queries
{
    public class AdminMenu : INavigationProvider
    {
        public AdminMenu(IStringLocalizer<AdminMenu> localizer)
        {
            T = localizer;
        }

        public IStringLocalizer T { get; }

        public Task BuildNavigationAsync(string name, NavigationBuilder builder)
        {
            if (!String.Equals(name, "admin", StringComparison.OrdinalIgnoreCase))
            {
                return Task.CompletedTask;
            }

            builder.Add(T["Search"], NavigationConstants.AdminMenuSearchPosition, search => search
                    .AddClass("search").Id("search")
                    .Add(T["Queries"], T["Queries"], contentItems => contentItems
                    .Add(T["All queries"], "1", queries => queries
                        .Action("Index", "Admin", new { area = "OrchardCore.Queries" })
                        .Permission(Permissions.ManageQueries)
                        .LocalNav())
                    ));

            return Task.CompletedTask;
        }
    }
}
