using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.OpenId.ViewModels;

namespace OrchardCore.OpenId.Drivers;

/// <summary>
/// Builds the editor of a new OpenID scope, whose fields are grouped in cards by their placement.
/// </summary>
internal sealed class CreateOpenIdScopeDisplayDriver : DisplayDriver<CreateOpenIdScopeViewModel>
{
    internal readonly IStringLocalizer S;

    public CreateOpenIdScopeDisplayDriver(IStringLocalizer<CreateOpenIdScopeDisplayDriver> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public override IDisplayResult Edit(CreateOpenIdScopeViewModel model, BuildEditorContext context)
    {
        return Initialize<CreateOpenIdScopeViewModel>("CreateOpenIdScopeGeneral_Edit", m => Populate(model, m))
            .Location(l => l.Zone("Content", "1").Card(S["General"], "10"));
    }

    public override async Task<IDisplayResult> UpdateAsync(CreateOpenIdScopeViewModel model, UpdateEditorContext context)
    {
        await context.Updater.TryUpdateModelAsync(model, Prefix, m => m.DisplayName, m => m.Name, m => m.Description, m => m.Resources);

        return Edit(model, context);
    }

    // The fields keep the names the page posts, which the controller validates.
    protected override void BuildPrefix(CreateOpenIdScopeViewModel model, string htmlFieldPrefix)
    {
        Prefix = htmlFieldPrefix ?? string.Empty;
    }

    private static void Populate(CreateOpenIdScopeViewModel source, CreateOpenIdScopeViewModel model)
    {
        model.DisplayName = source.DisplayName;
        model.Name = source.Name;
        model.Description = source.Description;
        model.Resources = source.Resources;
    }
}
