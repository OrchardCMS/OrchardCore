using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Sitemaps.Models;
using OrchardCore.Sitemaps.Services;
using OrchardCore.Sitemaps.ViewModels;

namespace OrchardCore.Sitemaps.Drivers
{
    public class CustomUrlsSitemapSourceDriver : DisplayDriver<SitemapSource, CustomUrlSitemapSource>
    {
        public override IDisplayResult Display(CustomUrlSitemapSource sitemapSource)
        {
            return Combine(
                View("CustomUrlSitemapSource_SummaryAdmin", sitemapSource).Location("SummaryAdmin", "Content"),
                View("CustomUrlSitemapSource_Thumbnail", sitemapSource).Location("Thumbnail", "Content")
            );
        }

        public override IDisplayResult Edit(CustomUrlSitemapSource sitemapSource, IUpdateModel updater)
        {
            

            return Initialize<CustomUrlSitemapSourceViewModel>("CustomUrlSitemapSource_Edit", model =>
            {
                model.Url = sitemapSource.Url;
                model.Priority = sitemapSource.Priority;
                model.ChangeFrequency = sitemapSource.ChangeFrequency;
                
            }).Location("Content");
        }

        public override async Task<IDisplayResult> UpdateAsync(CustomUrlSitemapSource sitemap, UpdateEditorContext context)
        {
            var model = new CustomUrlSitemapSourceViewModel();

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
            };

            return Edit(sitemap, context.Updater);
        }
    }
}
