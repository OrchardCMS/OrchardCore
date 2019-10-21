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

            builder.Add(T["Search"], search => search
                .Add(T["Queries"], T["Queries"], contentItems => contentItems
                    .Action("Index", "Admin", new { area = "OrchardCore.Queries" })
                    .Permission(Permissions.ManageQueries)
                    .LocalNav())
                );

            return Task.CompletedTask;
        }
    }
}
