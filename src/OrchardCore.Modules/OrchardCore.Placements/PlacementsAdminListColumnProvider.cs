using Microsoft.Extensions.Localization;
using OrchardCore.Admin;
using OrchardCore.Admin.Models;

namespace OrchardCore.Placements;

/// <summary>
/// Declares the columns of the <see cref="PlacementsAdminList"/> list, used by the layouts with columns, e.g.
/// <c>Table</c>. Each column renders one or more zones of the <c>ShapePlacement_SummaryAdmin</c> shape.
/// </summary>
public sealed class PlacementsAdminListColumnProvider : IAdminListColumnProvider
{
    private readonly IStringLocalizer S;

    public PlacementsAdminListColumnProvider(IStringLocalizer<PlacementsAdminListColumnProvider> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public Task BuildAsync(AdminListColumnsContext context, CancellationToken cancellationToken = default)
    {
        if (context.ListName != PlacementsAdminList.Name)
        {
            return Task.CompletedTask;
        }

        context.Columns.Add(AdminListColumns.Select());

        context.Columns.Add(new AdminListColumn
        {
            // The shape type takes the space left by the other columns.
            Name = "ShapeType",
            Position = "20",
            Title = S["Shape type"],
            Zones = ["Content"],
        });

        context.Columns.Add(AdminListColumns.Actions(S["Actions"]));

        return Task.CompletedTask;
    }
}
