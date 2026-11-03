namespace OrchardCore.Sitemaps.Models;

/// <summary>
/// One cached sitemap file in the sitemap cache admin list. Its name is the shape type of the rows built by
/// <see cref="Drivers.SitemapCacheEntryDisplayDriver"/>, so themes override them with
/// <c>SitemapCacheEntry-SummaryAdmin.cshtml</c>.
/// </summary>
public class SitemapCacheEntry
{
    public string FileName { get; set; }
}
