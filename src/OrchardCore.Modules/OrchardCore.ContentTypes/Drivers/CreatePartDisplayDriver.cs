using Microsoft.Extensions.Localization;
using OrchardCore.ContentTypes.ViewModels;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.ContentTypes.Drivers;

/// <summary>
/// Builds the editor of a new content part, whose fields are grouped in cards by their placement.
/// </summary>
internal sealed class CreatePartDisplayDriver : DisplayDriver<CreatePartViewModel>
{
    internal readonly IStringLocalizer S;

    public CreatePartDisplayDriver(IStringLocalizer<CreatePartDisplayDriver> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public override IDisplayResult Edit(CreatePartViewModel model, BuildEditorContext context)
    {
        return Initialize<CreatePartViewModel>("CreatePartGeneral_Edit", m => m.Name = model.Name)
            .Location(l => l.Zone("Content", "1").Card(S["General"], "10"));
    }

    public override async Task<IDisplayResult> UpdateAsync(CreatePartViewModel model, UpdateEditorContext context)
    {
        await context.Updater.TryUpdateModelAsync(model, Prefix, m => m.Name);

        return Edit(model, context);
    }

    // The fields keep the names the page posts, which the controller validates.
    protected override void BuildPrefix(CreatePartViewModel model, string htmlFieldPrefix)
    {
        Prefix = htmlFieldPrefix ?? string.Empty;
    }
}
