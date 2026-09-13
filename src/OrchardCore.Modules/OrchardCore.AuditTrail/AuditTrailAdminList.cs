using Microsoft.Extensions.Localization;
using OrchardCore.Admin.Models;

namespace OrchardCore.AuditTrail;

/// <summary>
/// The admin list of audit trail events rendered by the <c>AdminList</c> shape.
/// </summary>
public static class AuditTrailAdminList
{
    /// <summary>
    /// The name of the list, used by <see cref="OrchardCore.Admin.IAdminListColumnProvider"/> and the
    /// <c>AdminList__AuditTrail</c> and <c>AdminListCell__AuditTrail__{Column}</c> alternates.
    /// </summary>
    public const string Name = "AuditTrail";

    /// <summary>
    /// The default columns used by layouts with columns, e.g. <c>Table</c>. Each column renders one or more
    /// zones of the <c>AuditTrailEvent_SummaryAdmin</c> shape.
    /// </summary>
    /// <remarks>
    /// The events are read-only, so unlike the other lists there is no selection column: the page has no
    /// bulk actions to apply to the selected rows.
    /// </remarks>
    public static List<AdminListColumn> GetDefaultColumns(IStringLocalizer S) =>
    [
        // What happened and the category it happened in take a quarter of the row each, so no column of badges
        // crowds into a corner of a wide screen.
        new()
        {
            Name = "Event",
            Position = "10",
            Title = S["Event"],
            Zones = ["EventName"],
            Width = "25%",
        },
        new()
        {
            Name = "Category",
            Position = "20",
            Title = S["Category"],
            Zones = ["EventCategory"],
            Width = "25%",
        },
        new()
        {
            // The rest of it takes the space the others leave, and stands in for the title of a row when the
            // list is too narrow for columns and stacks them.
            Name = "Details",
            Position = "30",
            Title = S["Details"],
            Zones = ["EventMeta", "EventData"],
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
