using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.Sitemaps
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
            if (!string.Equals(name, "admin", StringComparison.OrdinalIgnoreCase))
            {
                return Task.CompletedTask;
            }

            builder.Add(S["Configuration"], NavigationConstants.AdminMenuConfigurationPosition, cfg => cfg
                    .Add(S["SEO"], S["am-SEO"], admt => admt
                        .Permission(Permissions.ManageSitemaps)
                        .Add(S["Sitemaps"], S["am1-Sitemaps"], sitemaps => sitemaps
                            .Action("List", "Admin", new { area = "OrchardCore.Sitemaps" })
                            .LocalNav()
                        )
                        .Add(S["Sitemap Indexes"], S["am2-Sitemap Indexes"], sitemaps => sitemaps
                            .Action("List", "SitemapIndex", new { area = "OrchardCore.Sitemaps" })
                            .LocalNav()
                        )
                        .Add(S["Sitemaps Cache"], S["am3-Sitemaps Cache"], sitemaps => sitemaps
                            .Action("List", "SitemapCache", new { area = "OrchardCore.Sitemaps" })
                            .LocalNav()
                        )
                    ));

            return Task.CompletedTask;
        }
    }
}
