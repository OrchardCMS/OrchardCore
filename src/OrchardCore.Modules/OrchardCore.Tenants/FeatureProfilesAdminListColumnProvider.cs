using Microsoft.Extensions.Localization;
using OrchardCore.Admin;
using OrchardCore.Admin.Models;

namespace OrchardCore.Tenants;

/// <summary>
/// Declares the columns of the <see cref="FeatureProfilesAdminList"/> list, used by the layouts with columns, e.g.
/// <c>Table</c>. Each column renders one or more zones of the <c>FeatureProfileEntry_SummaryAdmin</c> shape.
/// </summary>
public sealed class FeatureProfilesAdminListColumnProvider : IAdminListColumnProvider
{
    private readonly IStringLocalizer S;

    public FeatureProfilesAdminListColumnProvider(IStringLocalizer<FeatureProfilesAdminListColumnProvider> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public Task BuildAsync(AdminListColumnsContext context, CancellationToken cancellationToken = default)
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

        return Task.CompletedTask;
    }
}
