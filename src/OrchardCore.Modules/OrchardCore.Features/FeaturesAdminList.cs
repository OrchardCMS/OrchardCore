using Microsoft.Extensions.Localization;
using OrchardCore.Admin.Models;

namespace OrchardCore.Features;

/// <summary>
/// The admin list of features rendered by the <c>AdminList</c> shape. The page renders one list per
/// category, all sharing this name so a column provider configures them together.
/// </summary>
public static class FeaturesAdminList
{
    /// <summary>
    /// The name of the list, used by <see cref="Admin.IAdminListColumnProvider"/> and the
    /// <c>AdminList__Features</c> and <c>AdminListCell__Features__{Column}</c> alternates.
    /// </summary>
    public const string Name = "Features";

    /// <summary>
    /// The default columns used by layouts with columns, e.g. <c>Table</c>. Each column renders one or more
    /// zones of the <c>FeatureEntry_SummaryAdmin</c> shape.
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
            // The name and the description take the space left by the other columns.
            Name = "Name",
            Position = "20",
            Title = S["Name"],
            Zones = ["Content", "Description"],
        },
        new()
        {
            Name = "Dependencies",
            Position = "30",
            Title = S["Dependencies"],
            Zones = ["Tags"],
        },
        new()
        {
            Name = "Status",
            Position = "40",
            Title = S["Status"],
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
