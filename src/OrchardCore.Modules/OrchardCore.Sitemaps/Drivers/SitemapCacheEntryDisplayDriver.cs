using OrchardCore.DisplayManagement.Shapes;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Sitemaps.Models;

namespace OrchardCore.Sitemaps.Drivers;

/// <summary>
/// Builds the row of the sitemap cache admin list. Each shape is placed in the zone that the matching
/// <see cref="Admin.Models.AdminListColumn"/> of <c>SitemapCache</c> renders.
/// </summary>
public sealed class SitemapCacheEntryDisplayDriver : DisplayDriver<SitemapCacheEntry>
{
    public override Task<IDisplayResult> DisplayAsync(SitemapCacheEntry entry, BuildDisplayContext context)
    {
        if (context.DisplayType is OrchardCoreConstants.DisplayType.SummaryAdmin or OrchardCoreConstants.DisplayType.SummaryAdminGrid
            && context.Shape is Shape rowShape)
        {
            rowShape.Classes.Add("item");
            rowShape.Attributes["data-filter-value"] = entry.FileName.ToLowerInvariant();
        }

        return CombineAsync(
            View("SitemapCacheEntry_Fields_SummaryAdmin", entry)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Content:1")
                .Location(OrchardCoreConstants.DisplayType.SummaryAdminGrid, "FileName:1"),
            View("SitemapCacheEntry_Buttons_SummaryAdmin", entry)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Actions:5")
                .Location(OrchardCoreConstants.DisplayType.SummaryAdminGrid, "Actions:5")
        );
    }
}
