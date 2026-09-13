using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Mvc.ModelBinding;
using OrchardCore.Sitemaps.Models;
using OrchardCore.Sitemaps.Services;
using OrchardCore.Sitemaps.ViewModels;

namespace OrchardCore.Sitemaps.Drivers;

public sealed class CustomPathSitemapSourceDriver : DisplayDriver<SitemapSource, CustomPathSitemapSource>
{
    internal readonly IStringLocalizer S;

    public CustomPathSitemapSourceDriver(IStringLocalizer<CustomPathSitemapSourceDriver> localizer)
    {
        S = localizer;
    }

    public override Task<IDisplayResult> DisplayAsync(CustomPathSitemapSource sitemapSource, BuildDisplayContext context)
    {
        return CombineAsync(
            View("CustomPathSitemapSource_SummaryAdmin", sitemapSource).Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Content"),
            View("CustomPathSitemapSource_Thumbnail", sitemapSource).Location("Thumbnail", "Content")
        );
    }

    public override IDisplayResult Edit(CustomPathSitemapSource sitemapSource, BuildEditorContext context)
    {
        return Initialize<CustomPathSitemapSourceViewModel>("CustomPathSitemapSource_Edit", model =>
        {
            model.Path = sitemapSource.Path;
            model.Priority = sitemapSource.Priority;
            model.ChangeFrequency = sitemapSource.ChangeFrequency;

        }).Location("Content");
    }

    public override async Task<IDisplayResult> UpdateAsync(CustomPathSitemapSource sitemap, UpdateEditorContext context)
    {
        var model = new CustomPathSitemapSourceViewModel();

        await context.Updater.TryUpdateModelAsync(model,
                Prefix,
                m => m.Path,
                m => m.Priority,
                m => m.ChangeFrequency
            );

        sitemap.Path = model.Path;
        sitemap.Priority = model.Priority;
        sitemap.ChangeFrequency = model.ChangeFrequency;
        sitemap.LastUpdate = DateTime.Now;

        foreach (var error in SitemapSourceValidation.Validate(sitemap))
        {
            context.Updater.ModelState.AddModelError(Prefix, error.Key, S["The sitemap source setting is invalid."]);
        }

        return Edit(sitemap, context);
    }
}
