using Microsoft.Extensions.Localization;
using OrchardCore.Admin;
using OrchardCore.Admin.Models;

namespace OrchardCore.Notifications;

/// <summary>
/// Declares the columns of the <see cref="NotificationsAdminList"/> list, used by the layouts with columns, e.g.
/// <c>Grid</c>. Each column renders one or more zones of the <c>Notification_SummaryAdmin</c> shape.
/// </summary>
public sealed class NotificationsAdminListColumnProvider : IAdminListColumnProvider
{
    private readonly IStringLocalizer S;

    public NotificationsAdminListColumnProvider(IStringLocalizer<NotificationsAdminListColumnProvider> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public Task BuildAsync(AdminListColumnsContext context, CancellationToken cancellationToken = default)
    {
        context.Columns.Add(AdminListColumns.Select());

        context.Columns.Add(new AdminListColumn
        {
            // The subject and the summary take the space left by the other columns.
            Name = "Subject",
            Position = "20",
            Title = S["Subject"],
            Zones = ["Content"],
        });

        context.Columns.Add(new AdminListColumn
        {
            Name = "Received",
            Position = "30",
            Title = S["Received"],
            Zones = ["Meta"],
            Width = AdminListColumn.AutoWidth,
        });

        context.Columns.Add(AdminListColumns.Actions(S["Actions"]));

        return Task.CompletedTask;
    }
}
