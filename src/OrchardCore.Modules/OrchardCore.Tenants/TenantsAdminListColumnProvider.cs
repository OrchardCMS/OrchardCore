using Microsoft.Extensions.Localization;
using OrchardCore.Admin;
using OrchardCore.Admin.Models;

namespace OrchardCore.Tenants;

/// <summary>
/// Declares the columns of the <see cref="TenantsAdminList"/> list, used by the layouts with columns, e.g.
/// <c>Table</c>. Each column renders one or more zones of the <c>ShellSettingsEntry_SummaryAdmin</c> shape.
/// </summary>
public sealed class TenantsAdminListColumnProvider : IAdminListColumnProvider
{
    private readonly IStringLocalizer S;

    public TenantsAdminListColumnProvider(IStringLocalizer<TenantsAdminListColumnProvider> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public Task BuildAsync(AdminListColumnsContext context, CancellationToken cancellationToken = default)
    {
        context.Columns.Add(AdminListColumns.Select());

        context.Columns.Add(new AdminListColumn
        {
            // The name and the url take the space left by the other columns.
            Name = "Name",
            Position = "20",
            Title = S["Name"],
            Zones = ["Content", "Description"],
        });

        context.Columns.Add(new AdminListColumn
        {
            Name = "Category",
            Position = "30",
            Title = S["Category"],
            Zones = ["Category"],
            Width = AdminListColumn.AutoWidth,
        });

        context.Columns.Add(new AdminListColumn
        {
            Name = "Database",
            Position = "40",
            Title = S["Database"],
            Zones = ["Database"],
            Width = AdminListColumn.AutoWidth,
        });

        context.Columns.Add(new AdminListColumn
        {
            Name = "Recipe",
            Position = "50",
            Title = S["Recipe"],
            Zones = ["Recipe"],
            Width = AdminListColumn.AutoWidth,
        });

        context.Columns.Add(new AdminListColumn
        {
            // What other features add to the row, e.g. the feature profiles of the tenant.
            Name = "Tags",
            Position = "60",
            Title = S["Tags"],
            Zones = ["Tags"],
            Width = AdminListColumn.AutoWidth,
        });

        context.Columns.Add(new AdminListColumn
        {
            Name = "State",
            Position = "70",
            Title = S["State"],
            Zones = ["State"],
            Width = AdminListColumn.AutoWidth,
        });

        context.Columns.Add(AdminListColumns.Actions(S["Actions"]));

        return Task.CompletedTask;
    }
}
