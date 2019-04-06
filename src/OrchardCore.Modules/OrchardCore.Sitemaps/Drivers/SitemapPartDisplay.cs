using System;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Sitemaps.Models;
using OrchardCore.Sitemaps.ViewModels;

namespace OrchardCore.Sitemaps.Drivers
{
    public class SitemapPartDisplay : ContentPartDisplayDriver<SitemapPart>
    {
        public SitemapPartDisplay() { }

        public override IDisplayResult Edit(SitemapPart part)
        {
            //TODO when tabs implemented locate in tab
            return Initialize<SitemapPartViewModel>("SitemapPart_Edit", m =>
            {
                BuildViewModel(m, part);
            });
        }

        public override async Task<IDisplayResult> UpdateAsync(SitemapPart model, IUpdateModel updater)
        {
            await updater.TryUpdateModelAsync(model, Prefix, t => t.OverrideSitemapSetConfig, t => t.ChangeFrequency, t => t.Exclude, t => t.Priority);
            return Edit(model);
        }


        private void BuildViewModel(SitemapPartViewModel model, SitemapPart part)
        {
            model.OverrideSitemapSetConfig = part.OverrideSitemapSetConfig;
            model.ChangeFrequency = part.ChangeFrequency;
            model.Exclude = part.Exclude;
            model.Priority = part.Priority;
            model.SitemapPart = part;
        }
    }
}
