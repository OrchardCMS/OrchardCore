using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Sitemaps.Models;
using OrchardCore.Sitemaps.ViewModels;

namespace OrchardCore.Sitemaps.Drivers;

public sealed class SitemapPartDisplayDriver : ContentPartDisplayDriver<SitemapPart>
{
    internal readonly IStringLocalizer S;

    public SitemapPartDisplayDriver(IStringLocalizer<SitemapPartDisplayDriver> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public override IDisplayResult Edit(SitemapPart part, BuildPartEditorContext context)
    {
        return Initialize<SitemapPartViewModel>("SitemapPart_Edit", m => BuildViewModel(m, part))
            .Location(l => l.Zone("Parts", "5").Tab("SEO").Card(S["Sitemap"], "60"));
    }

    public override async Task<IDisplayResult> UpdateAsync(SitemapPart model, UpdatePartEditorContext context)
    {
        var viewModel = new SitemapPartViewModel();

        await context.Updater.TryUpdateModelAsync(viewModel,
            Prefix,
            t => t.OverrideSitemapConfig,
            t => t.ChangeFrequency,
            t => t.Exclude,
            t => t.Priority);

        model.OverrideSitemapConfig = viewModel.OverrideSitemapConfig;
        model.ChangeFrequency = viewModel.ChangeFrequency;
        model.Exclude = viewModel.Exclude;
        model.Priority = viewModel.Priority;

        return Edit(model, context);
    }


    private static void BuildViewModel(SitemapPartViewModel model, SitemapPart part)
    {
        model.OverrideSitemapConfig = part.OverrideSitemapConfig;
        model.ChangeFrequency = part.ChangeFrequency;
        model.Exclude = part.Exclude;
        model.Priority = part.Priority;
        model.SitemapPart = part;
    }
}
