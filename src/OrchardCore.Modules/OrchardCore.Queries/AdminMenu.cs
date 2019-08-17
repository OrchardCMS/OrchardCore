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
                    .Add(T["Queries"], "10", import => import
                        .Add(T["Stored Queries"], "1", contentItems => contentItems
                            .Action("Index", "Admin", new { area = "OrchardCore.Queries" })
                            .Permission(Permissions.ManageQueries)
                            .LocalNav())
            ));

            return Task.CompletedTask;
        }
    }
}
