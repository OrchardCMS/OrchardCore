using Microsoft.Extensions.Localization;
using OrchardCore.Admin.Models;

namespace OrchardCore.Tenants;

/// <summary>
/// The admin list of tenants rendered by the <c>AdminList</c> shape.
/// </summary>
public static class TenantsAdminList
{
    /// <summary>
    /// The name of the list, used by <see cref="Admin.IAdminListColumnProvider"/> and the
    /// <c>AdminList__Tenants</c> and <c>AdminListCell__Tenants__{Column}</c> alternates.
    /// </summary>
    public const string Name = "Tenants";

    /// <summary>
    /// The default columns used by layouts with columns, e.g. <c>Table</c>. Each column renders one or more
    /// zones of the <c>ShellSettingsEntry_SummaryAdmin</c> shape.
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
            // The name and the url take the space left by the other columns.
            Name = "Name",
            Position = "20",
            Title = S["Name"],
            Zones = ["Content", "Description"],
        },
        new()
        {
            Name = "Category",
            Position = "30",
            Title = S["Category"],
            Zones = ["Category"],
            Width = AdminListColumn.AutoWidth,
        },
        new()
        {
            Name = "Database",
            Position = "40",
            Title = S["Database"],
            Zones = ["Database"],
            Width = AdminListColumn.AutoWidth,
        },
        new()
        {
            Name = "Recipe",
            Position = "50",
            Title = S["Recipe"],
            Zones = ["Recipe"],
            Width = AdminListColumn.AutoWidth,
        },
        new()
        {
            // What other features add to the row, e.g. the feature profiles of the tenant.
            Name = "Tags",
            Position = "60",
            Title = S["Tags"],
            Zones = ["Tags"],
            Width = AdminListColumn.AutoWidth,
        },
        new()
        {
            Name = "State",
            Position = "70",
            Title = S["State"],
            Zones = ["State"],
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
