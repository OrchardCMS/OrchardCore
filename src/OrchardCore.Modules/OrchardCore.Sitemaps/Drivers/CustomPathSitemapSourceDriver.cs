using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Mvc.ModelBinding;
using OrchardCore.Sitemaps.Models;
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
            View("CustomPathSitemapSource_SummaryAdmin", sitemapSource).Location("SummaryAdmin", "Content"),
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

        if (sitemap.Path?.IndexOfAny(CustomPathSitemapSource.InvalidCharactersForPath) > -1 || sitemap.Path?.IndexOf(' ') > -1 || sitemap.Path?.IndexOf("//") > -1)
        {
            var invalidCharactersForMessage = string.Join(", ", CustomPathSitemapSource.InvalidCharactersForPath.Select(c => $"\"{c}\""));
            context.Updater.ModelState.AddModelError(Prefix, sitemap.Path, S["Please do not use any of the following characters in your permalink: {0}. No spaces, or consecutive slashes, are allowed (please use dashes or underscores instead).", invalidCharactersForMessage]);
        }

        if (sitemap.Path?.Length > CustomPathSitemapSource.MaxPathLength)
        {
            context.Updater.ModelState.AddModelError(Prefix, sitemap.Path, S["Your path is too long. The path can only be up to {0} characters.", CustomPathSitemapSource.MaxPathLength]);
        }

        return Edit(sitemap, context);
    }
}
