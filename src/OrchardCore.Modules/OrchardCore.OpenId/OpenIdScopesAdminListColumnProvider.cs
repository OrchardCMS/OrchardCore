using Microsoft.Extensions.Localization;
using OrchardCore.Admin;
using OrchardCore.Admin.Models;

namespace OrchardCore.OpenId;

/// <summary>
/// Declares the columns of the <see cref="OpenIdScopesAdminList"/> list, used by the layouts with columns, e.g.
/// <c>Grid</c>. Each column renders one or more zones of the <c>OpenIdScopeEntry_SummaryAdmin</c> shape.
/// </summary>
/// <remarks>
/// There is no selection column: the page has no bulk actions to apply to the selected rows.
/// </remarks>
public sealed class OpenIdScopesAdminListColumnProvider : IAdminListColumnProvider
{
    private readonly IStringLocalizer S;

    public OpenIdScopesAdminListColumnProvider(IStringLocalizer<OpenIdScopesAdminListColumnProvider> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public Task BuildAsync(AdminListColumnsContext context, CancellationToken cancellationToken = default)
    {
        context.Columns.Add(new AdminListColumn
        {
            // The display name and the description take the space left by the other columns.
            Name = "DisplayName",
            Position = "10",
            Title = S["Display name"],
            Zones = ["Content", "Description"],
        });

        context.Columns.Add(new AdminListColumn
        {
            Name = "Name",
            Position = "20",
            Title = S["Name"],
            Zones = ["Tags"],
            Width = AdminListColumn.AutoWidth,
        });

        context.Columns.Add(AdminListColumns.Actions(S["Actions"]));

        return Task.CompletedTask;
    }
}
