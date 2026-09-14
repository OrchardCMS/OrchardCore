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

    private readonly ISitemapManager _sitemapManager;

    internal readonly IStringLocalizer S;

    public SitemapsBreadcrumbProvider(ISitemapManager sitemapManager, IStringLocalizer<SitemapsBreadcrumbProvider> stringLocalizer)
    {
        _sitemapManager = sitemapManager;
        S = stringLocalizer;
    }

    public async ValueTask BuildBreadcrumbAsync(BreadcrumbBuilder builder)
    {
        switch (builder.Name)
        {
            case SitemapsConstants.List:
                AddSitemaps(builder);
                break;

            case SitemapsConstants.Create:
                AddSitemaps(builder);
                builder.Add(S["Create Sitemap"], item => item.Id("Sitemap"));
                break;

            case SitemapsConstants.Edit:
                AddSitemaps(builder);
                builder.Add(S["Edit Sitemap"], item => item.Id("Sitemap"));
                break;

            case SitemapsConstants.Display:
                AddSitemaps(builder);
                builder.Add(S["Sitemap"], item => item.Id("Sitemap"));
                break;

            case SitemapsConstants.SourceCreate:
                AddSitemaps(builder);
                await AddSitemapAsync(builder);
                builder.Add(S["Create Sitemap Source"], item => item.Id("Source"));
                break;

            case SitemapsConstants.SourceEdit:
                AddSitemaps(builder);
                await AddSitemapAsync(builder);
                builder.Add(S["Edit Sitemap Source"], item => item.Id("Source"));
                break;

            case SitemapsConstants.IndexesList:
                AddIndexes(builder);
                break;

            case SitemapsConstants.IndexesCreate:
                AddIndexes(builder);
                builder.Add(S["Create Sitemap Index"], item => item.Id("SitemapIndex"));
                break;

            case SitemapsConstants.IndexesEdit:
                AddIndexes(builder);
                builder.Add(S["Edit Sitemap Index"], item => item.Id("SitemapIndex"));
                break;

            case SitemapsConstants.Cache:
                builder.Add(S["Sitemaps Cache"], item => item
                    .Id("SitemapCache")
                    .Action("List", "SitemapCache", s_routeValues)
                    .Permission(SitemapsPermissions.ManageSitemaps));
                break;
        }
    }

    // A source is only reached from its sitemap, so its trail leads back through the sitemap.
    private async ValueTask AddSitemapAsync(BreadcrumbBuilder builder)
    {
        var sitemapId = builder.GetData<string>(SitemapsConstants.SitemapIdKey);

        if (string.IsNullOrEmpty(sitemapId))
        {
            return;
        }

        var sitemap = await _sitemapManager.GetSitemapAsync(sitemapId);

        builder.Add(sitemap?.Name ?? S["Sitemap"].Value, item => item
            .Id("Sitemap")
            .Action("Display", "Admin", new RouteValueDictionary(s_routeValues)
            {
                { "sitemapId", sitemapId },
            })
            .Permission(SitemapsPermissions.ManageSitemaps));
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
