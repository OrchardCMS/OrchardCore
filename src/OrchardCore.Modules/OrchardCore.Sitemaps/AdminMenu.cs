using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.Sitemaps
{
    public class AdminMenu : INavigationProvider
    {
        protected readonly IStringLocalizer S;

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

            builder.Add(S["Configuration"], configuration => configuration
                    .Add(S["SEO"], S["SEO"].PrefixPosition(), seo => seo
                        .Permission(Permissions.ManageSitemaps)
                        .Add(S["Sitemaps"], S["Sitemaps"].PrefixPosition("1"), sitemaps => sitemaps
                            .Action("List", "Admin", new { area = "OrchardCore.Sitemaps" })
                            .LocalNav()
                        )
                        .Add(S["Sitemap Indexes"], S["Sitemap Indexes"].PrefixPosition("2"), sitemaps => sitemaps
                            .Action("List", "SitemapIndex", new { area = "OrchardCore.Sitemaps" })
                            .LocalNav()
                        )
                        .Add(S["Sitemaps Cache"], S["Sitemaps Cache"].PrefixPosition("3"), sitemaps => sitemaps
                            .Action("List", "SitemapCache", new { area = "OrchardCore.Sitemaps" })
                            .LocalNav()
                        )
                    ));

            return Task.CompletedTask;
        }
    }
}
