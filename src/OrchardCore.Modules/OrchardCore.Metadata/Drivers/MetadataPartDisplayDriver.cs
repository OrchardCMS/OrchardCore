using System;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Metadata.Models;
using OrchardCore.Metadata.Settings;
using OrchardCore.Metadata.ViewModels;
using OrchardCore.Settings;

namespace OrchardCore.Metadata.Drivers
{
    public class MetadataPartDisplayDriver : ContentPartDisplayDriver<MetadataPart>
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly ISiteService _siteService;

        public MetadataPartDisplayDriver(
            IContentDefinitionManager contentDefinitionManager,
            ISiteService siteService)
        {
            _contentDefinitionManager = contentDefinitionManager;
            _siteService = siteService;
        }

        public override IDisplayResult Display(MetadataPart metadataPart)
        {
            return Initialize<MetadataPartViewModel>("MetadataPart", async model => await BuildViewModelAsync(model, metadataPart))
            .Location("Detail", "Content:20");
        }

        public override IDisplayResult Edit(MetadataPart metadataPart)
        {
            return Initialize<MetadataPartViewModel>("MetadataPart_Edit", model =>
            {
                var settings = GetSettings(metadataPart);

                model.MetaTitle = metadataPart.MetaTitle;
                model.MetaDescription = metadataPart.MetaDescription;
                model.OpenGraphTitle = metadataPart.OpenGraphTitle;
                model.OpenGraphDescription = metadataPart.OpenGraphDescription;
                model.OpenGraphImage = metadataPart.OpenGraphImage;
                model.TwitterCard = metadataPart.TwitterCard;
                model.OpenGraphImageAlt = metadataPart.OpenGraphImageAlt;
                model.MetadataPart = metadataPart;
                model.Settings = settings;

                return Task.CompletedTask;
            });
        }

        public override async Task<IDisplayResult> UpdateAsync(MetadataPart model, IUpdateModel updater)
        {
            var settings = GetSettings(model);

            await updater.TryUpdateModelAsync(model, Prefix);

            return Edit(model);
        }

        public MetadataPartSettings GetSettings(MetadataPart metadataPart)
        {
            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(metadataPart.ContentItem.ContentType);
            var contentTypePartDefinition = contentTypeDefinition.Parts.FirstOrDefault(x => String.Equals(x.PartDefinition.Name, nameof(MetadataPart), StringComparison.Ordinal));
            return contentTypePartDefinition.Settings.ToObject<MetadataPartSettings>();
        }

        private async Task BuildViewModelAsync(MetadataPartViewModel viewModel, MetadataPart part)
        {
            var settings = GetSettings(part);
            var siteSettings = await _siteService.GetSiteSettingsAsync();

            // If blank get title from Content items display text
            viewModel.MetaTitle = GenerateText(part.MetaTitle, part.ContentItem.DisplayText);
            viewModel.MetaDescription = part.MetaDescription;

            // If blank get title from MetaTitle
            viewModel.OpenGraphTitle = GenerateText(part.OpenGraphTitle, viewModel.MetaTitle);

            // If blank get description from MetaDescription
            viewModel.OpenGraphDescription = GenerateText(part.OpenGraphDescription, viewModel.MetaDescription);
            viewModel.OpenGraphImage = part.OpenGraphImage;
            viewModel.OpenGraphVideo = part.OpenGraphVideo;

            // If blank default to summary card
            viewModel.TwitterCard = GenerateText(part.TwitterCard, "summary");

            // If blank get image alternate text from OpenGraphTitle
            viewModel.OpenGraphImageAlt = GenerateText(part.OpenGraphImageAlt, viewModel.OpenGraphTitle);

            // Get site name from site settings
            viewModel.OpenGraphSite_name = siteSettings.SiteName;
            viewModel.OpenGraphType = GenerateText(part.OpenGraphType, "website");

            // TODO not quite sure how to get the full url of the page
            viewModel.OpenGraphUrl = siteSettings.BaseUrl;
            viewModel.MetadataPart = part;
            viewModel.Settings = settings;
        }

        private string GenerateText(string text, string alternateText)
        {
            return String.IsNullOrWhiteSpace(text) ? alternateText : text;
        }
    }
}
