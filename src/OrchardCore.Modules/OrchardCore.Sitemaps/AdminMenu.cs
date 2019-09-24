using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Sitemaps.Drivers;

namespace OrchardCore.Sitemaps
{
    public class AdminMenu : INavigationProvider
    {
        public AdminMenu(IStringLocalizer<AdminMenu> localizer)
        {
            S = localizer;
        }

        public IStringLocalizer S { get; set; }

        public Task BuildNavigationAsync(string name, NavigationBuilder builder)
        {
            if (!string.Equals(name, "admin", StringComparison.OrdinalIgnoreCase))
            {
                return Task.CompletedTask;
            }

            builder.Add(S["Configuration"], cfg => cfg
                    .Add(S["Sitemaps"], "1.6", admt => admt
                        .Permission(Permissions.ManageSitemaps)
                        .Action("List", "Set", new { area = "OrchardCore.Sitemaps" })
                        .LocalNav()
                    ));
            return Task.CompletedTask;
        }
    }
}