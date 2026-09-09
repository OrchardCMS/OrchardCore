using Microsoft.Extensions.Localization;
using OrchardCore.Admin.Models;

namespace OrchardCore.Sitemaps;

/// <summary>
/// The admin list of sitemaps rendered by the <c>AdminList</c> shape.
/// </summary>
public static class SitemapsAdminList
{
    /// <summary>
    /// The name of the list, used by <see cref="Admin.IAdminListColumnProvider"/> and the
    /// <c>AdminList__Sitemaps</c> and <c>AdminListCell__Sitemaps__{Column}</c> alternates.
    /// </summary>
    public const string Name = "Sitemaps";

    /// <summary>
    /// The default columns used by layouts with columns, e.g. <c>Table</c>.
    /// </summary>
    public static List<AdminListColumn> GetDefaultColumns(IStringLocalizer S) =>
        SitemapsAdminListColumns.Build(S);
}

/// <summary>
/// The admin list of sitemap indexes rendered by the <c>AdminList</c> shape.
/// </summary>
public static class SitemapIndexesAdminList
{
    /// <summary>
    /// The name of the list, used by <see cref="Admin.IAdminListColumnProvider"/> and the
    /// <c>AdminList__SitemapIndexes</c> and <c>AdminListCell__SitemapIndexes__{Column}</c> alternates.
    /// </summary>
    public const string Name = "SitemapIndexes";

    /// <summary>
    /// The default columns used by layouts with columns, e.g. <c>Table</c>.
    /// </summary>
    public static List<AdminListColumn> GetDefaultColumns(IStringLocalizer S) =>
        SitemapsAdminListColumns.Build(S);
}

/// <summary>
/// The admin list of cached sitemap files rendered by the <c>AdminList</c> shape.
/// </summary>
public static class SitemapCacheAdminList
{
    /// <summary>
    /// The name of the list, used by <see cref="Admin.IAdminListColumnProvider"/> and the
    /// <c>AdminList__SitemapCache</c> and <c>AdminListCell__SitemapCache__{Column}</c> alternates.
    /// </summary>
    public const string Name = "SitemapCache";

    /// <summary>
    /// The default columns used by layouts with columns, e.g. <c>Table</c>. There is no selection column:
    /// the page purges one file at a time or all of them at once, it has no bulk actions on a selection.
    /// </summary>
    public static List<AdminListColumn> GetDefaultColumns(IStringLocalizer S) =>
    [
        new()
        {
            Name = "FileName",
            Position = "10",
            Title = S["File name"],
            Zones = ["Content"],
        },
        new()
        {
            Name = "Actions",
            Position = "end",
            Title = S["Actions"],
            Zones = ["Actions", "ActionsMenu"],
            Width = AdminListColumn.AutoWidth,
            Alignment = AdminListColumnAlignment.End,
        },
    ];
}

/// <summary>
/// The columns shared by the sitemaps and the sitemap indexes lists: both rows carry the same name and
/// enabled state, so they present the same way.
/// </summary>
internal static class SitemapsAdminListColumns
{
    public static List<AdminListColumn> Build(IStringLocalizer S) =>
    [
        new()
        {
            Name = "Select",
            Position = "10",
            Zones = ["Checkbox"],
            CssClass = "admin-list-select",
            Width = AdminListColumn.AutoWidth,
        },
        new()
        {
            // The name takes the space left by the other columns.
            Name = "Name",
            Position = "20",
            Title = S["Name"],
            Zones = ["Content", "Description"],
        },
        new()
        {
            Name = "Status",
            Position = "30",
            Title = S["Status"],
            Zones = ["Tags"],
            Width = AdminListColumn.AutoWidth,
        },
        new()
        {
            // "end" keeps the actions last even when a feature adds a column without a position.
            Name = "Actions",
            Position = "end",
            Title = S["Actions"],
            Zones = ["Actions", "ActionsMenu"],
            Width = AdminListColumn.AutoWidth,
            Alignment = AdminListColumnAlignment.End,
        },
    ];
}
