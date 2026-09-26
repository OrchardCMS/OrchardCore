using Microsoft.Extensions.Localization;
using OrchardCore.Admin;
using OrchardCore.Admin.Models;

namespace OrchardCore.Roles;

/// <summary>
/// Declares the columns of the <see cref="RolesAdminList"/> list, used by the layouts with columns, e.g.
/// <c>Grid</c>. There is no selection column: the page has no bulk actions to apply to the selected rows.
/// </summary>
public sealed class RolesAdminListColumnProvider : IAdminListColumnProvider
{
    private readonly IStringLocalizer S;

    public RolesAdminListColumnProvider(IStringLocalizer<RolesAdminListColumnProvider> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public Task BuildAsync(AdminListColumnsContext context, CancellationToken cancellationToken = default)
    {
        context.Columns.Add(new AdminListColumn
        {
            // The name and the description take the space left by the other columns.
            Name = "Name",
            Position = "10",
            Title = S["Name"],
            Zones = ["Content", "Description"],
        });

        context.Columns.Add(new AdminListColumn
        {
            Name = "Kind",
            Position = "20",
            Title = S["Kind"],
            Zones = ["Tags"],
            Width = AdminListColumn.AutoWidth,
        });

        context.Columns.Add(AdminListColumns.Actions(S["Actions"]));

        return Task.CompletedTask;
    }
}
