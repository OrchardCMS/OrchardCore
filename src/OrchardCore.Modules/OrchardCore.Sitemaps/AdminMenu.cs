using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

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
                        .Add(S["Sitemaps"], "5", sitemaps => sitemaps
                            .Action("List", "Admin", new { area = "OrchardCore.Sitemaps" })
                            .LocalNav()
                        )
                        .Add(S["Sitemap Index"], "10", sitemaps => sitemaps
                            .Action("Edit", "SitemapIndex", new { area = "OrchardCore.Sitemaps" })
                            .LocalNav()
                        )
                    ));

            return Task.CompletedTask;
        }
    }
}