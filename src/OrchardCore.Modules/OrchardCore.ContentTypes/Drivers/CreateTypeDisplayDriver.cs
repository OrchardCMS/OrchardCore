using Microsoft.Extensions.Localization;
using OrchardCore.ContentTypes.ViewModels;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.ContentTypes.Drivers;

/// <summary>
/// Builds the editor of a new content type, whose fields are grouped in cards by their placement.
/// </summary>
internal sealed class CreateTypeDisplayDriver : DisplayDriver<CreateTypeViewModel>
{
    internal readonly IStringLocalizer S;

    public CreateTypeDisplayDriver(IStringLocalizer<CreateTypeDisplayDriver> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public override IDisplayResult Edit(CreateTypeViewModel model, BuildEditorContext context)
    {
        return Initialize<CreateTypeViewModel>("CreateTypeGeneral_Edit", m => Populate(model, m))
            .Location(l => l.Zone("Content", "1").Card(S["General"], "10"));
    }

    public override async Task<IDisplayResult> UpdateAsync(CreateTypeViewModel model, UpdateEditorContext context)
    {
        await context.Updater.TryUpdateModelAsync(model, Prefix, m => m.DisplayName, m => m.Name);

        return Edit(model, context);
    }

    // The fields keep the names the page posts, which the controller validates and the scripts of the page select.
    protected override void BuildPrefix(CreateTypeViewModel model, string htmlFieldPrefix)
    {
        Prefix = htmlFieldPrefix ?? string.Empty;
    }

    private static void Populate(CreateTypeViewModel source, CreateTypeViewModel model)
    {
        model.DisplayName = source.DisplayName;
        model.Name = source.Name;
    }
}
