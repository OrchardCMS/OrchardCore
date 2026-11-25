using Microsoft.Extensions.Localization;
using OrchardCore.ContentTypes.ViewModels;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.ContentTypes.Drivers;

/// <summary>
/// Builds the editor of the parts added to a content type, grouped in cards by their placement.
/// </summary>
internal sealed class AddPartsDisplayDriver : DisplayDriver<AddPartsViewModel>
{
    internal readonly IStringLocalizer S;

    public AddPartsDisplayDriver(IStringLocalizer<AddPartsDisplayDriver> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public override IDisplayResult Edit(AddPartsViewModel model, BuildEditorContext context)
    {
        return Initialize<AddPartsViewModel>("AddPartsSelection_Edit", m => Populate(model, m))
            .Location(l => l.Zone("Content", "1").Card(S["Parts"], "10"));
    }

    public override async Task<IDisplayResult> UpdateAsync(AddPartsViewModel model, UpdateEditorContext context)
    {
        await context.Updater.TryUpdateModelAsync(model, Prefix, m => m.PartSelections);

        return Edit(model, context);
    }

    // The fields keep the names the page posts, which the controller binds and the scripts of the page select.
    protected override void BuildPrefix(AddPartsViewModel model, string htmlFieldPrefix)
    {
        Prefix = htmlFieldPrefix ?? string.Empty;
    }

    private static void Populate(AddPartsViewModel source, AddPartsViewModel model)
    {
        model.Type = source.Type;
        model.PartSelections = source.PartSelections;
    }
}
