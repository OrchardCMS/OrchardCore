using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;


namespace OrchardCore.Sitemaps.Services;

/// <summary>
/// Describes the breadcrumb trails of the sitemaps, sitemap indexes and sitemaps cache screens.
/// </summary>
public sealed class SitemapsBreadcrumbProvider : IBreadcrumbProvider
{
    private static readonly RouteValueDictionary s_routeValues = new()
    {
        { "area", "OrchardCore.Sitemaps" },
    };

    internal readonly IStringLocalizer S;

    public SitemapsBreadcrumbProvider(IStringLocalizer<SitemapsBreadcrumbProvider> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public ValueTask BuildBreadcrumbAsync(BreadcrumbBuilder builder)
    {
        switch (builder.Name)
        {
            case SitemapsBreadcrumbs.List:
                AddSitemaps(builder);
                break;

            case SitemapsBreadcrumbs.Create:
                AddSitemaps(builder);
                builder.Add(S["Create Sitemap"], item => item.Id("Sitemap"));
                break;

            case SitemapsBreadcrumbs.Edit:
                AddSitemaps(builder);
                builder.Add(S["Edit Sitemap"], item => item.Id("Sitemap"));
                break;

            case SitemapsBreadcrumbs.Display:
                AddSitemaps(builder);
                builder.Add(S["Sitemap"], item => item.Id("Sitemap"));
                break;

            case SitemapsBreadcrumbs.SourceCreate:
                AddSitemaps(builder);
                builder.Add(S["Create Sitemap Source"], item => item.Id("Source"));
                break;

            case SitemapsBreadcrumbs.SourceEdit:
                AddSitemaps(builder);
                builder.Add(S["Edit Sitemap Source"], item => item.Id("Source"));
                break;

            case SitemapsBreadcrumbs.IndexesList:
                AddIndexes(builder);
                break;

            case SitemapsBreadcrumbs.IndexesCreate:
                AddIndexes(builder);
                builder.Add(S["Create Sitemap Index"], item => item.Id("SitemapIndex"));
                break;

            case SitemapsBreadcrumbs.IndexesEdit:
                AddIndexes(builder);
                builder.Add(S["Edit Sitemap Index"], item => item.Id("SitemapIndex"));
                break;

            case SitemapsBreadcrumbs.Cache:
                builder.Add(S["Sitemaps Cache"], item => item
                    .Id("SitemapCache")
                    .Action("List", "SitemapCache", s_routeValues)
                    .Permission(SitemapsPermissions.ManageSitemaps));
                break;
        }

        return ValueTask.CompletedTask;
    }

    private void AddSitemaps(BreadcrumbBuilder builder)
        => builder.Add(S["Sitemaps"], item => item
            .Id("Sitemaps")
            .Action("List", "Admin", s_routeValues)
            .Permission(SitemapsPermissions.ManageSitemaps));

    private void AddIndexes(BreadcrumbBuilder builder)
        => builder.Add(S["Sitemap Indexes"], item => item
            .Id("SitemapIndexes")
            .Action("List", "SitemapIndex", s_routeValues)
            .Permission(SitemapsPermissions.ManageSitemaps));
}
