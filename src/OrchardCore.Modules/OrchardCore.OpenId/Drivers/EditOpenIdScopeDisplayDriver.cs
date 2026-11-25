using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.OpenId.ViewModels;

namespace OrchardCore.OpenId.Drivers;

/// <summary>
/// Builds the editor of an existing OpenID scope, whose fields are grouped in cards by their placement.
/// </summary>
internal sealed class EditOpenIdScopeDisplayDriver : DisplayDriver<EditOpenIdScopeViewModel>
{
    internal readonly IStringLocalizer S;

    public EditOpenIdScopeDisplayDriver(IStringLocalizer<EditOpenIdScopeDisplayDriver> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public override IDisplayResult Edit(EditOpenIdScopeViewModel model, BuildEditorContext context)
    {
        return Initialize<EditOpenIdScopeViewModel>("EditOpenIdScopeGeneral_Edit", m => Populate(model, m))
            .Location(l => l.Zone("Content", "1").Card(S["General"], "10"));
    }

    public override async Task<IDisplayResult> UpdateAsync(EditOpenIdScopeViewModel model, UpdateEditorContext context)
    {
        await context.Updater.TryUpdateModelAsync(model, Prefix, m => m.DisplayName, m => m.Name, m => m.Description, m => m.Resources);

        return Edit(model, context);
    }

    // The fields keep the names the page posts, which the controller validates.
    protected override void BuildPrefix(EditOpenIdScopeViewModel model, string htmlFieldPrefix)
    {
        Prefix = htmlFieldPrefix ?? string.Empty;
    }

    private static void Populate(EditOpenIdScopeViewModel source, EditOpenIdScopeViewModel model)
    {
        model.Id = source.Id;
        model.DisplayName = source.DisplayName;
        model.Name = source.Name;
        model.Description = source.Description;
        model.Resources = source.Resources;
    }
}
