using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Mvc.ModelBinding;
using OrchardCore.Sitemaps.Models;
using OrchardCore.Sitemaps.Services;
using OrchardCore.Sitemaps.ViewModels;

namespace OrchardCore.Sitemaps.Drivers
{
    public class CustomPathSitemapSourceDriver : DisplayDriver<SitemapSource, CustomPathSitemapSource>
    {
        private readonly IStringLocalizer S;

        public CustomPathSitemapSourceDriver(
            IStringLocalizer<CustomPathSitemapSourceDriver> localizer
        )
        {
            S = localizer;
        }
        
        public override IDisplayResult Display(CustomPathSitemapSource sitemapSource)
        {
            return Combine(
                View("CustomPathSitemapSource_SummaryAdmin", sitemapSource).Location("SummaryAdmin", "Content"),
                View("CustomPathSitemapSource_Thumbnail", sitemapSource).Location("Thumbnail", "Content")
            );
        }

        public override IDisplayResult Edit(CustomPathSitemapSource sitemapSource, IUpdateModel updater)
        {
            return Initialize<CustomPathSitemapSourceViewModel>("CustomPathSitemapSource_Edit", model =>
            {
                model.Url = sitemapSource.Url;
                model.Priority = sitemapSource.Priority;
                model.ChangeFrequency = sitemapSource.ChangeFrequency;
                
            }).Location("Content");
        }

        public override async Task<IDisplayResult> UpdateAsync(CustomPathSitemapSource sitemap, UpdateEditorContext context)
        {
            var model = new CustomPathSitemapSourceViewModel();

            if (await context.Updater.TryUpdateModelAsync(model,
                    Prefix,
                    m => m.Url,
                    m => m.Priority,
                    m => m.ChangeFrequency
                ))
            {
                sitemap.Url = model.Url;
                sitemap.Priority = model.Priority;
                sitemap.ChangeFrequency = model.ChangeFrequency;     
                sitemap.LastUpdate = DateTime.Now;

                context.Updater.ModelState.BindValidationResults(Prefix, sitemap.ValidateUrlFieldValue(S));           
            };

            return Edit(sitemap, context.Updater);
        }
    }
}
