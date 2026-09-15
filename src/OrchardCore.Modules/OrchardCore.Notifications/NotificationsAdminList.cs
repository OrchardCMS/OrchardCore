using Microsoft.Extensions.Localization;
using OrchardCore.Admin.Models;

namespace OrchardCore.Notifications;

/// <summary>
/// The admin list of notifications rendered by the <c>AdminList</c> shape.
/// </summary>
public static class NotificationsAdminList
{
    /// <summary>
    /// The name of the list, used by <see cref="Admin.IAdminListColumnProvider"/> and the
    /// <c>AdminList__Notifications</c> and <c>AdminListCell__Notifications__{Column}</c> alternates.
    /// </summary>
    public const string Name = "Notifications";

    /// <summary>
    /// The CSS class carried by the row of a notification that has not been read yet. It is added to the
    /// row shape rather than to the row template, so every layout renders it on its own row element.
    /// </summary>
    public const string UnreadCssClass = "notification-is-unread";

    /// <summary>
    /// The CSS class carried by the row of a notification that has been read.
    /// </summary>
    public const string ReadCssClass = "notification-is-read";

    /// <summary>
    /// The default columns used by layouts with columns, e.g. <c>Table</c>. Each column renders one or more
    /// zones of the <c>Notification_SummaryAdmin</c> shape.
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
            // The subject and the summary take the space left by the other columns.
            Name = "Subject",
            Position = "20",
            Title = S["Subject"],
            Zones = ["Content"],
        },
        new()
        {
            Name = "Received",
            Position = "30",
            Title = S["Received"],
            Zones = ["Meta"],
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
