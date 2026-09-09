using Microsoft.Extensions.Localization;
using OrchardCore.Admin.Models;

namespace OrchardCore.Roles;

/// <summary>
/// The admin list of roles rendered by the <c>AdminList</c> shape.
/// </summary>
public static class RolesAdminList
{
    /// <summary>
    /// The name of the list, used by <see cref="Admin.IAdminListColumnProvider"/> and the
    /// <c>AdminList__Roles</c> and <c>AdminListCell__Roles__{Column}</c> alternates.
    /// </summary>
    public const string Name = "Roles";

    /// <summary>
    /// The default columns used by layouts with columns, e.g. <c>Table</c>. There is no selection column:
    /// the page has no bulk actions to apply to the selected rows.
    /// </summary>
    public static List<AdminListColumn> GetDefaultColumns(IStringLocalizer S) =>
    [
        new()
        {
            // The name and the description take the space left by the other columns.
            Name = "Name",
            Position = "10",
            Title = S["Name"],
            Zones = ["Content", "Description"],
        },
        new()
        {
            Name = "Kind",
            Position = "20",
            Title = S["Kind"],
            Zones = ["Tags"],
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
