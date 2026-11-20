using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Sitemaps.ViewModels;

namespace OrchardCore.Sitemaps.Drivers;

/// <summary>
/// Builds the editor of an existing sitemap, whose fields are grouped in cards by their placement.
/// </summary>
internal sealed class EditSitemapDisplayDriver : DisplayDriver<EditSitemapViewModel>
{
    internal readonly IStringLocalizer S;

    public EditSitemapDisplayDriver(IStringLocalizer<EditSitemapDisplayDriver> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public override IDisplayResult Edit(EditSitemapViewModel model, BuildEditorContext context)
    {
        return Initialize<EditSitemapViewModel>("SitemapEditGeneral_Edit", m =>
        {
            m.SitemapId = model.SitemapId;
            m.Name = model.Name;
            m.Path = model.Path;
            m.Enabled = model.Enabled;
        }).Location(l => l.Zone("Content", "1").Card(S["General"], "10"));
    }

    public override async Task<IDisplayResult> UpdateAsync(EditSitemapViewModel model, UpdateEditorContext context)
    {
        await context.Updater.TryUpdateModelAsync(model, Prefix, m => m.Name, m => m.Path, m => m.Enabled);

        return Edit(model, context);
    }

    // The fields keep the names the page posts, which the controller validates.
    protected override void BuildPrefix(EditSitemapViewModel model, string htmlFieldPrefix)
    {
        Prefix = htmlFieldPrefix ?? string.Empty;
    }
}
