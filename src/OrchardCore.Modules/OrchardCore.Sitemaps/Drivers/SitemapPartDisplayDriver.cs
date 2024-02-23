using System.Threading.Tasks;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Sitemaps.Models;
using OrchardCore.Sitemaps.ViewModels;

namespace OrchardCore.Sitemaps.Drivers
{
    public class SitemapPartDisplayDriver : ContentPartDisplayDriver<SitemapPart>
    {
        public override IDisplayResult Edit(SitemapPart part)
        {
            return Initialize<SitemapPartViewModel>("SitemapPart_Edit", m => BuildViewModel(m, part))
                .Location("Parts#SEO:5");
        }

        public override async Task<IDisplayResult> UpdateAsync(SitemapPart model, IUpdateModel updater)
        {
            var viewModel = new SitemapPartViewModel();

            if (await updater.TryUpdateModelAsync(viewModel,
                Prefix,
                t => t.OverrideSitemapConfig,
                t => t.ChangeFrequency,
                t => t.Exclude,
                t => t.Priority
                ))
            {
                model.OverrideSitemapConfig = viewModel.OverrideSitemapConfig;
                model.ChangeFrequency = viewModel.ChangeFrequency;
                model.Exclude = viewModel.Exclude;
                model.Priority = viewModel.Priority;
            }

            return Edit(model);
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
}
