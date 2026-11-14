using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Sitemaps.ViewModels;

namespace OrchardCore.Sitemaps.Drivers;

/// <summary>
/// Builds the editor of a sitemap index, new or existing, whose fields are grouped in cards by their placement.
/// </summary>
internal sealed class SitemapIndexDisplayDriver : DisplayDriver<CreateSitemapIndexViewModel>
{
    internal readonly IStringLocalizer S;

    public SitemapIndexDisplayDriver(IStringLocalizer<SitemapIndexDisplayDriver> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public override IDisplayResult Edit(CreateSitemapIndexViewModel model, BuildEditorContext context)
    {
        return Combine(
            Initialize<CreateSitemapIndexViewModel>("SitemapIndexGeneral_Edit", m => Populate(model, m))
                .Location(l => l.Zone("Content", "1").Card(S["General"], "10")),
            Initialize<CreateSitemapIndexViewModel>("SitemapIndexSitemaps_Edit", m => Populate(model, m))
                .Location(l => l.Zone("Content", "2").Card(S["Sitemaps"], "20")));
    }

    public override async Task<IDisplayResult> UpdateAsync(CreateSitemapIndexViewModel model, UpdateEditorContext context)
    {
        await context.Updater.TryUpdateModelAsync(model, Prefix,
            m => m.Name,
            m => m.Path,
            m => m.Enabled,
            m => m.ContainableSitemaps);

        return Edit(model, context);
    }

    // The fields keep the names the page posts, which the controller validates.
    protected override void BuildPrefix(CreateSitemapIndexViewModel model, string htmlFieldPrefix)
    {
        Prefix = htmlFieldPrefix ?? string.Empty;
    }

    private static void Populate(CreateSitemapIndexViewModel source, CreateSitemapIndexViewModel model)
    {
        model.Name = source.Name;
        model.Path = source.Path;
        model.Enabled = source.Enabled;
        model.ContainableSitemaps = source.ContainableSitemaps;
    }
}
