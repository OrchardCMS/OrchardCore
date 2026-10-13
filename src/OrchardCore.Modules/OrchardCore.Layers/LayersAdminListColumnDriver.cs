using Microsoft.Extensions.Localization;
using OrchardCore.Admin;
using OrchardCore.Admin.Models;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Layers.Models;

namespace OrchardCore.Layers;

/// <summary>
/// Declares the columns of the <see cref="LayersAdminList"/> list, used by the layouts with columns, e.g.
/// <c>Grid</c>. Each column renders one or more zones of the <c>Layer_SummaryAdmin</c> shape. There is no bulk
/// action: the checkbox of a row reveals the widgets of that layer.
/// </summary>
public sealed class LayersAdminListColumnDriver : AdminListColumnDriver<Layer>
{
    private readonly IStringLocalizer S;

    public LayersAdminListColumnDriver(IStringLocalizer<LayersAdminListColumnDriver> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public override IDisplayResult Columns(AdminListColumnsContext context)
    {
        context.Columns.Add(AdminListColumns.Select());

        context.Columns.Add(new AdminListColumn
        {
            // The name and the description take the space left by the actions.
            Name = "Name",
            Position = "20",
            Title = S["Name"],
            Zones = ["Content", "Description"],
        });

        context.Columns.Add(AdminListColumns.Actions(S["Actions"]));

        return null;
    }
}
