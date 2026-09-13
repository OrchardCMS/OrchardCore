using Microsoft.Extensions.Localization;
using OrchardCore.Admin.Models;

namespace OrchardCore.Placements;

/// <summary>
/// The admin list of shape placements rendered by the <c>AdminList</c> shape.
/// </summary>
public static class PlacementsAdminList
{
    /// <summary>
    /// The name of the list, used by <see cref="OrchardCore.Admin.IAdminListColumnProvider"/> and the
    /// <c>AdminList__Placements</c> and <c>AdminListCell__Placements__{Column}</c> alternates.
    /// </summary>
    public const string Name = "Placements";

    /// <summary>
    /// The default columns used by layouts with columns, e.g. <c>Table</c>. Each column renders one or more
    /// zones of the <c>ShapePlacement_SummaryAdmin</c> shape.
    /// </summary>
    public static List<AdminListColumn> GetDefaultColumns(IStringLocalizer S) =>
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
            // The shape type takes the space left by the other columns.
            Name = "ShapeType",
            Position = "20",
            Title = S["Shape type"],
            Zones = ["Content"],
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
