using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Sitemaps.Models;

namespace OrchardCore.Sitemaps.Drivers;

/// <summary>
/// Builds the row of the sitemap cache admin list. Each shape is placed in the zone that the matching
/// <see cref="Admin.Models.AdminListColumn"/> of <see cref="SitemapCacheAdminList"/> renders.
/// </summary>
public sealed class SitemapCacheEntryDisplayDriver : DisplayDriver<SitemapCacheEntry>
{
    public override Task<IDisplayResult> DisplayAsync(SitemapCacheEntry entry, BuildDisplayContext context)
    {
        return CombineAsync(
            View("SitemapCacheEntry_Fields_SummaryAdmin", entry)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Content:1"),
            View("SitemapCacheEntry_Buttons_SummaryAdmin", entry)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Actions:5")
        );
    }
}
