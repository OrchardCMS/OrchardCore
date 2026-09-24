using Microsoft.Extensions.Localization;
using OrchardCore.Admin;
using OrchardCore.Admin.Models;

namespace OrchardCore.Features;

/// <summary>
/// Declares the columns of the <see cref="FeaturesAdminList"/> list, used by the layouts with columns, e.g.
/// <c>Table</c>. Each column renders one or more zones of the <c>FeatureEntry_SummaryAdmin</c> shape.
/// </summary>
public sealed class FeaturesAdminListColumnProvider : IAdminListColumnProvider
{
    private readonly IStringLocalizer S;

    public FeaturesAdminListColumnProvider(IStringLocalizer<FeaturesAdminListColumnProvider> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public Task BuildAsync(AdminListColumnsContext context, CancellationToken cancellationToken = default)
    {
        if (context.ListName != FeaturesAdminList.Name)
        {
            return Task.CompletedTask;
        }

        context.Columns.Add(AdminListColumns.Select());

        context.Columns.Add(new AdminListColumn
        {
            // The name and the description take the space left by the other columns.
            Name = "Name",
            Position = "20",
            Title = S["Name"],
            Zones = ["Content", "Description"],
        });

        context.Columns.Add(new AdminListColumn
        {
            Name = "Dependencies",
            Position = "30",
            Title = S["Dependencies"],
            Zones = ["Tags"],
        });

        context.Columns.Add(new AdminListColumn
        {
            Name = "Status",
            Position = "40",
            Title = S["Status"],
            Zones = ["Meta"],
            Width = AdminListColumn.AutoWidth,
        });

        context.Columns.Add(AdminListColumns.Actions(S["Actions"]));

        return Task.CompletedTask;
    }
}
