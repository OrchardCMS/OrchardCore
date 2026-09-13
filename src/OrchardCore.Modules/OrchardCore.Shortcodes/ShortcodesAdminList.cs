using Microsoft.Extensions.Localization;
using OrchardCore.Admin.Models;

namespace OrchardCore.Shortcodes;

/// <summary>
/// The admin list of shortcode templates rendered by the <c>AdminList</c> shape.
/// </summary>
public static class ShortcodesAdminList
{
    /// <summary>
    /// The name of the list, used by <see cref="Admin.IAdminListColumnProvider"/> and the
    /// <c>AdminList__Shortcodes</c> and <c>AdminListCell__Shortcodes__{Column}</c> alternates.
    /// </summary>
    public const string Name = "Shortcodes";

    /// <summary>
    /// The default columns used by layouts with columns, e.g. <c>Table</c>. Each column renders one or more
    /// zones of the <c>ShortcodeTemplateEntry_SummaryAdmin</c> shape.
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
            // The name and the hint take the space left by the other columns.
            Name = "Name",
            Position = "20",
            Title = S["Name"],
            Zones = ["Content", "Description"],
        },
        new()
        {
            Name = "Categories",
            Position = "30",
            Title = S["Categories"],
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
