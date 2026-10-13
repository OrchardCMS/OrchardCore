using Microsoft.Extensions.Localization;
using OrchardCore.Admin;
using OrchardCore.Admin.Models;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Tenants.ViewModels;

namespace OrchardCore.Tenants;

/// <summary>
/// Declares the columns of the <see cref="FeatureProfilesAdminList"/> list, used by the layouts with columns, e.g.
/// <c>Grid</c>. Each column renders one or more zones of the <c>FeatureProfileEntry_SummaryAdmin</c> shape.
/// </summary>
public sealed class FeatureProfilesAdminListColumnDriver : AdminListColumnDriver<FeatureProfileEntry>
{
    private readonly IStringLocalizer S;

    public FeatureProfilesAdminListColumnDriver(IStringLocalizer<FeatureProfilesAdminListColumnDriver> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public override IDisplayResult Columns(AdminListColumnsContext context)
    {
        context.Columns.Add(AdminListColumns.Select());

        context.Columns.Add(new AdminListColumn
        {
            // The name takes the space left by the actions.
            Name = "Name",
            Position = "20",
            Title = S["Name"],
            Zones = ["Content", "Description"],
        });

        context.Columns.Add(AdminListColumns.Actions(S["Actions"]));

        return null;
    }
}
