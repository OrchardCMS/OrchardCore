using Microsoft.Extensions.Localization;
using OrchardCore.Admin.Models;

namespace OrchardCore.UrlRewriting;

/// <summary>
/// The admin list of rewrite rules rendered by the <c>AdminList</c> shape.
/// </summary>
public static class UrlRewritingAdminList
{
    /// <summary>
    /// The name of the list, used by <see cref="Admin.IAdminListColumnProvider"/> and the
    /// <c>AdminList__UrlRewriting</c> and <c>AdminListCell__UrlRewriting__{Column}</c> alternates.
    /// </summary>
    public const string Name = "UrlRewriting";

    /// <summary>
    /// The id of the element wrapping the rows, which the sortable script reorders.
    /// </summary>
    public const string SortableContainerId = "rewrite-rules-sortable-list";

    /// <summary>
    /// The default columns used by layouts with columns, e.g. <c>Table</c>. Each column renders one or more
    /// zones of the <c>RewriteRule_SummaryAdmin</c> shape.
    /// </summary>
    public static List<AdminListColumn> GetDefaultColumns(IStringLocalizer S) =>
    [
        new()
        {
            // The drag handle: rules are evaluated in order, so the order is part of the data.
            Name = "Handle",
            Position = "5",
            Zones = ["Handle"],
            Width = AdminListColumn.AutoWidth,
        },
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
            Name = "Source",
            Position = "30",
            Title = S["Source"],
            Zones = ["Tags"],
            Width = AdminListColumn.AutoWidth,
        },
        new()
        {
            Name = "Modified",
            Position = "40",
            Title = S["Last modified"],
            Zones = ["Meta"],
            Width = AdminListColumn.AutoWidth,
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
