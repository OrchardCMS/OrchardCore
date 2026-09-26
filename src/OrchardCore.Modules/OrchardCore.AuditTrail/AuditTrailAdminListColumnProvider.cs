using Microsoft.Extensions.Localization;
using OrchardCore.Admin;
using OrchardCore.Admin.Models;

namespace OrchardCore.AuditTrail;

/// <summary>
/// Declares the columns of the <see cref="AuditTrailAdminList"/> list, used by the layouts with columns, e.g.
/// <c>Grid</c>. Each column renders one or more zones of the <c>AuditTrailEvent_SummaryAdmin</c> shape.
/// </summary>
/// <remarks>
/// The events are read-only, so unlike the other lists there is no selection column: the page has no
/// bulk actions to apply to the selected rows.
/// </remarks>
public sealed class AuditTrailAdminListColumnProvider : IAdminListColumnProvider
{
    private readonly IStringLocalizer S;

    public AuditTrailAdminListColumnProvider(IStringLocalizer<AuditTrailAdminListColumnProvider> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public Task BuildAsync(AdminListColumnsContext context, CancellationToken cancellationToken = default)
    {
        // What happened and the category it happened in take a quarter of the row each, so no column of badges
        // crowds into a corner of a wide screen.
        context.Columns.Add(new AdminListColumn
        {
            Name = "Event",
            Position = "10",
            Title = S["Event"],
            Zones = ["EventName"],
            Width = "25%",
        });

        context.Columns.Add(new AdminListColumn
        {
            Name = "Category",
            Position = "20",
            Title = S["Category"],
            Zones = ["EventCategory"],
            Width = "25%",
        });

        context.Columns.Add(new AdminListColumn
        {
            // The rest of it takes the space the others leave, and stands in for the title of a row when the
            // list is too narrow for columns and stacks them.
            Name = "Details",
            Position = "30",
            Title = S["Details"],
            Zones = ["EventMeta", "EventData"],
        });

        context.Columns.Add(AdminListColumns.Actions(S["Actions"]));

        return Task.CompletedTask;
    }
}
