using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Localization;
using OrchardCore.Sitemaps.ViewModels;

namespace OrchardCore.Sitemaps.Drivers;

/// <summary>
/// Builds the editor of a new sitemap, whose fields are grouped in cards by their placement.
/// </summary>
internal sealed class CreateSitemapDisplayDriver : DisplayDriver<CreateSitemapViewModel>
{
    private readonly IStringLocalizerFactory _localizerFactory;

    internal readonly IStringLocalizer S;

    public CreateSitemapDisplayDriver(IStringLocalizer<CreateSitemapDisplayDriver> stringLocalizer, IStringLocalizerFactory localizerFactory)
    {
        _localizerFactory = localizerFactory;
        S = stringLocalizer;
    }

    public override IDisplayResult Edit(CreateSitemapViewModel model, BuildEditorContext context)
    {
        return Initialize<CreateSitemapViewModel>("SitemapCreateGeneral_Edit", m =>
        {
            m.Name = model.Name;
            m.Path = model.Path;
            m.Enabled = model.Enabled;
        }).Location(l => l.Zone("Content", "1").Card(_localizerFactory.Localize(CommonLocalizationSources.General), "10"));
    }

    public override async Task<IDisplayResult> UpdateAsync(CreateSitemapViewModel model, UpdateEditorContext context)
    {
        await context.Updater.TryUpdateModelAsync(model, Prefix, m => m.Name, m => m.Path, m => m.Enabled);

        return Edit(model, context);
    }

    // The fields keep the names the page posts, which the controller validates.
    protected override void BuildPrefix(CreateSitemapViewModel model, string htmlFieldPrefix)
    {
        Prefix = htmlFieldPrefix ?? string.Empty;
    }
}
