using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Tenants.ViewModels;

namespace OrchardCore.Tenants.Drivers;

/// <summary>
/// Builds the editor of a feature profile, whose fields are grouped in cards by their placement.
/// </summary>
internal sealed class FeatureProfileDisplayDriver : DisplayDriver<FeatureProfileViewModel>
{
    internal readonly IStringLocalizer S;

    public FeatureProfileDisplayDriver(IStringLocalizer<FeatureProfileDisplayDriver> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public override IDisplayResult Edit(FeatureProfileViewModel model, BuildEditorContext context)
    {
        return Initialize<FeatureProfileViewModel>("FeatureProfileGeneral_Edit", m => Populate(model, m))
            .Location(l => l.Zone("Content", "1").Card(S["General"], "10"));
    }

    public override async Task<IDisplayResult> UpdateAsync(FeatureProfileViewModel model, UpdateEditorContext context)
    {
        await context.Updater.TryUpdateModelAsync(model, Prefix, m => m.Name, m => m.FeatureRules);

        return Edit(model, context);
    }

    // The fields keep the names the page posts, which the controller validates and the scripts of the page select.
    protected override void BuildPrefix(FeatureProfileViewModel model, string htmlFieldPrefix)
    {
        Prefix = htmlFieldPrefix ?? string.Empty;
    }

    private static void Populate(FeatureProfileViewModel source, FeatureProfileViewModel model)
    {
        model.Id = source.Id;
        model.Name = source.Name;
        model.FeatureRules = source.FeatureRules;
    }
}
