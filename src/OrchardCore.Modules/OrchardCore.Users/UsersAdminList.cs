using Microsoft.Extensions.Localization;
using OrchardCore.Admin.Models;

namespace OrchardCore.Users;

/// <summary>
/// The admin list of users rendered by the <c>AdminList</c> shape.
/// </summary>
public static class UsersAdminList
{
    /// <summary>
    /// The name of the list, used by <see cref="OrchardCore.Admin.IAdminListColumnProvider"/> and the
    /// <c>AdminList__Users</c> and <c>AdminListCell__Users__{Column}</c> alternates.
    /// </summary>
    public const string Name = "Users";

    /// <summary>
    /// The default columns used by layouts with columns, e.g. <c>Table</c>. Each column renders one or more
    /// zones of the <c>User_SummaryAdmin</c> shape.
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
            // The user takes the space left by the other columns.
            Name = "User",
            Position = "20",
            Title = S["User"],
            Zones = ["Header", "Meta", "Content"],
        },
        new()
        {
            Name = "Roles",
            Position = "30",
            Title = S["Roles"],
            Zones = ["Description"],
            Width = "30%",
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
