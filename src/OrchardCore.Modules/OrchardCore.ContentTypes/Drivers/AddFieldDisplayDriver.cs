using Microsoft.Extensions.Localization;
using OrchardCore.ContentTypes.ViewModels;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.ContentTypes.Drivers;

/// <summary>
/// Builds the editor of a field added to a content part, whose fields are grouped in cards by their placement.
/// </summary>
internal sealed class AddFieldDisplayDriver : DisplayDriver<AddFieldViewModel>
{
    internal readonly IStringLocalizer S;

    public AddFieldDisplayDriver(IStringLocalizer<AddFieldDisplayDriver> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public override IDisplayResult Edit(AddFieldViewModel model, BuildEditorContext context)
    {
        return Initialize<AddFieldViewModel>("AddFieldGeneral_Edit", m => Populate(model, m))
            .Location(l => l.Zone("Content", "1").Card(S["General"], "10"));
    }

    public override async Task<IDisplayResult> UpdateAsync(AddFieldViewModel model, UpdateEditorContext context)
    {
        await context.Updater.TryUpdateModelAsync(model, Prefix, m => m.DisplayName, m => m.Name, m => m.FieldTypeName);

        return Edit(model, context);
    }

    // The fields keep the names the page posts, which the controller validates and the scripts of the page select.
    protected override void BuildPrefix(AddFieldViewModel model, string htmlFieldPrefix)
    {
        Prefix = htmlFieldPrefix ?? string.Empty;
    }

    private static void Populate(AddFieldViewModel source, AddFieldViewModel model)
    {
        model.DisplayName = source.DisplayName;
        model.Name = source.Name;
        model.FieldTypeName = source.FieldTypeName;
        model.Part = source.Part;
        model.Fields = source.Fields;
    }
}
