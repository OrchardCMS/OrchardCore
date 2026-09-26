using Microsoft.Extensions.Localization;
using OrchardCore.Admin;
using OrchardCore.Admin.Models;

namespace OrchardCore.OpenId;

/// <summary>
/// Declares the columns of the <see cref="OpenIdApplicationsAdminList"/> list, used by the layouts with columns,
/// e.g. <c>Grid</c>. Each column renders one or more zones of the <c>OpenIdApplicationEntry_SummaryAdmin</c>
/// shape.
/// </summary>
/// <remarks>
/// There is no selection column: the page has no bulk actions to apply to the selected rows.
/// </remarks>
public sealed class OpenIdApplicationsAdminListColumnProvider : IAdminListColumnProvider
{
    private readonly IStringLocalizer S;

    public OpenIdApplicationsAdminListColumnProvider(IStringLocalizer<OpenIdApplicationsAdminListColumnProvider> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public Task BuildAsync(AdminListColumnsContext context, CancellationToken cancellationToken = default)
    {
        context.Columns.Add(new AdminListColumn
        {
            // The display name takes the space left by the actions.
            Name = "DisplayName",
            Position = "10",
            Title = S["Display name"],
            Zones = ["Content"],
        });

        context.Columns.Add(AdminListColumns.Actions(S["Actions"]));

        return Task.CompletedTask;
    }
}
