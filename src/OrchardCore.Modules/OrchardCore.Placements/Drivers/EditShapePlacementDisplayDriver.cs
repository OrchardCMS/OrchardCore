using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Placements.ViewModels;

namespace OrchardCore.Placements.Drivers;

/// <summary>
/// Builds the editor of the placement rules of a shape, whose fields are grouped in cards by their placement.
/// </summary>
internal sealed class EditShapePlacementDisplayDriver : DisplayDriver<EditShapePlacementViewModel>
{
    internal readonly IStringLocalizer S;

    public EditShapePlacementDisplayDriver(IStringLocalizer<EditShapePlacementDisplayDriver> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public override IDisplayResult Edit(EditShapePlacementViewModel model, BuildEditorContext context)
    {
        return Initialize<EditShapePlacementViewModel>("ShapePlacementRules_Edit", m =>
        {
            m.Creating = model.Creating;
            m.ShapeType = model.ShapeType;
            m.Nodes = model.Nodes;
        }).Location(l => l.Zone("Content", "1").Card(S["Placement"], "10"));
    }

    public override async Task<IDisplayResult> UpdateAsync(EditShapePlacementViewModel model, UpdateEditorContext context)
    {
        await context.Updater.TryUpdateModelAsync(model, Prefix, m => m.ShapeType, m => m.Nodes);

        return Edit(model, context);
    }

    // The fields keep the names the page posts, which the controller validates and the scripts of the page select.
    protected override void BuildPrefix(EditShapePlacementViewModel model, string htmlFieldPrefix)
    {
        Prefix = htmlFieldPrefix ?? string.Empty;
    }
}
