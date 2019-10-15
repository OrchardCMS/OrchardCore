using System.Threading.Tasks;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Sitemaps.Models;

namespace OrchardCore.Sitemaps.Sitemaps
{
    public class SitemapIndexDriver : DisplayDriver<Sitemap, SitemapIndex>
    {
        public override IDisplayResult Display(SitemapIndex sitemapIndex)
        {
            return Combine(
                View("SitemapIndex_SummaryAdmin", sitemapIndex).Location("SummaryAdmin", "Content"),
                View("SitemapIndex_Thumbnail", sitemapIndex).Location("Thumbnail", "Content")
            );
        }

        public override IDisplayResult Edit(SitemapIndex sitemapIndex)
        {
            return Initialize<SitemapIndexViewModel>("SitemapIndex_Edit", model =>
            {
                model.Name = sitemapIndex.Name;
                model.Path = sitemapIndex.Path;
                model.SitemapNode = sitemapIndex;
            }).Location("Content");
        }

        public override async Task<IDisplayResult> UpdateAsync(SitemapIndex sitemapIndex, IUpdateModel updater)
        {
            var model = new SitemapIndexViewModel();
            if (await updater.TryUpdateModelAsync(model, Prefix, x => x.Name, x => x.Path))
            {
                sitemapIndex.Name = model.Name;
                sitemapIndex.Path = model.Path;
                //TODO path Validation as per Autoroute. And sort leading /
            };

            return Edit(sitemapIndex);
        }
    }
}
