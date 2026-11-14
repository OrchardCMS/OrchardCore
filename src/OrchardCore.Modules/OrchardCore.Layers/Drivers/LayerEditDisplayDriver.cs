using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Layers.ViewModels;

namespace OrchardCore.Layers.Drivers;

/// <summary>
/// Builds the editor of a layer, on the pages creating and editing a layer, whose fields and rules are grouped in
/// cards by their placement.
/// </summary>
internal sealed class LayerEditDisplayDriver : DisplayDriver<LayerEditViewModel>
{
    internal readonly IStringLocalizer S;

    public LayerEditDisplayDriver(IStringLocalizer<LayerEditDisplayDriver> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public override IDisplayResult Edit(LayerEditViewModel model, BuildEditorContext context)
    {
        var results = new List<IDisplayResult>
        {
            Initialize<LayerEditViewModel>("LayerGeneral_Edit", m => Populate(model, m, context.IsNew))
                .Location(l => l.Zone("Content", "1").Card(S["General"], "10")),
        };

        // The rules of an existing layer, with the button adding one: the shape of its rule, built by the controller.
        if (model.LayerRule is not null)
        {
            results.Add(Initialize<LayerEditViewModel>("LayerRules_Edit", m => Populate(model, m, context.IsNew))
                .Location(l => l.Zone("Content", "2").Card(S["Rules"], "20")));
        }

        return Combine(results);
    }

    public override async Task<IDisplayResult> UpdateAsync(LayerEditViewModel model, UpdateEditorContext context)
    {
        await context.Updater.TryUpdateModelAsync(model, Prefix, m => m.Name, m => m.Description);

        return Edit(model, context);
    }

    // The fields keep the names the pages post, which the controller validates.
    protected override void BuildPrefix(LayerEditViewModel model, string htmlFieldPrefix)
    {
        Prefix = htmlFieldPrefix ?? string.Empty;
    }

    private static void Populate(LayerEditViewModel source, LayerEditViewModel model, bool isNew)
    {
        model.Name = source.Name;
        model.Description = source.Description;
        model.LayerRule = source.LayerRule;
        model.Thumbnails = source.Thumbnails;
        model.IsNew = isNew;
    }
}
