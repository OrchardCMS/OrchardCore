using Microsoft.Extensions.Localization;
using OrchardCore.Admin;
using OrchardCore.Admin.Models;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Sitemaps.Models;

namespace OrchardCore.Sitemaps;

/// <summary>
/// Declares the columns of the <see cref="SitemapCacheAdminList"/> list, used by the layouts with columns, e.g.
/// <c>Grid</c>. There is no selection column: the page purges one file at a time or all of them at once, it has
/// no bulk actions on a selection.
/// </summary>
public sealed class SitemapCacheAdminListColumnDriver : AdminListColumnDriver<SitemapCacheEntry>
{
    private readonly IStringLocalizer S;

    public SitemapCacheAdminListColumnDriver(IStringLocalizer<SitemapCacheAdminListColumnDriver> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public override IDisplayResult Columns(AdminListColumnsContext context)
    {
        context.Columns.Add(new AdminListColumn
        {
            Name = "FileName",
            Position = "10",
            Title = S["File name"],
            Zones = ["Content"],
        });

        context.Columns.Add(AdminListColumns.Actions(S["Actions"]));

        return null;
    }
}
