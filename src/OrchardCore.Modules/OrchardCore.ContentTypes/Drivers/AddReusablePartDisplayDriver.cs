using Microsoft.Extensions.Localization;
using OrchardCore.ContentTypes.ViewModels;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.ContentTypes.Drivers;

/// <summary>
/// Builds the editor of a named part added to a content type, whose fields are grouped in cards by their placement.
/// </summary>
internal sealed class AddReusablePartDisplayDriver : DisplayDriver<AddReusablePartViewModel>
{
    internal readonly IStringLocalizer S;

    public AddReusablePartDisplayDriver(IStringLocalizer<AddReusablePartDisplayDriver> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public override IDisplayResult Edit(AddReusablePartViewModel model, BuildEditorContext context)
    {
        // The page shows a notice instead when there is no named part to add.
        if (model.PartSelections == null || model.PartSelections.Count == 0)
        {
            return null;
        }

        return Initialize<AddReusablePartViewModel>("AddReusablePartGeneral_Edit", m => Populate(model, m))
            .Location(l => l.Zone("Content", "1").Card(S["General"], "10"));
    }

    public override async Task<IDisplayResult> UpdateAsync(AddReusablePartViewModel model, UpdateEditorContext context)
    {
        await context.Updater.TryUpdateModelAsync(model, Prefix, m => m.DisplayName, m => m.Name, m => m.Description, m => m.SelectedPartName);

        return Edit(model, context);
    }

    // The fields keep the names the page posts, which the controller validates and the scripts of the page select.
    protected override void BuildPrefix(AddReusablePartViewModel model, string htmlFieldPrefix)
    {
        Prefix = htmlFieldPrefix ?? string.Empty;
    }

    private static void Populate(AddReusablePartViewModel source, AddReusablePartViewModel model)
    {
        model.Type = source.Type;
        model.DisplayName = source.DisplayName;
        model.Name = source.Name;
        model.Description = source.Description;
        model.PartSelections = source.PartSelections;
        model.SelectedPartName = source.SelectedPartName;
    }
}
