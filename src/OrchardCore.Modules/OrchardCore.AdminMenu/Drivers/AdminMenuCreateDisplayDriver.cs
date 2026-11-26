using Microsoft.Extensions.Localization;
using OrchardCore.AdminMenu.ViewModels;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Localization;

namespace OrchardCore.AdminMenu.Drivers;

/// <summary>
/// Builds the editor of a new admin menu, whose fields are grouped in cards by their placement.
/// </summary>
internal sealed class AdminMenuCreateDisplayDriver : DisplayDriver<AdminMenuCreateViewModel>
{
    private readonly IStringLocalizerFactory _localizerFactory;

    internal readonly IStringLocalizer S;

    public AdminMenuCreateDisplayDriver(IStringLocalizer<AdminMenuCreateDisplayDriver> stringLocalizer, IStringLocalizerFactory localizerFactory)
    {
        _localizerFactory = localizerFactory;
        S = stringLocalizer;
    }

    public override IDisplayResult Edit(AdminMenuCreateViewModel model, BuildEditorContext context)
    {
        return Initialize<AdminMenuCreateViewModel>("AdminMenuCreateGeneral_Edit", m => m.Name = model.Name)
            .Location(l => l.Zone("Content", "1").Card(_localizerFactory.Localize(CommonLocalizationSources.General), "10"));
    }

    public override async Task<IDisplayResult> UpdateAsync(AdminMenuCreateViewModel model, UpdateEditorContext context)
    {
        await context.Updater.TryUpdateModelAsync(model, Prefix, m => m.Name);

        return Edit(model, context);
    }

    // The fields keep the names the page posts, which the controller validates.
    protected override void BuildPrefix(AdminMenuCreateViewModel model, string htmlFieldPrefix)
    {
        Prefix = htmlFieldPrefix ?? string.Empty;
    }
}
