using Microsoft.Extensions.Localization;
using OrchardCore.Admin.Models;

namespace OrchardCore.BackgroundTasks;

/// <summary>
/// The admin list of background tasks rendered by the <c>AdminList</c> shape.
/// </summary>
public static class BackgroundTasksAdminList
{
    /// <summary>
    /// The name of the list, used by <see cref="Admin.IAdminListColumnProvider"/> and the
    /// <c>AdminList__BackgroundTasks</c> and <c>AdminListCell__BackgroundTasks__{Column}</c> alternates.
    /// </summary>
    public const string Name = "BackgroundTasks";

    /// <summary>
    /// The default columns used by layouts with columns, e.g. <c>Table</c>. Each column renders one or more
    /// zones of the <c>BackgroundTaskEntry_SummaryAdmin</c> shape.
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
            // The title and the description take the space left by the other columns.
            Name = "Title",
            Position = "20",
            Title = S["Title"],
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
