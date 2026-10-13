using Microsoft.Extensions.Localization;
using OrchardCore.Admin;
using OrchardCore.Admin.Models;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Sitemaps.ViewModels;

namespace OrchardCore.Sitemaps;

/// <summary>
/// Declares the columns of the <see cref="SitemapsAdminList"/> list, used by the layouts with columns, e.g.
/// <c>Grid</c>. They are the columns of the <see cref="SitemapIndexesAdminList"/> list as well: both rows carry the
/// same name and enabled state, so they present the same way.
/// </summary>
public sealed class SitemapsAdminListColumnDriver : AdminListColumnDriver<SitemapListEntry>
{
    private readonly IStringLocalizer S;

    public SitemapsAdminListColumnDriver(IStringLocalizer<SitemapsAdminListColumnDriver> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public override IDisplayResult Columns(AdminListColumnsContext context)
    {
        SitemapsAdminListColumns.Add(context, S["Name"], S["Status"], S["Actions"]);

        return null;
    }
}

/// <summary>
/// Declares the columns of the <see cref="SitemapIndexesAdminList"/> list, which are the ones of the
/// <see cref="SitemapsAdminList"/> list.
/// </summary>
public sealed class SitemapIndexesAdminListColumnDriver : AdminListColumnDriver<SitemapIndexListEntry>
{
    private readonly IStringLocalizer S;

    public SitemapIndexesAdminListColumnDriver(IStringLocalizer<SitemapIndexesAdminListColumnDriver> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public override IDisplayResult Columns(AdminListColumnsContext context)
    {
        SitemapsAdminListColumns.Add(context, S["Name"], S["Status"], S["Actions"]);

        return null;
    }
}

// The columns both lists share. The titles are localized by each provider, so they are extracted and looked up
// with the class they are written in.
internal static class SitemapsAdminListColumns
{
    public static void Add(AdminListColumnsContext context, LocalizedString name, LocalizedString status, LocalizedString actions)
    {
        context.Columns.Add(AdminListColumns.Select());

        context.Columns.Add(new AdminListColumn
        {
            // The name takes the space left by the other columns.
            Name = "Name",
            Position = "20",
            Title = name,
            Zones = ["Content", "Description"],
        });

        context.Columns.Add(new AdminListColumn
        {
            Name = "Status",
            Position = "30",
            Title = status,
            Zones = ["Tags"],
            Width = AdminListColumn.AutoWidth,
        });

        context.Columns.Add(AdminListColumns.Actions(actions));
    }
}
