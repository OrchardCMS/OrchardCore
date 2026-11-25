using Microsoft.Extensions.Localization;
using OrchardCore.Deployment.ViewModels;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Deployment.Drivers;

/// <summary>
/// Builds the editor of the JSON import page, whose fields are grouped in cards by their placement.
/// </summary>
internal sealed class ImportJsonDisplayDriver : DisplayDriver<ImportJsonViewModel>
{
    internal readonly IStringLocalizer S;

    public ImportJsonDisplayDriver(IStringLocalizer<ImportJsonDisplayDriver> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public override IDisplayResult Edit(ImportJsonViewModel model, BuildEditorContext context)
    {
        return Initialize<ImportJsonViewModel>("ImportJsonRecipe_Edit", m => m.Json = model.Json)
            .Location(l => l.Zone("Content", "1").Card(S["Recipe"], "10"));
    }

    public override async Task<IDisplayResult> UpdateAsync(ImportJsonViewModel model, UpdateEditorContext context)
    {
        await context.Updater.TryUpdateModelAsync(model, Prefix, m => m.Json);

        return Edit(model, context);
    }

    // The fields keep the names the page posts, which the controller validates and the script of the page selects.
    protected override void BuildPrefix(ImportJsonViewModel model, string htmlFieldPrefix)
    {
        Prefix = htmlFieldPrefix ?? string.Empty;
    }
}
