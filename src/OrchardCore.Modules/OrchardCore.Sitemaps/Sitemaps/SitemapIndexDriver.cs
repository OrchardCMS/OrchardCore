using System.Linq;
using System.Threading.Tasks;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Sitemaps.Models;
using OrchardCore.Sitemaps.Services;

namespace OrchardCore.Sitemaps.Sitemaps
{
    public class SitemapIndexDriver : DisplayDriver<Sitemap, SitemapIndex>
    {
        private readonly ISitemapManager _sitemapManager;

        public SitemapIndexDriver(ISitemapManager sitemapManager)
        {
            _sitemapManager = sitemapManager;
        }

        public override IDisplayResult Display(SitemapIndex sitemap)
        {
            return Combine(
                View("SitemapIndex_SummaryAdmin", sitemap).Location("SummaryAdmin", "Content"),
                View("SitemapIndex_Thumbnail", sitemap).Location("Thumbnail", "Content")
            );
        }

        public override async Task<IDisplayResult> EditAsync(SitemapIndex sitemap, IUpdateModel updater)
        {
            var sitemaps = await _sitemapManager.ListSitemapsAsync();

            var containableSitemaps = sitemaps
                .Where(s => s.IsContainable)
                .Select(s => new ContainableSitemapEntryViewModel
                {
                    Id = s.Id,
                    Name = s.Name,
                    IsChecked = sitemap.ContainedSitemapIds.Any(id => id == s.Id)
                })
                .OrderBy(s => s.Name)
                .ToArray();


            return Initialize<SitemapIndexViewModel>("SitemapIndex_Edit", model =>
            {
                model.Name = sitemap.Name;
                model.Path = sitemap.Path;
                model.ContainableSitemaps = containableSitemaps;
                model.Sitemap = sitemap;
            }).Location("Content");
        }

        public override async Task<IDisplayResult> UpdateAsync(SitemapIndex sitemap, IUpdateModel updater)
        {
            var model = new SitemapIndexViewModel();
            if (await updater.TryUpdateModelAsync(model,
                Prefix,
                m => m.Name,
                m => m.Path,
                m => m.ContainableSitemaps))
            {
                sitemap.Name = model.Name;
                sitemap.Path = model.Path;
                sitemap.ContainedSitemapIds = model.ContainableSitemaps
                    .Where(m => m.IsChecked)
                    .Select(m => m.Id)
                    .ToArray();

                //TODO path Validation as per Autoroute. And sort leading /
            };

            return Edit(sitemap);
        }
    }
}
