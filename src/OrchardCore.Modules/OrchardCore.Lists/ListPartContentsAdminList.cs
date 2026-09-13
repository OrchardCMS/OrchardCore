using Microsoft.Extensions.Localization;
using OrchardCore.Admin.Models;

namespace OrchardCore.Lists;

/// <summary>
/// The list of content items contained in a List content item, rendered by the <c>AdminList</c> shape.
/// </summary>
/// <remarks>
/// It has its own name rather than reusing the one of the Manage Content list: this module does not
/// reference the Contents module, and the two lists are configured independently. The columns read the same
/// zones of the <c>Content_SummaryAdmin</c> shape, so the rows present the same way.
/// </remarks>
public static class ListPartContentsAdminList
{
    /// <summary>
    /// The name of the list, used by <see cref="Admin.IAdminListColumnProvider"/> and the
    /// <c>AdminList__ListPartContents</c> and <c>AdminListCell__ListPartContents__{Column}</c> alternates.
    /// </summary>
    public const string Name = "ListPartContents";

    /// <summary>
    /// The default columns used by layouts with columns, e.g. <c>Table</c>. There is no selection column:
    /// the contained items are managed one at a time, the list has no bulk actions.
    /// </summary>
    public static List<AdminListColumn> GetDefaultColumns(IStringLocalizer S) =>
    [
        new()
        {
            // The title takes the space left by the other columns.
            Name = "Title",
            Position = "10",
            Title = S["Title"],
            Zones = ["Title", "Header"],
        },
        new()
        {
            Name = "Type",
            Position = "20",
            Title = S["Type"],
            Zones = ["Type"],
            Width = AdminListColumn.AutoWidth,
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
