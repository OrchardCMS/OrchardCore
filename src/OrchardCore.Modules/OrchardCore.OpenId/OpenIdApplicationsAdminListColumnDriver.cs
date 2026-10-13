using Microsoft.Extensions.Localization;
using OrchardCore.Admin;
using OrchardCore.Admin.Models;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.OpenId.ViewModels;

namespace OrchardCore.OpenId;

/// <summary>
/// Declares the columns of the <see cref="OpenIdApplicationsAdminList"/> list, used by the layouts with columns,
/// e.g. <c>Grid</c>. Each column renders one or more zones of the <c>OpenIdApplicationEntry_SummaryAdmin</c>
/// shape.
/// </summary>
/// <remarks>
/// There is no selection column: the page has no bulk actions to apply to the selected rows.
/// </remarks>
public sealed class OpenIdApplicationsAdminListColumnDriver : AdminListColumnDriver<OpenIdApplicationEntry>
{
    private readonly IStringLocalizer S;

    public OpenIdApplicationsAdminListColumnDriver(IStringLocalizer<OpenIdApplicationsAdminListColumnDriver> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public override IDisplayResult Columns(AdminListColumnsContext context)
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

        return null;
    }
}
