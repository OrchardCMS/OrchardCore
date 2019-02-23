using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        private readonly IContentManager _contentManager;
        private readonly IServiceProvider _serviceProvider;
        private readonly IContentDefinitionManager _contentDefinitionManager;

        public SitemapPartDisplay(
            IContentManager contentManager,
            IServiceProvider serviceProvider,
            IContentDefinitionManager contentDefinitionManager
            )
        {
            _contentManager = contentManager;
            _serviceProvider = serviceProvider;
            _contentDefinitionManager = contentDefinitionManager;
        }

        public override IDisplayResult Edit(SitemapPart sitemapPart)
        {
            return Initialize<SitemapPartViewModel>("SitemapPart_Edit", m =>
            {
                BuildViewModel(m, sitemapPart);
            });
        }

        private SitemapPartSettings GetSettings(SitemapPart sitemapPart)
        {
            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(sitemapPart.ContentItem.ContentType);
            var contentTypePartDefinition = contentTypeDefinition.Parts.FirstOrDefault(x => String.Equals(x.PartDefinition.Name, nameof(SitemapPart), StringComparison.Ordinal));
            return contentTypePartDefinition.Settings.ToObject<SitemapPartSettings>();
        }

        public override async Task<IDisplayResult> UpdateAsync(SitemapPart model, IUpdateModel updater)
        {
            await updater.TryUpdateModelAsync(model, Prefix, t => t.ChangeFrequency, t => t.Exclude, t => t.Priority);

            //probably don't do this, probably check the settings at generation time.
            //var settings = GetSettings(model);
            
            //if (settings.ExcludePart)
            //{
            //    model.Exclude = true;
            //}

            return Edit(model);
        }


        private void BuildViewModel(SitemapPartViewModel model, SitemapPart part)
        {
            var settings = GetSettings(part);
            model.ChangeFrequency = part.ChangeFrequency;
            model.Exclude = part.Exclude;
            model.Priority = part.Priority;
            model.SitemapPart = part;
            model.Settings = settings;

            if (part.ContentItem.Id == 0 && settings.ExcludeByDefault)
            {
                model.Exclude = true;
            }
        }
    }
}
