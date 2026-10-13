using Microsoft.Extensions.Localization;
using OrchardCore.Admin;
using OrchardCore.Admin.Models;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Placements.Models;

namespace OrchardCore.Placements;

/// <summary>
/// Declares the columns of the <see cref="PlacementsAdminList"/> list, used by the layouts with columns, e.g.
/// <c>Grid</c>. Each column renders one or more zones of the <c>ShapePlacement_SummaryAdmin</c> shape.
/// </summary>
public sealed class PlacementsAdminListColumnDriver : AdminListColumnDriver<ShapePlacement>
{
    private readonly IStringLocalizer S;

    public PlacementsAdminListColumnDriver(IStringLocalizer<PlacementsAdminListColumnDriver> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public override IDisplayResult Columns(AdminListColumnsContext context)
    {
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

        return null;
    }
}
