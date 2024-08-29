using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.Sitemaps;

public sealed class AdminMenu : INavigationProvider
{
    internal readonly IStringLocalizer S;

    public AdminMenu(IStringLocalizer<AdminMenu> localizer)
    {
        S = localizer;
    }

    public Task BuildNavigationAsync(string name, NavigationBuilder builder)
    {
        if (!NavigationHelper.IsAdminMenu(name))
        {
            return Task.CompletedTask;
        }

        builder
            .Add(S["Configuration"], configuration => configuration
                .Add(S["SEO"], S["SEO"].PrefixPosition(), seo => seo
                    .Permission(Permissions.ManageSitemaps)
                    .Add(S["Sitemaps"], S["Sitemaps"].PrefixPosition("1"), sitemaps => sitemaps
                        .Action("List", "Admin", "OrchardCore.Sitemaps")
                        .LocalNav()
                    )
                    .Add(S["Sitemap Indexes"], S["Sitemap Indexes"].PrefixPosition("2"), indexes => indexes
                        .Action("List", "SitemapIndex", "OrchardCore.Sitemaps")
                        .LocalNav()
                    )
                    .Add(S["Sitemaps Cache"], S["Sitemaps Cache"].PrefixPosition("3"), cache => cache
                        .Action("List", "SitemapCache", "OrchardCore.Sitemaps")
                        .LocalNav()
                    )
                )
            );

        return Task.CompletedTask;
    }
}
